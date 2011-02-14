using System;
using System.Windows;
using System.Windows.Controls;

namespace Lemon
{
    public static class VisualStates
    {
        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.RegisterAttached("CurrentState", typeof(String), typeof(VisualStates), new PropertyMetadata(TransitionToState));

        public static string GetCurrentState(DependencyObject obj)
        {
            return (string)obj.GetValue(CurrentStateProperty);
        }

        public static void SetCurrentState(DependencyObject obj, string value)
        {
            obj.SetValue(CurrentStateProperty, value);
        }

        private static void TransitionToState(object sender, DependencyPropertyChangedEventArgs args)
        {
            var c = sender as Control;

            if (c != null)
            {
                VisualStateManager.GoToState(c, (string)args.NewValue, true);
            }
            else
            {
                throw new ArgumentException("CurrentState is only supported on the Control type");
            }
        }
    }
}
