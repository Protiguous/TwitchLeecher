using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels {
    public class DownloadsViewVM : ViewModelBase, INavigationState {

        private readonly ITwitchService _twitchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPreferencesService _preferencesService;

        private ICommand _retryDownloadCommand;
        private ICommand _cancelDownloadCommand;
        private ICommand _removeDownloadCommand;
        private ICommand _showLogCommand;
        private ICommand _openDownloadFolderCommand;

        private readonly object _commandLockObject;

        public DownloadsViewVM(
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IPreferencesService preferencesService ) {
            this._twitchService = twitchService;
            this._dialogService = dialogService;
            this._navigationService = navigationService;
            this._eventAggregator = eventAggregator;
            this._preferencesService = preferencesService;

            this._twitchService.PropertyChanged += this.TwitchService_PropertyChanged;

            this._commandLockObject = new object();
        }

        public double ScrollPosition { get; set; }

        public ObservableCollection<TwitchVideoDownload> Downloads {
            get {
                return this._twitchService.Downloads;
            }
        }

        public ICommand RetryDownloadCommand {
            get {
                if ( this._retryDownloadCommand == null ) {
                    this._retryDownloadCommand = new DelegateCommand<string>( this.RetryDownload );
                }

                return this._retryDownloadCommand;
            }
        }

        public ICommand CancelDownloadCommand {
            get {
                if ( this._cancelDownloadCommand == null ) {
                    this._cancelDownloadCommand = new DelegateCommand<string>( this.CancelDownload );
                }

                return this._cancelDownloadCommand;
            }
        }

        public ICommand RemoveDownloadCommand {
            get {
                if ( this._removeDownloadCommand == null ) {
                    this._removeDownloadCommand = new DelegateCommand<string>( this.RemoveDownload );
                }

                return this._removeDownloadCommand;
            }
        }

        public ICommand ShowLogCommand {
            get {
                if ( this._showLogCommand == null ) {
                    this._showLogCommand = new DelegateCommand<string>( this.ShowLog );
                }

                return this._showLogCommand;
            }
        }

        public ICommand OpenDownloadFolderCommand {
            get {
                if ( this._openDownloadFolderCommand == null ) {
                    this._openDownloadFolderCommand = new DelegateCommand( this.OpenDownloadFolder );
                }

                return this._openDownloadFolderCommand;
            }
        }

        private void RetryDownload( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        this._twitchService.Retry( id );
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void CancelDownload( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        this._twitchService.Cancel( id );
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void RemoveDownload( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        this._twitchService.Remove( id );
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ViewVideo( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        TwitchVideoDownload download = this.Downloads.Where( d => d.Id == id ).FirstOrDefault();

                        if ( download != null ) {
                            string folder = download.DownloadParams.Folder;

                            if ( Directory.Exists( folder ) ) {
                                Process.Start( folder );
                            }
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowLog( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        TwitchVideoDownload download = this.Downloads.Where( d => d.Id == id ).FirstOrDefault();

                        if ( download != null ) {
                            this._navigationService.ShowLog( download );
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void OpenDownloadFolder() {
            try {
                lock ( this._commandLockObject ) {
                    string folder = this._preferencesService.CurrentPreferences.DownloadFolder;

                    if ( !string.IsNullOrWhiteSpace( folder ) && Directory.Exists( folder ) ) {
                        Process.Start( folder );
                    }
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

            menuCommands.Add( new MenuCommand( this.OpenDownloadFolderCommand, "Open Download Folder", "FolderOpen", 230 ) );

            return menuCommands;
        }

        private void TwitchService_PropertyChanged( object sender, PropertyChangedEventArgs e ) {
            this.FirePropertyChanged( e.PropertyName );
        }
    }
}