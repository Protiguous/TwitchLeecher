namespace TwitchLeecher {

    using System;
    using System.Globalization;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Services.Modules;
    using TwitchLeecher.Shared.Events;

    public partial class App : Application {

        private IKernel _kernel;

        private IKernel CreateKernel() {
            IKernel kernel = new StandardKernel();

            this.RegisterTypes( kernel );
            this.LoadModules( kernel );

            return kernel;
        }

        private void Current_DispatcherUnhandledException( Object sender, DispatcherUnhandledExceptionEventArgs e ) {
            try {
                Exception ex = e.Exception;

                ILogService logService = this._kernel.Get<ILogService>();
                var logFile = logService.LogException( ex );

                MessageBox.Show( "An unhandled UI exception occured and was written to log file"
                    + Environment.NewLine + Environment.NewLine + logFile
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal UI Error", MessageBoxButton.OK, MessageBoxImage.Error );

                Current?.Shutdown();
            }
            catch {
                try {
                    MessageBox.Show( "An unhandled UI exception occured but could not be written to a log file!"
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal UI Error", MessageBoxButton.OK, MessageBoxImage.Error );
                }
                finally {
                    Current?.Shutdown();
                }
            }
        }

        private void CurrentDomain_UnhandledException( Object sender, UnhandledExceptionEventArgs e ) {
            try {
                Exception ex = ( Exception )e.ExceptionObject;

                ILogService logService = this._kernel.Get<ILogService>();
                var logFile = logService.LogException( ex );

                MessageBox.Show( "An unhandled exception occured and was written to a log file!"
                    + Environment.NewLine + Environment.NewLine + logFile
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error );

                Current?.Shutdown();
            }
            catch {
                try {
                    MessageBox.Show( "An unhandled exception occured but could not be written to a log file!"
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error );
                }
                finally {
                    Current?.Shutdown();
                }
            }
        }

        private void LoadModules( IKernel kernel ) {
            kernel.Load<GuiModule>();
            kernel.Load<ServiceModule>();
        }

        private void RegisterTypes( IKernel kernel ) {
            kernel.Bind<MainWindow>().ToSelf().InSingletonScope();
            kernel.Bind<MainWindowVM>().ToSelf().InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        }

        protected override void OnStartup( StartupEventArgs e ) {
            base.OnStartup( e );

            if ( CultureInfo.CurrentCulture.Name.StartsWith( "ar", StringComparison.OrdinalIgnoreCase ) ||
                CultureInfo.CurrentUICulture.Name.StartsWith( "ar", StringComparison.OrdinalIgnoreCase ) ) {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo( "en-US" );
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo( "en-US" );
            }

            this._kernel = this.CreateKernel();

            DispatcherUnhandledException += this.Current_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;

            ToolTipService.ShowDurationProperty.OverrideMetadata( typeof( DependencyObject ), new FrameworkPropertyMetadata( Int32.MaxValue ) );

            ServicePointManager.ServerCertificateValidationCallback = ( sender, certificate, chain, sslPolicyErrors ) => { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 10;

            this.MainWindow = this._kernel.Get<MainWindow>();
            this.MainWindow.Show();
        }
    }
}