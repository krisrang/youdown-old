using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Lemon
{
    public class InvalidateListBoxItemAfterLoadBehavior : Behavior<FrameworkElement>
    {
        public InvalidateListBoxItemAfterLoadBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        private void AssociatedObjectLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameworkElement ancestor = this.AssociatedObject;

            while (ancestor != null)
            {
                if (ancestor is ListBoxItem)
                {
                    ancestor.InvalidateArrange();
                    return;
                }
                ancestor = VisualTreeHelper.GetParent(ancestor) as FrameworkElement;
            }
        }
    }
}
