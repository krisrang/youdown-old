namespace Lemon
{
    public interface IOverlay
    {
        object DataContext { get; set; }
        void Show();
        void Close();
    }
}