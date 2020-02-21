namespace TwitchLeecher.Shared.Native {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using static TwitchLeecher.Shared.Native.NativeDelegates;
    using static TwitchLeecher.Shared.Native.NativeMethods;
    using static TwitchLeecher.Shared.Native.NativeStructs;

    public class NativeDisplay {

        public System.Windows.Rect Bounds { get; }

        public IntPtr Handle { get; }

        public Boolean IsPrimary { get; }

        public String Name { get; }

        public System.Windows.Rect WorkingArea { get; }

        private NativeDisplay( IntPtr hMonitor, IntPtr hdc ) {
            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfoNative( new HandleRef( null, hMonitor ), info );

            this.IsPrimary = ( ( info.dwFlags & NativeFlags.MonitorinfofPrimary ) != 0 );
            this.Name = new String( info.szDevice ).TrimEnd( ( Char )0 );
            this.Handle = hMonitor;

            this.Bounds = new System.Windows.Rect(
                        info.rcMonitor.left, info.rcMonitor.top,
                        info.rcMonitor.right - info.rcMonitor.left,
                        info.rcMonitor.bottom - info.rcMonitor.top );

            this.WorkingArea = new System.Windows.Rect(
                        info.rcWork.left, info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top );
        }

        public static IEnumerable<NativeDisplay> GetAllDisplays() {
            DisplayEnumCallback closure = new DisplayEnumCallback();
            MonitorEnumProc proc = new MonitorEnumProc( closure.Callback );
            EnumDisplayMonitorsNative( new HandleRef( null, IntPtr.Zero ), IntPtr.Zero, proc, IntPtr.Zero );
            return closure.Displays.Cast<NativeDisplay>();
        }

        public static NativeDisplay GetDisplayFromWindow( IntPtr handle ) {
            IntPtr hMonitor = MonitorFromWindowNative( handle, NativeFlags.MONITOR_DEFAULTTONEAREST );

            return GetAllDisplays().Where( d => d.Handle == hMonitor ).First();
        }

        private class DisplayEnumCallback {

            public ArrayList Displays { get; private set; }

            public DisplayEnumCallback() {
                this.Displays = new ArrayList();
            }

            public Boolean Callback( IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData ) {
                this.Displays.Add( new NativeDisplay( hMonitor, hdcMonitor ) );
                return true;
            }
        }
    }
}