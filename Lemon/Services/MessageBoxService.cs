using Lemon.Services.Contracts;
using MEFedMVVM.ViewModelLocator;

namespace Lemon.Services
{
    [ExportService(ServiceType.Runtime, typeof(IMessageBox))]
    public class MessageBoxService : IMessageBox
    {
        public MessageBoxResult Show(string text)
        {
            return Show(text, null, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public MessageBoxResult Show(string text, string caption)
        {
            return Show(text, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public MessageBoxResult Show(string text, string caption, MessageBoxButton buttons)
        {
            return Show(text, caption, buttons, MessageBoxImage.None);
        }

        public MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return (MessageBoxResult) System.Windows.MessageBox.Show(text, caption, (System.Windows.MessageBoxButton)buttons, (System.Windows.MessageBoxImage)icon);
        }
    }
}
