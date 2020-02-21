using System;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels {
    public class RevokeAuthorizationViewVM : ViewModelBase {

        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IEventAggregator _eventAggregator;

        private ICommand _revokeCommand;

        private readonly object _commandLockObject;

        public RevokeAuthorizationViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator ) {
            this._dialogService = dialogService;
            this._twitchService = twitchService;
            this._navigationService = navigationService;
            this._notificationService = notificationService;
            this._eventAggregator = eventAggregator;

            this._commandLockObject = new object();

            this._eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe( this.IsAuthorizedChanged );
        }

        public ICommand RevokeCommand {
            get {
                if ( this._revokeCommand == null ) {
                    this._revokeCommand = new DelegateCommand( this.Revoke );
                }

                return this._revokeCommand;
            }
        }

        private void Revoke() {
            try {
                lock ( this._commandLockObject ) {
                    this._twitchService.RevokeAuthorization();
                    this._navigationService.ShowAuthorize();
                    this._notificationService.ShowNotification( "Twitch authorization has been revoked!" );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void IsAuthorizedChanged( bool isAuthorized ) {
            if ( !isAuthorized ) {
                this._navigationService.ShowAuthorize();
            }
        }
    }
}