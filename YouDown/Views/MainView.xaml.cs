using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Lemon;
using Lemon.Utils;
using MEFedMVVM.Services.Contracts;
using Microsoft.Win32;

namespace YouDown.Views
{
    [Export(typeof(IShell))]
    public partial class MainView : LayeredWindow, IShell
    {
        private Storyboard fadeStoryBoard;
        private string currentScreen;

        private IMediator _mediator;

        [ImportingConstructor]
        public MainView(IMediator mediator)
        {
            InitializeComponent();

            if (!Features.LayeredWindowAccelerated)
            {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                Background = System.Windows.Media.Brushes.Transparent;

                LayoutRoot.Margin = new Thickness(10);
                LayoutRoot.Effect = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 0 };

                Deactivated += (s, a) => ((DropShadowEffect)LayoutRoot.Effect).Opacity = 0.5;
                Activated += (s, a) => ((DropShadowEffect)LayoutRoot.Effect).Opacity = 1;
            }

            // Wire the events
            Deactivated += (s, a) => BrandingBox.Opacity = 0.5;
            Activated += (s, a) => BrandingBox.Opacity = 1;

            MinimiseButton.Click += (s, a) => WindowState = WindowState.Minimized;
            CloseBtn.Click += (s, a) => Close();

            // Enable the window resize elements
            TopBorder.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.Top);
            BottomBorder.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.Bottom);
            LeftBorder.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.Left);
            RightBorder.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.Right);

            TopLeftCorner.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.TopLeft);
            TopRightCorner.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.TopRight);
            BottomLeftCorner.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.BottomLeft);
            BottomRightCorner.MouseLeftButtonDown += (s, a) => ResizeWindow(ResizeDirection.BottomRight);

            _mediator = mediator;

            fadeStoryBoard = FindResource("MetroFadeSlide") as Storyboard;
            OpenScreen("Download", false);
        }

        public void OpenScreen(string screenName)
        {
            OpenScreen(screenName, true);
        }

        public void OpenScreen(string screenName, bool animate)
        {
            var screen = Resolver.GetInstance<IScreen>(screenName);

            if (screen != null && screenName != currentScreen)
            {
                _mediator.NotifyColleagues(MediatorMessages.ScreenChange, screenName);

                currentScreen = screenName;
                ChildControl.Content = screen;

                if (animate)
                    fadeStoryBoard.Begin(ChildControl);
            }
        }
    }
}