using System;
using System.Windows;

namespace Lemon
{
    public static class Execute
    {
        private static Action<Action> _executor = action => action();

        public static void InitializeWithDispatcher()
        {
# if SILVERLIGHT
            var dispatcher = Deployment.Current.Dispatcher;
# else
            var dispatcher = Application.Current.Dispatcher;
#endif

            _executor = action => {
                if (dispatcher.CheckAccess())
                    action();
                else dispatcher.BeginInvoke(action);
            };
        }

        public static void OnUIThread(this Action action)
        {
            _executor(action);
        }
    }
}