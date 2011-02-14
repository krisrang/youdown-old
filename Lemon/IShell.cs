namespace Lemon
{
    public interface IShell
    {
        object DataContext { get; set; }
        void Close();
        void OpenScreen(string screen);
    }
}