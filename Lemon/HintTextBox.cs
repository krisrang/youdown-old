using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Lemon
{
    public class HintTextBox : TextBox
    {
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText",
                                        typeof(string),
                                        typeof(HintTextBox),
                                        new PropertyMetadata(String.Empty));

        [Description("Gets or sets the hint text.")]
        [Category("Common Properties")]
        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        public static readonly DependencyProperty HintVisibleProperty =
            DependencyProperty.Register("HintVisible",
                                        typeof(bool),
                                        typeof(HintTextBox),
                                        new PropertyMetadata(false));

        [Bindable(false)]
        public bool HintVisible
        {
            get { return (bool)GetValue(HintVisibleProperty); }
            set { SetValue(HintVisibleProperty, value); }
        }

        public HintTextBox()
        {
            TextChanged += HintTextBoxTextChanged;
            GotFocus += HintTextBoxGotFocus;
            LostFocus += HintTextBoxLostFocus;

            HintVisible = String.IsNullOrWhiteSpace(Text);
        }

        void HintTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            HintVisible = String.IsNullOrWhiteSpace(Text);
        }

        private void HintTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            HintVisible = false;
        }

        private void HintTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            HintVisible = String.IsNullOrWhiteSpace(Text);
        }

        static HintTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintTextBox), new FrameworkPropertyMetadata(typeof(HintTextBox)));
        }
    }
}
