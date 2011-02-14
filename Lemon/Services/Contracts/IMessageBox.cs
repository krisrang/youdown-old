namespace Lemon.Services.Contracts
{
    public interface IMessageBox
    {
        MessageBoxResult Show(string text);
        MessageBoxResult Show(string text, string caption);
        MessageBoxResult Show(string text, string caption, MessageBoxButton buttons);
        MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }

    public enum MessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }

    public enum MessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
    }

    public enum MessageBoxImage
    {
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Asterisk = 64,
        Information = 64,
    }
}