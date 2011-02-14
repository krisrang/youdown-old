using System;
using System.Reflection;

namespace Lemon.Utils
{
    public static class App
    {
        public static Version GetVersion()
        {
            return Assembly.GetCallingAssembly().GetName().Version;
        }
    }
}
