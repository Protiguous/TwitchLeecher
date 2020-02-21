using System;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels {
    public class AuthorizeViewVM : ViewModelBase {

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IEventAggregator _eventAggregator;

        private ICommand _connectCommand;

        private readonly object _commandLockObject;

        public AuthorizeViewVM(
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator ) {
            this._dialogService = dialogService;
            this._navigationService = navigationService;
            this._eventAggregator = eventAggregator;

            this._commandLockObject = new object();

            this._eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe( this.IsAuthorizedChanged );
        }

        public ICommand ConnectCommand {
            get {
                if ( this._connectCommand == null ) {
                    this._connectCommand = new DelegateCommand( this.Connect );
                }

                return this._connectCommand;
            }
        }

        private void Connect() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.ShowTwitchConnect();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void IsAuthorizedChanged( bool isAuthorized ) {
            if ( isAuthorized ) {
                this._navigationService.ShowRevokeAuthorization();
            }
        }
    }
}