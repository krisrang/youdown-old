using System;
using System.Windows;

namespace Lemon
{
    public static class Bootstrapper
    {
        public static UIElement Initialize()
        {
            try
            {
                Execute.InitializeWithDispatcher();
                var shell = Resolver.GetInstance<IShell>();

                return (UIElement)shell;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot initialize Lemon", ex);
            }
        }
    }
}