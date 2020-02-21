using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels {
    public class UpdateInfoViewVM : ViewModelBase {

        private readonly IDialogService _dialogService;

        private readonly object _commandLockObject;

        private ICommand _downloadCommand;

        public UpdateInfoViewVM( IDialogService dialogService ) {
            this._dialogService = dialogService;
            this._commandLockObject = new object();
        }

        public UpdateInfo UpdateInfo { get; set; }

        public ICommand DownloadCommand {
            get {
                if ( this._downloadCommand == null ) {
                    this._downloadCommand = new DelegateCommand( this.Download );
                }

                return this._downloadCommand;
            }
        }

        private void Download() {
            try {
                lock ( this._commandLockObject ) {
                    Process.Start( this.UpdateInfo.DownloadUrl );
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

            menuCommands.Add( new MenuCommand( this.DownloadCommand, "Download", "Download" ) );

            return menuCommands;
        }
    }
}