using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Lemon.Utils;

namespace Lemon
{
    public class LayeredWindow : Window
    {
        private HwndSource _hwndSource;

        public LayeredWindow()
        {
            Loaded += SetProc;
        }

        void SetProc(object sender, RoutedEventArgs e)
        {
            _hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;

            if (Features.LayeredWindowAccelerated)
            {
                _hwndSource.AddHook(WndProc);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (Features.LayeredWindowAccelerated)
            {
                WindowStyle = WindowStyle.None;

                Win32.MARGINS margins;
                margins.bottomHeight = 1;
                margins.leftWidth = 0;
                margins.rightWidth = 0;
                margins.topHeight = 0;
                var helper = new WindowInteropHelper(this);
                Win32.DwmExtendFrameIntoClientArea(helper.Handle, ref margins);
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x83: // WM_NCCALCSIZE
                    if (wParam == new IntPtr(1))
                    {
                        handled = true;
                    }
                    break;
                case 0x86: // WM_NCACTIVATE
                    var ptr = Win32.DefWindowProc(hwnd, msg, wParam, new IntPtr(-1));
                    handled = true;
                    return ptr;
                case 20: // 0x14 WM_ERASEBKGND
                    Graphics.FromHdc(wParam).Clear(System.Drawing.Color.White);
                    handled = true;
                    break;
                case 0x24:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        public void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(0xf000 + direction), IntPtr.Zero);
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var structure = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            var hMonitor = MonitorFromWindow(hwnd, 2);

            if (hMonitor != IntPtr.Zero)
            {
                var lpmi = new MONITORINFO();
                GetMonitorInfo(hMonitor, lpmi);
                RECT rcWork = lpmi.rcWork;
                RECT rcMonitor = lpmi.rcMonitor;
                structure.ptMaxPosition.x = Math.Abs((int)(rcWork.left - rcMonitor.left));
                structure.ptMaxPosition.y = Math.Abs((int)(rcWork.top - rcMonitor.top));
                structure.ptMaxSize.x = Math.Abs((int)(rcWork.right - rcWork.left));
                structure.ptMaxSize.y = Math.Abs((int)(rcWork.bottom - rcWork.top));
                structure.ptMinTrackSize.x = (int)Application.Current.MainWindow.MinWidth;
                structure.ptMinTrackSize.y = (int)Application.Current.MainWindow.MinHeight;
            }
            Marshal.StructureToPtr(structure, lParam, true);
        }

        #region Pinvokes
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty;

            public int Width
            {
                get
                {
                    return Math.Abs((int)(this.right - this.left));
                }
            }
            public int Height
            {
                get
                {
                    return (this.bottom - this.top);
                }
            }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            public bool IsEmpty
            {
                get
                {
                    return ((this.left >= this.right) || (this.top >= this.bottom));
                }
            }

            public override string ToString()
            {
                if (this == Empty)
                {
                    return "RECT {Empty}";
                }
                return string.Concat(new object[] { "RECT { left : ", this.left, " / top : ", this.top, " / right : ", this.right, " / bottom : ", this.bottom, " }" });
            }

            public override bool Equals(object obj)
            {
                return ((obj is Rect) && (this == ((RECT)obj)));
            }

            public override int GetHashCode()
            {
                return (((this.left.GetHashCode() + this.top.GetHashCode()) + this.right.GetHashCode()) + this.bottom.GetHashCode());
            }

            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return ((((rect1.left == rect2.left) && (rect1.top == rect2.top)) && (rect1.right == rect2.right)) && (rect1.bottom == rect2.bottom));
            }

            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }

            static RECT()
            {
                Empty = new RECT();
            }
        }

        public enum ResizeDirection
        {
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOSPARAMS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }
        #endregion
    }
}
