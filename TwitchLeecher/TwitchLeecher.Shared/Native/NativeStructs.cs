namespace TwitchLeecher.Shared.Native {

    using System;
    using System.Runtime.InteropServices;

    public static class NativeStructs {

        [StructLayout( LayoutKind.Sequential )]
        public struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout( LayoutKind.Sequential )]
        public struct POINT {
            public Int32 x;
            public Int32 y;

            public POINT( Int32 x, Int32 y ) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct Rect {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct WINDOWPOS {
            private readonly IntPtr hwnd;
            private readonly IntPtr hwndInsertAfter;
            public Int32 x;
            public Int32 y;
            public Int32 cx;
            public Int32 cy;
            public Int32 flags;
        }

        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4 )]
        public class MonitorInfoEx {
            public Int32 cbSize = Marshal.SizeOf( typeof( MonitorInfoEx ) );
            public Rect rcMonitor = new Rect();
            public Rect rcWork = new Rect();
            public Int32 dwFlags = 0;

            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
            public Char[] szDevice = new Char[ 32 ];
        }
    }
}