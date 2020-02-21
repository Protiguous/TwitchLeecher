using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels {
    public class SearchResultViewVM : ViewModelBase, INavigationState {

        private readonly ITwitchService _twitchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPreferencesService _preferencesService;
        private readonly IFilenameService _filenameService;

        private readonly object _commandLockObject;

        private ICommand _viewCommand;
        private ICommand _downloadCommand;
        private ICommand _seachCommand;

        public SearchResultViewVM(
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator,
            IPreferencesService preferencesService,
            IFilenameService filenameService ) {
            this._twitchService = twitchService;
            this._dialogService = dialogService;
            this._navigationService = navigationService;
            this._notificationsService = notificationService;
            this._eventAggregator = eventAggregator;
            this._preferencesService = preferencesService;
            this._filenameService = filenameService;

            this._twitchService.PropertyChanged += this.TwitchService_PropertyChanged;

            this._commandLockObject = new object();
        }

        public double ScrollPosition { get; set; }

        public ObservableCollection<TwitchVideo> Videos {
            get {
                return this._twitchService.Videos;
            }
        }

        public ICommand ViewCommand {
            get {
                if ( this._viewCommand == null ) {
                    this._viewCommand = new DelegateCommand<string>( this.ViewVideo );
                }

                return this._viewCommand;
            }
        }

        public ICommand DownloadCommand {
            get {
                if ( this._downloadCommand == null ) {
                    this._downloadCommand = new DelegateCommand<string>( this.DownloadVideo );
                }

                return this._downloadCommand;
            }
        }

        public ICommand SeachCommnad {
            get {
                if ( this._seachCommand == null ) {
                    this._seachCommand = new DelegateCommand( this.ShowSearch );
                }

                return this._seachCommand;
            }
        }

        private void ViewVideo( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        TwitchVideo video = this.Videos.Where( v => v.Id == id ).FirstOrDefault();

                        if ( video != null && video.Url != null && video.Url.IsAbsoluteUri ) {
                            this.StartVideoStream( video );
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void StartVideoStream( TwitchVideo video ) {
            try {
                Preferences currentPrefs = this._preferencesService.CurrentPreferences;

                if ( currentPrefs.MiscUseExternalPlayer ) {
                    Process.Start( currentPrefs.MiscExternalPlayer, video.Url.ToString() );
                }
                else {
                    Process.Start( video.Url.ToString() );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void DownloadVideo( string id ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( !string.IsNullOrWhiteSpace( id ) ) {
                        TwitchVideo video = this.Videos.Where( v => v.Id == id ).FirstOrDefault();

                        if ( video != null ) {
                            VodAuthInfo vodAuthInfo = this._twitchService.RetrieveVodAuthInfo( video.Id );

                            if ( !vodAuthInfo.Privileged && vodAuthInfo.SubOnly ) {
                                if ( !this._twitchService.IsAuthorized ) {
                                    this._dialogService.ShowMessageBox( "This video is sub-only! Please authorize your Twitch account by clicking the Twitch button in the menu.", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                                }
                                else {
                                    this._dialogService.ShowMessageBox( "This video is sub-only but you are not subscribed to '" + video.Channel + "'!", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                                }

                                return;
                            }

                            Preferences currentPrefs = this._preferencesService.CurrentPreferences.Clone();

                            string folder = currentPrefs.DownloadSubfoldersForFav && this._preferencesService.IsChannelInFavourites( video.Channel )
                                ? Path.Combine( currentPrefs.DownloadFolder, video.Channel )
                                : currentPrefs.DownloadFolder;

                            string filename = this._filenameService.SubstituteWildcards( currentPrefs.DownloadFileName, video );
                            filename = this._filenameService.EnsureExtension( filename, currentPrefs.DownloadDisableConversion );

                            DownloadParameters downloadParams = new DownloadParameters( video, vodAuthInfo, video.Qualities.First(), folder, filename, currentPrefs.DownloadDisableConversion );

                            this._navigationService.ShowDownload( downloadParams );
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        public void ShowSearch() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.ShowSearch();
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

            menuCommands.Add( new MenuCommand( this.SeachCommnad, "New Search", "Search" ) );

            return menuCommands;
        }

        private void TwitchService_PropertyChanged( object sender, PropertyChangedEventArgs e ) {
            string propertyName = e.PropertyName;

            this.FirePropertyChanged( propertyName );

            if ( propertyName.Equals( nameof( this.Videos ) ) ) {
                this.ScrollPosition = 0;
            }
        }
    }
}