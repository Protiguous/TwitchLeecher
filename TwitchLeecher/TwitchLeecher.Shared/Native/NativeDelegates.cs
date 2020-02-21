namespace TwitchLeecher.Shared.Native {

    using System;

    public static class NativeDelegates {

        public delegate Boolean MonitorEnumProc( IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData );
    }
}