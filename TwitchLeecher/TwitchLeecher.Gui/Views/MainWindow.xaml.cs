using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using FontAwesome.WPF;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Native;
using static TwitchLeecher.Shared.Native.NativeMethods;
using static TwitchLeecher.Shared.Native.NativeStructs;

namespace TwitchLeecher.Gui.Views {

    public partial class MainWindow : Window {

        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private IRuntimeDataService _runtimeDataService;

        public double HeightNormal { get; set; }

        public double LeftNormal { get; set; }

        public double TopNormal { get; set; }

        public double WidthNormal { get; set; }

        public MainWindow( MainWindowVM viewModel,
                                            IEventAggregator eventAggregator,
            IDialogService dialogService,
            IRuntimeDataService runtimeDataService ) {
            this._eventAggregator = eventAggregator;
            this._dialogService = dialogService;
            this._runtimeDataService = runtimeDataService;

            this.InitializeComponent();

            WindowChrome windowChrome = new WindowChrome() {
                CaptionHeight = 55,
                CornerRadius = new CornerRadius( 0 ),
                GlassFrameThickness = new Thickness( 0 ),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness( 6 ),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome( this, windowChrome );

            // Hold reference to FontAwesome library
            ImageAwesome.CreateImageSource( FontAwesomeIcon.Times, Brushes.Black );

            this.SizeChanged += ( s, e ) => {
                if ( this.WindowState == WindowState.Normal ) {
                    this.WidthNormal = this.Width;
                    this.HeightNormal = this.Height;
                }
            };

            this.LocationChanged += ( s, e ) => {
                if ( this.WindowState == WindowState.Normal ) {
                    this.TopNormal = this.Top;
                    this.LeftNormal = this.Left;
                }
            };

            this.Loaded += ( s, e ) => {
                HwndSource.FromHwnd( new WindowInteropHelper( this ).Handle ).AddHook( new HwndSourceHook( this.WindowProc ) );

                this.DataContext = viewModel;

                if ( viewModel != null ) {
                    viewModel.Loaded();
                }

                this.LoadWindowState();
            };

            this.Closed += ( s, e ) => {
                this.SaveWindowState();
            };
        }

        private static void WmGetMinMaxInfo( IntPtr hwnd, IntPtr lParam ) {
            MINMAXINFO mmi = ( MINMAXINFO )Marshal.PtrToStructure( lParam, typeof( MINMAXINFO ) );

            IntPtr hMonitor = MonitorFromWindowNative( hwnd, NativeFlags.MONITOR_DEFAULTTONEAREST );

            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfoNative( new HandleRef( null, hMonitor ), info );

            NativeStructs.Rect rcWorkArea = info.rcWork;
            NativeStructs.Rect rcMonitorArea = info.rcMonitor;

            mmi.ptMaxPosition.x = Math.Abs( rcWorkArea.left - rcMonitorArea.left );
            mmi.ptMaxPosition.y = Math.Abs( rcWorkArea.top - rcMonitorArea.top );
            mmi.ptMaxSize.x = Math.Abs( rcWorkArea.right - rcWorkArea.left );
            mmi.ptMaxSize.y = Math.Abs( rcWorkArea.bottom - rcWorkArea.top );

            Marshal.StructureToPtr( mmi, lParam, true );
        }

        private void ValidateWindowState( bool firstStart ) {
            if ( firstStart ) {
                Screen screen = Screen.FromHandle( new WindowInteropHelper( this ).Handle );

                double availableHeight = screen.WorkingArea.Height;

                if ( this.Height > availableHeight ) {
                    this.Height = Math.Max( this.MinHeight, availableHeight );

                    if ( this.Height > availableHeight ) {
                        this.Top = 0;
                    }
                    else {
                        this.Top = ( availableHeight / 2 ) - ( this.Height / 2 );
                    }
                }
            }
            else {
                Screen currentScreen = Screen.FromRectangle( new System.Drawing.Rectangle( ( int ) this.Left, ( int ) this.Top, ( int ) this.Width, ( int ) this.Height ) );
                Screen mostRightScreen = Screen.AllScreens.Aggregate( ( s1, s2 ) => s1.Bounds.Right > s2.Bounds.Right ? s1 : s2 );

                if ( this.Top < 0 ) {
                    this.Top = 0;
                }

                if ( this.Top > currentScreen.WorkingArea.Height ) {
                    this.Top = Math.Max( 0, currentScreen.WorkingArea.Height - this.Height );
                }

                if ( this.Left < 0 ) {
                    this.Left = 0;
                }

                if ( this.Left > mostRightScreen.Bounds.Right ) {
                    this.Left = Math.Max( mostRightScreen.Bounds.Left, mostRightScreen.Bounds.Right - this.Width );
                }
            }
        }

        private IntPtr WindowProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled ) {
            switch ( msg ) {
                case NativeFlags.WM_GETMINMAXINFO:
                    WmGetMinMaxInfo( hwnd, lParam );
                    handeled = true;
                    break;

                case NativeFlags.WM_WINDOWPOSCHANGING:
                    WINDOWPOS pos = ( WINDOWPOS )Marshal.PtrToStructure( lParam, typeof( WINDOWPOS ) );

                    if ( ( pos.flags & 0x0002 ) != 0 ) {
                        return IntPtr.Zero;
                    }

                    Window wnd = ( Window )HwndSource.FromHwnd( hwnd ).RootVisual;
                    if ( wnd == null ) {
                        return IntPtr.Zero;
                    }

                    bool changedPos = false;

                    PresentationSource source = PresentationSource.FromVisual( this );

                    double wpfDpi = 96;

                    double dpiX = wpfDpi;
                    double dpiY = wpfDpi;

                    if ( source != null ) {
                        dpiX = wpfDpi * source.CompositionTarget.TransformToDevice.M11;
                        dpiY = wpfDpi * source.CompositionTarget.TransformToDevice.M22;
                    }

                    int minWidth = ( int )Math.Round( this.MinWidth / 96 * dpiX, 0 );

                    if ( pos.cx < minWidth ) {
                        pos.cx = minWidth;
                        changedPos = true;
                    }

                    int minHeight = ( int )Math.Round( this.MinHeight / 96 * dpiY, 0 );

                    if ( pos.cy < minHeight ) {
                        pos.cy = minHeight;
                        changedPos = true;
                    }

                    if ( !changedPos ) {
                        return IntPtr.Zero;
                    }

                    Marshal.StructureToPtr( pos, lParam, true );
                    handeled = true;
                    break;
            }

            return ( IntPtr )0;
        }

        public void LoadWindowState() {
            try {
                MainWindowInfo mainWindowInfo = this._runtimeDataService.RuntimeData.MainWindowInfo;

                if ( mainWindowInfo != null ) {
                    this.Width = Math.Max( this.MinWidth, mainWindowInfo.Width );
                    this.Height = Math.Max( this.MinHeight, mainWindowInfo.Height );
                    this.Top = mainWindowInfo.Top;
                    this.Left = mainWindowInfo.Left;
                    this.WindowState = mainWindowInfo.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                    this.ValidateWindowState( false );
                }
                else {
                    this.ValidateWindowState( true );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        public void SaveWindowState() {
            try {
                MainWindowInfo mainWindowInfo = new MainWindowInfo() {
                    Width = this.WidthNormal,
                    Height = this.HeightNormal,
                    Top = this.TopNormal,
                    Left = this.LeftNormal,
                    IsMaximized = this.WindowState == WindowState.Maximized
                };

                this._runtimeDataService.RuntimeData.MainWindowInfo = mainWindowInfo;
                this._runtimeDataService.Save();
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        public void ShowNotification( string text ) {
            this.notificationStrip.ShowNotification( text );
        }
    }
}