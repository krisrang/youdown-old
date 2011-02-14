using System;
using System.Windows.Media;

namespace Lemon.Utils
{
    public static class Features
    {
        public static bool LayeredWindowAccelerated { get { return Win32.IsDwmAvailable() && RenderCapability.Tier >> 16 == 2; } }
        public static bool VistaOrLater { get { return Environment.OSVersion.Version.Major > 5; } }
    }
}
