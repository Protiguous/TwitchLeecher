using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using Cursors = System.Windows.Input.Cursors;

namespace TwitchLeecher.Gui.Services {
    internal class DialogService : IDialogService {

        private readonly ILogService _logService;

        private bool _busy;

        public DialogService( ILogService logService ) {
            this._logService = logService;
        }

        public MessageBoxResult ShowMessageBox( string message ) {
            MessageBoxWindow msg = new MessageBoxWindow( message );
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox( string message, string caption ) {
            MessageBoxWindow msg = new MessageBoxWindow( message, caption );
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox( string message, string caption, MessageBoxButton buttons ) {
            MessageBoxWindow msg = new MessageBoxWindow( message, caption, buttons );
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox( string message, string caption, MessageBoxButton buttons, MessageBoxImage icon ) {
            MessageBoxWindow msg = new MessageBoxWindow( message, caption, buttons, icon );
            msg.ShowDialog();

            return msg.Result;
        }

        public void ShowAndLogException( Exception ex ) {
            if ( ex == null ) {
                return;
            }

            string logFile = this._logService.LogException( ex );

            MessageBoxWindow msg = new MessageBoxWindow( "An unexpected error occured:"
                + Environment.NewLine + Environment.NewLine + ex.Message
                + Environment.NewLine + Environment.NewLine + "All details were written to log file"
                + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error );
            msg.ShowDialog();
        }

        public void ShowFolderBrowserDialog( string folder, Action<bool, string> dialogCompleteCallback ) {
            using ( CommonOpenFileDialog cofd = new CommonOpenFileDialog() ) {
                cofd.IsFolderPicker = true;

                if ( !string.IsNullOrWhiteSpace( folder ) ) {
                    cofd.InitialDirectory = folder;
                }

                CommonFileDialogResult result = cofd.ShowDialog();

                bool canceled = result != CommonFileDialogResult.Ok;

                dialogCompleteCallback( canceled, canceled ? null : cofd.FileName );
            }
        }

        public void ShowFileBrowserDialog( CommonFileDialogFilter filter, string folder, Action<bool, string> dialogCompleteCallback ) {
            using ( CommonOpenFileDialog cofd = new CommonOpenFileDialog() ) {
                cofd.Filters.Add( filter );

                if ( !string.IsNullOrWhiteSpace( folder ) ) {
                    cofd.InitialDirectory = folder;
                }

                CommonFileDialogResult result = cofd.ShowDialog();

                bool canceled = result != CommonFileDialogResult.Ok;

                dialogCompleteCallback( canceled, canceled ? null : cofd.FileName );
            }
        }

        public void SetBusy() {
            this.SetBusy( true );
        }

        private void SetBusy( bool busy ) {
            if ( this._busy != busy ) {
                this._busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if ( this._busy ) {
                    new DispatcherTimer( TimeSpan.FromSeconds( 0 ), DispatcherPriority.ApplicationIdle, this.DispatcherTimer_Tick, Dispatcher.CurrentDispatcher );
                }
            }
        }

        private void DispatcherTimer_Tick( object sender, EventArgs e ) {
            if ( sender is DispatcherTimer dispatcherTimer ) {
                this.SetBusy( false );
                dispatcherTimer.Stop();
            }
        }
    }
}