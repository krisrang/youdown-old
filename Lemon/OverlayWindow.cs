using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lemon
{
    [TemplateVisualState(GroupName = "WindowStates", Name = "Open"),
    TemplateVisualState(GroupName = "WindowStates", Name = "Closed"),
    TemplatePart(Name = "Chrome", Type = typeof(FrameworkElement)),
    TemplatePart(Name = "CloseButton", Type = typeof(ButtonBase)),
    TemplatePart(Name = "ContentPresenter", Type = typeof(FrameworkElement)),
    TemplatePart(Name = "ContentRoot", Type = typeof(FrameworkElement)),
    TemplatePart(Name = "Overlay", Type = typeof(Panel)),
    TemplatePart(Name = "Root", Type = typeof(FrameworkElement))]
    public class OverlayWindow : ContentControl, IOverlay
    {
        # region Properties
        public static readonly DependencyProperty OverlayBrushProperty =
            DependencyProperty.Register("OverlayBrush",
                                        typeof(Brush),
                                        typeof(OverlayWindow),
                                        new PropertyMetadata());

        public static readonly DependencyProperty OverlayOpacityProperty =
            DependencyProperty.Register("OverlayOpacity",
                                        typeof(double),
                                        typeof(OverlayWindow),
                                        new PropertyMetadata());

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                                        typeof(string),
                                        typeof(OverlayWindow),
                                        new PropertyMetadata());

        public static readonly DependencyProperty HasChromeProperty =
            DependencyProperty.Register("HasChrome",
                                        typeof(bool),
                                        typeof(OverlayWindow),
                                        new PropertyMetadata(true, HasChromeChanged));

        public static readonly DependencyProperty HasCloseButtonProperty =
            DependencyProperty.Register("HasCloseButton",
                                        typeof(bool),
                                        typeof(OverlayWindow),
                                        new PropertyMetadata(true, HasCloseButtonChanged));

        [Description("Gets or sets brush of the overlay.")]
        [Category("Brushes")]
        public Brush OverlayBrush
        {
            get { return (Brush)GetValue(OverlayBrushProperty); }
            set { SetValue(OverlayBrushProperty, value); }
        }

        [Description("Gets or sets opacity of the overlay.")]
        [Category("Appearance")]
        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }

        [Description("Gets or sets the overlay window title.")]
        [Category("Common Properties")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        [Description("Gets or sets whether the chrome is visible.")]
        [Category("Appearance")]
        public bool HasChrome
        {
            get { return (bool)GetValue(HasChromeProperty); }
            set { SetValue(HasChromeProperty, value); }
        }

        [Description("Gets or sets whether the close button is visible.")]
        [Category("Appearance")]
        public bool HasCloseButton
        {
            get { return (bool)GetValue(HasCloseButtonProperty); }
            set { SetValue(HasCloseButtonProperty, value); }
        }

        public Panel Overlay { get; set; }
        public FrameworkElement Root { get; set; }
        public FrameworkElement ContentRoot { get; set; }
        public ButtonBase CloseButton { get; set; }

        public bool? DialogResult
        {
            get
            {
                return _dialogresult;
            }

            set
            {
                if (_dialogresult != value)
                {
                    _dialogresult = value;
                    Close();
                }
            }
        }
        #endregion

        #region Events and privates
        private double _desiredContentWidth;
        private double _desiredContentHeight;
        private Thickness _desiredMargin;

        private double _verticalOffset;
        private double _horizontalOffset;

        private bool _isClosing;
        private bool _isOpen;
        private bool _isAppExit;

        public event EventHandler Closed;
        public event EventHandler<CancelEventArgs> Closing;

        private Storyboard _opened;
        private Storyboard _closed;

        private bool? _dialogresult;

        private bool IsOpen { get; set; }

        private static Panel TargetVisual
        {
            get
            {
                return Application.Current.MainWindow == null ? null : (Application.Current.MainWindow.Content as Panel);
            }
        }
        #endregion

        public OverlayWindow()
        {
            //DefaultStyleKey = typeof (OverlayWindow);
        }

        static OverlayWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (OverlayWindow),
                                                     new FrameworkPropertyMetadata(typeof (OverlayWindow)));
        }

        public void Show()
        {
            _verticalOffset = 0;
            _horizontalOffset = 0;

            SubscribeToEvents();
            SubscribeToTemplatePartEvents();
            SubscribeToStoryBoardEvents();

            try
            {
                TargetVisual.Children.Add(this);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("", ex);
            }

            if (Application.Current.MainWindow != null)
            {
                IsOpen = true;
                _dialogresult = null;
            }
        }

        public void Close()
        {
            var e = new CancelEventArgs();
            OnClosing(e);

            // On ApplicationExit, close() cannot be cancelled
            if (!e.Cancel || _isAppExit)
            {
                // Close Popup
                if (IsOpen)
                {
                    if (_closed != null)
                    {
                        // Popup will be closed when the storyboard ends
                        _isClosing = true;
                        try
                        {
                            var sb = GetVisualStateStoryboard("WindowStates", "Closed");
                            sb.Completed += (s, args) =>
                                                {
                                                    OnClosed(EventArgs.Empty);
                                                    UnSubscribeFromEvents();
                                                    UnsubscribeFromTemplatePartEvents();

                                                    if (Application.Current.MainWindow != null)
                                                    {
                                                        Application.Current.MainWindow.GotFocus -= MainWindowGotFocus;
                                                    }

                                                    TargetVisual.Children.Remove(this);
                                                };
                            UpdateStates(true);
                        }
                        finally
                        {
                            _isClosing = false;
                        }
                    }
                    else
                    {
                        TargetVisual.Children.Remove(this);
                    }

                    IsOpen = false;

                    if (!_dialogresult.HasValue)
                    {
                        // If close action is not happening because of DialogResult property change action,
                        // Dialogresult is always false:
                        _dialogresult = false;
                    }
                }
            }
            else
            {
                // If the Close is cancelled, DialogResult should always be NULL:
                _dialogresult = null;
            }
        }

        public override void OnApplyTemplate()
        {
            UnsubscribeFromTemplatePartEvents();

            base.OnApplyTemplate();

            if (_closed != null)
            {
                _closed.Completed -= ClosingCompleted;
            }

            if (_opened != null)
            {
                _opened.Completed -= OpeningCompleted;
            }

            Root = Template.FindName("Root", this) as FrameworkElement;

            if (Root != null)
            {
                var groups = VisualStateManager.GetVisualStateGroups(Root);

                if (groups != null)
                {
                    var states = (from VisualStateGroup vsg in groups where vsg.Name == "WindowStates" select vsg.States).FirstOrDefault();

                    if (states != null)
                    {
                        foreach (VisualState state in states)
                        {
                            if (state.Name == "Closed")
                            {
                                _closed = state.Storyboard;
                            }

                            if (state.Name == "Open")
                            {
                                _opened = state.Storyboard;
                            }
                        }
                    }
                }
            }

            var chrome = Template.FindName("Chrome", this) as FrameworkElement;

            if (chrome != null)
            {
                chrome.Visibility = HasChrome ? Visibility.Visible : Visibility.Collapsed;
            }

            var button = Template.FindName("CloseButton", this) as Button;

            if (button != null)
            {
                button.Visibility = HasCloseButton ? Visibility.Visible : Visibility.Collapsed;
            }

            Overlay = Template.FindName("Overlay", this) as Panel;
            ContentRoot = Template.FindName("ContentRoot", this) as FrameworkElement;
            CloseButton = Template.FindName("CloseButton", this) as ButtonBase;

            SubscribeToTemplatePartEvents();
            SubscribeToStoryBoardEvents();
            _desiredMargin = Margin;
            Margin = new Thickness(0);

            // Update overlay size)
            if (IsOpen)
            {
                _desiredContentHeight = Height;
                _desiredContentWidth = Width;
                UpdateOverlaySize();
                UpdateStates(false);
            }
        }

        protected static void HasChromeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var overlayWindow = obj as OverlayWindow;

            if (overlayWindow != null)
            {
                var chrome = overlayWindow.Template.FindName("Chrome", overlayWindow) as FrameworkElement;

                if (chrome != null)
                {
                    chrome.Visibility = (bool)args.NewValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                }
            }
        }

        protected static void HasCloseButtonChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var overlayWindow = obj as OverlayWindow;

            if (overlayWindow != null)
            {
                var button = overlayWindow.Template.FindName("CloseButton", overlayWindow) as Button;

                if (button != null)
                {
                    button.Visibility = (bool)args.NewValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                }
            }
        }

        protected virtual void OnOpened()
        {
            _isOpen = true;

            if (Overlay != null)
            {
                Overlay.Opacity = OverlayOpacity;
                Overlay.Background = OverlayBrush;
            }

            if (!Focus())
            {
                // If the Focus() fails it means there is no focusable element in the 
                // FloatableWindow. In this case we set IsTabStop to true to have the keyboard functionality
                IsTabStop = true;
                Focus();
            }
        }

        protected virtual void OnClosed(EventArgs e)
        {
            var handler = Closed;

            if (null != handler)
            {
                handler(this, e);
            }

            _isOpen = false;
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            var handler = Closing;

            if (null != handler)
            {
                handler(this, e);
            }
        }

        private void SubscribeToEvents()
        {
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.Content != null)
            {
                Application.Current.Exit += ApplicationExit;
                Application.Current.MainWindow.SizeChanged += AppResized;
                Application.Current.MainWindow.LocationChanged += AppLocationChanged;
            }

            SizeChanged += OverlayWindowSizeChanged;
        }

        private void AppLocationChanged(object sender, EventArgs e)
        {
            
        }

        private void OverlayWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Overlay != null)
            {
                if (e.NewSize.Height != Overlay.Height)
                {
                    _desiredContentHeight = e.NewSize.Height;
                }

                if (e.NewSize.Width != Overlay.Width)
                {
                    _desiredContentWidth = e.NewSize.Width;
                }
            }

            if (IsOpen)
            {
                UpdateOverlaySize();
            }
        }

        internal void ApplicationExit(object sender, ExitEventArgs e)
        {
            if (IsOpen)
            {
                _isAppExit = true;
                try
                {
                    Close();
                }
                finally
                {
                    _isAppExit = false;
                }
            }
        }

        private void AppResized(object sender, EventArgs e)
        {
            UpdateOverlaySize();
        }

        private void SubscribeToStoryBoardEvents()
        {
            if (_closed != null)
            {
                _closed.Completed += ClosingCompleted;
            }

            if (_opened != null)
            {
                _opened.Completed += OpeningCompleted;
            }
        }

        private void OpeningCompleted(object sender, EventArgs e)
        {
            if (_opened != null)
            {
                _opened.Completed -= OpeningCompleted;
            }

            OnOpened();
        }

        private void ClosingCompleted(object sender, EventArgs e)
        {
            IsOpen = false;

            if (_closed != null)
            {
                _closed.Completed -= ClosingCompleted;
            }
        }

        private void SubscribeToTemplatePartEvents()
        {
            if (CloseButton != null)
            {
                CloseButton.Click += CloseButtonClick;
            }

            if (Overlay != null)
            {
                Overlay.MouseDown += OverlayMouseDown;
            }   
        }

        private void OverlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.DragMove();
            }
        }

        private void UnSubscribeFromEvents()
        {
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.Content != null)
            {
                Application.Current.Exit -= ApplicationExit;
                Application.Current.MainWindow.SizeChanged -= AppResized;
                Application.Current.MainWindow.LocationChanged -= AppLocationChanged;
            }

            SizeChanged -= OverlayWindowSizeChanged;
        }

        private void UnsubscribeFromTemplatePartEvents()
        {
            if (CloseButton != null)
            {
                CloseButton.Click -= CloseButtonClick;
            }

            if (Overlay != null)
            {
                Overlay.MouseDown -= OverlayMouseDown;
            }  
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindowGotFocus(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        private void UpdateOverlaySize()
        {
            if (Overlay != null && Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.Content != null)
            {
                Height = Application.Current.MainWindow.ActualHeight;
                Width = Application.Current.MainWindow.ActualWidth;
                Overlay.Height = Height;
                Overlay.Width = Width;

                if (ContentRoot != null)
                {
                    ContentRoot.Width = _desiredContentWidth;
                    ContentRoot.Height = _desiredContentHeight;
                    ContentRoot.Margin = _desiredMargin;
                }
            }
        }

        private void UpdateStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, _isClosing ? "Closed" : "Open", useTransitions);
        }

        private Storyboard GetVisualStateStoryboard(string visualStateGroupName, string visualStateName)
        {
            foreach (VisualStateGroup g in VisualStateManager.GetVisualStateGroups((FrameworkElement)this.ContentRoot.Parent))
            {
                if (g.Name != visualStateGroupName) continue;

                foreach (var s in from VisualState s in g.States where s.Name == visualStateName select s)
                {
                    return s.Storyboard;
                }
            }
            return null;
        }
    }
}
