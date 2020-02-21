namespace TwitchLeecher.Shared.Native {

    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using static TwitchLeecher.Shared.Native.NativeDelegates;
    using static TwitchLeecher.Shared.Native.NativeStructs;

    public static class NativeMethods {

        [DllImport( "user32.dll", ExactSpelling = true )]
        [ResourceExposure( ResourceScope.None )]
        private static extern Boolean EnumDisplayMonitors( HandleRef hdc, IntPtr rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        [ResourceExposure( ResourceScope.None )]
        private static extern Boolean GetMonitorInfo( HandleRef hMonitor, [In, Out]MonitorInfoEx lpmi );

        [DllImport( "user32.dll" )]
        [ResourceExposure( ResourceScope.None )]
        private static extern IntPtr MonitorFromWindow( IntPtr hwnd, UInt32 dwFlags );

        public static Boolean EnumDisplayMonitorsNative( HandleRef hdc, IntPtr rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData ) {
            return EnumDisplayMonitors( hdc, rcClip, lpfnEnum, dwData );
        }

        public static Boolean GetMonitorInfoNative( HandleRef hMonitor, [In, Out]MonitorInfoEx lpmi ) {
            return GetMonitorInfo( hMonitor, lpmi );
        }

        public static IntPtr MonitorFromWindowNative( IntPtr hwnd, UInt32 dwFlags ) {
            return MonitorFromWindow( hwnd, dwFlags );
        }
    }
}