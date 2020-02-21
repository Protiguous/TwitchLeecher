using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels {
    public class TwitchConnectViewVM : ViewModelBase {

        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        private ICommand _navigatingCommand;
        private ICommand _cancelCommand;

        private readonly object _commandLockObject;

        public TwitchConnectViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService ) {
            this._dialogService = dialogService;
            this._twitchService = twitchService;
            this._navigationService = navigationService;
            this._notificationService = notificationService;

            this._commandLockObject = new object();
        }

        public ICommand NavigatingCommand {
            get {
                if ( this._navigatingCommand == null ) {
                    this._navigatingCommand = new DelegateCommand<Uri>( this.Navigating );
                }

                return this._navigatingCommand;
            }
        }

        public ICommand CancelCommand {
            get {
                if ( this._cancelCommand == null ) {
                    this._cancelCommand = new DelegateCommand( this.Cancel );
                }

                return this._cancelCommand;
            }
        }

        private void Navigating( Uri url ) {
            try {
                lock ( this._commandLockObject ) {
                    string urlStr = url?.OriginalString;

                    if ( !string.IsNullOrWhiteSpace( urlStr ) && urlStr.StartsWith( "http://www.tl.com", StringComparison.OrdinalIgnoreCase ) ) {
                        NameValueCollection urlParams = HttpUtility.ParseQueryString( url.Query );

                        int tokenIndex = urlStr.IndexOf( "#access_token=" );

                        if ( tokenIndex >= 0 ) {
                            tokenIndex = tokenIndex + 14; // #access_token= -> 14 chars

                            int paramIndex = urlStr.IndexOf( "&" );

                            string accessToken = null;

                            if ( paramIndex >= 0 ) {
                                accessToken = urlStr.Substring( tokenIndex, paramIndex - tokenIndex );
                            }
                            else {
                                accessToken = urlStr.Substring( tokenIndex );
                            }

                            if ( string.IsNullOrWhiteSpace( accessToken ) ) {
                                this._dialogService.ShowMessageBox( "Twitch did not respond with an access token! Authorization aborted!", "Error", MessageBoxButton.OK );
                                this._navigationService.NavigateBack();
                            }
                            else {
                                if ( this._twitchService.Authorize( accessToken ) ) {
                                    this._navigationService.ShowRevokeAuthorization();
                                    this._notificationService.ShowNotification( "Twitch authorization successful!" );
                                }
                                else {
                                    this._dialogService.ShowMessageBox( "Access Token '" + accessToken + "' could not be verified! Authorization aborted!", "Error", MessageBoxButton.OK );
                                    this._navigationService.NavigateBack();
                                }
                            }
                        }
                        else if ( urlParams.ContainsKey( "error" ) ) {
                            string error = urlParams.Get( "error" );

                            if ( !string.IsNullOrWhiteSpace( error ) && error.Equals( "access_denied", StringComparison.OrdinalIgnoreCase ) ) {
                                this._navigationService.NavigateBack();
                                this._notificationService.ShowNotification( "Twitch authorization has been canceled!" );
                            }
                            else {
                                void UnspecifiedError() {
                                    this._dialogService.ShowMessageBox( "Twitch responded with an unspecified error! Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
                                    this._navigationService.NavigateBack();
                                }

                                if ( urlParams.ContainsKey( "error_description" ) ) {
                                    string errorDesc = urlParams.Get( "error_description" );

                                    if ( string.IsNullOrWhiteSpace( errorDesc ) ) {
                                        UnspecifiedError();
                                    }
                                    else {
                                        this._dialogService.ShowMessageBox(
                                            "Twitch responded with an error:" +
                                            Environment.NewLine + Environment.NewLine +
                                            "\"" + errorDesc + "\"" +
                                            Environment.NewLine + Environment.NewLine +
                                            "Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error );

                                        this._navigationService.NavigateBack();
                                    }
                                }
                                else {
                                    UnspecifiedError();
                                }
                            }
                        }
                        else {
                            this._dialogService.ShowMessageBox( "Twitch responded neither with an access token nor an error! Authorization aborted!", "Error", MessageBoxButton.OK );
                            this._navigationService.NavigateBack();
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Cancel() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.NavigateBack();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        protected override List<MenuCommand> BuildMenu() {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if ( menuCommands == null ) {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add( new MenuCommand( this.CancelCommand, "Cancel", "Times" ) );

            return menuCommands;
        }
    }
}