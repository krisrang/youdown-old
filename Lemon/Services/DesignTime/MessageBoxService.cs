using Lemon.Services.Contracts;
using MEFedMVVM.ViewModelLocator;

namespace Lemon.Services.DesignTime
{
    [ExportService(ServiceType.DesignTime, typeof(IMessageBox))]
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
            return MessageBoxResult.None;
        }
    }
}
