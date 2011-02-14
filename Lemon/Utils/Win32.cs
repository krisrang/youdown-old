using System;
using System.Runtime.InteropServices;

namespace Lemon.Utils
{
    public static class Win32
    {
        // Methods
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);
        public static int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins)
        {
            IntPtr hModule = LoadLibrary("dwmapi");
            if (hModule == IntPtr.Zero)
            {
                return 0;
            }
            IntPtr procAddress = GetProcAddress(hModule, "DwmExtendFrameIntoClientArea");
            if (procAddress == IntPtr.Zero)
            {
                return 0;
            }
            DwmExtendFrameIntoClientAreaDelegate delegateForFunctionPointer = (DwmExtendFrameIntoClientAreaDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DwmExtendFrameIntoClientAreaDelegate));
            return delegateForFunctionPointer(hwnd, ref margins);
        }

        public static int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute, ref IntPtr pvAttribute, uint cbAttribute)
        {
            IntPtr hModule = LoadLibrary("dwmapi");
            if (hModule == IntPtr.Zero)
            {
                return 0;
            }
            IntPtr procAddress = GetProcAddress(hModule, "DwmSetWindowAttribute");
            if (procAddress == IntPtr.Zero)
            {
                return 0;
            }
            DwmSetWindowAttributeDelegate delegateForFunctionPointer = (DwmSetWindowAttributeDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DwmSetWindowAttributeDelegate));
            return delegateForFunctionPointer(hwnd, dwAttribute, ref pvAttribute, cbAttribute);
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        public static bool IsDwmAvailable()
        {
            if (Features.VistaOrLater)
            {
                if (LoadLibrary("dwmapi") == IntPtr.Zero)
                    return false;

                bool enabled;
                DwmIsCompositionEnabled(out enabled);

                return enabled;
            }
            return false;
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        // Nested Types
        private delegate int DwmExtendFrameIntoClientAreaDelegate(IntPtr hwnd, ref MARGINS margins);

        private delegate int DwmSetWindowAttributeDelegate(IntPtr hwnd, uint dwAttribute, ref IntPtr pvAttribute, uint cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
    }
}
