using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels {
    public class LogViewVM : ViewModelBase {

        private TwitchVideoDownload _download;

        private ICommand _copyCommand;
        private ICommand _closeCommand;

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private readonly object _commandLockObject;

        public LogViewVM(
            IDialogService dialogService,
            INavigationService navigationService ) {
            this._dialogService = dialogService;
            this._navigationService = navigationService;

            this._commandLockObject = new object();
        }

        public TwitchVideoDownload Download {
            get {
                return this._download;
            }
            set {
                this.SetProperty( ref this._download, value, nameof( this.Download ) );
            }
        }

        public ICommand CopyCommand {
            get {
                if ( this._copyCommand == null ) {
                    this._copyCommand = new DelegateCommand( this.Copy );
                }

                return this._copyCommand;
            }
        }

        public ICommand CloseCommand {
            get {
                if ( this._closeCommand == null ) {
                    this._closeCommand = new DelegateCommand( this.Close );
                }

                return this._closeCommand;
            }
        }

        private void Copy() {
            try {
                lock ( this._commandLockObject ) {
                    Clipboard.SetDataObject( this._download?.Log );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Close() {
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

            menuCommands.Add( new MenuCommand( this.CopyCommand, "Copy", "Copy" ) );
            menuCommands.Add( new MenuCommand( this.CloseCommand, "Back", "ArrowLeft" ) );

            return menuCommands;
        }
    }
}