namespace Lemon
{
    public class ObservableProperty<T> : NotifyPropertyChangedBase
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            set { _value = value; NotifyOfPropertyChange(() => Value); }
        }

        public static implicit operator T(ObservableProperty<T> val)
        {
            return val.Value;
        }
    }
}
