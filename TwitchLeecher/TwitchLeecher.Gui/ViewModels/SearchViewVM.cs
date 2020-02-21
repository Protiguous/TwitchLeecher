using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Helpers;

namespace TwitchLeecher.Gui.ViewModels {
    public class SearchViewVM : ViewModelBase {

        private SearchParameters _searchParams;

        private ICommand _clearUrlsCommand;
        private ICommand _clearIdsCommand;
        private ICommand _searchCommand;
        private ICommand _cancelCommand;

        private readonly ITwitchService _twitchService;
        private readonly ISearchService _searchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IPreferencesService _preferencesService;

        private readonly object _commandLockObject;

        public SearchViewVM(
            ITwitchService twitchService,
            ISearchService searchService,
            IDialogService dialogService,
            INavigationService navigationService,
            IPreferencesService preferencesService ) {
            this._twitchService = twitchService;
            this._searchService = searchService;
            this._dialogService = dialogService;
            this._navigationService = navigationService;
            this._preferencesService = preferencesService;

            this._commandLockObject = new object();
        }

        public SearchParameters SearchParams {
            get {
                if ( this._searchParams == null ) {
                    this._searchParams = this._searchService.LastSearchParams.Clone();
                }

                return this._searchParams;
            }
            set {
                this.SetProperty( ref this._searchParams, value, nameof( this.SearchParams ) );
            }
        }

        public RangeObservableCollection<string> FavChannels {
            get {
                return this._preferencesService.CurrentPreferences.SearchFavouriteChannels;
            }
        }

        public ICommand ClearUrlsCommand {
            get {
                if ( this._clearUrlsCommand == null ) {
                    this._clearUrlsCommand = new DelegateCommand( this.ClearUrls );
                }

                return this._clearUrlsCommand;
            }
        }

        public ICommand ClearIdsCommand {
            get {
                if ( this._clearIdsCommand == null ) {
                    this._clearIdsCommand = new DelegateCommand( this.ClearIds );
                }

                return this._clearIdsCommand;
            }
        }

        public ICommand SearchCommand {
            get {
                if ( this._searchCommand == null ) {
                    this._searchCommand = new DelegateCommand( this.Search );
                }

                return this._searchCommand;
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

        private void ClearUrls() {
            try {
                lock ( this._commandLockObject ) {
                    this.SearchParams.Urls = null;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ClearIds() {
            try {
                lock ( this._commandLockObject ) {
                    this.SearchParams.Ids = null;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Search() {
            try {
                lock ( this._commandLockObject ) {
                    this._dialogService.SetBusy();
                    this.Validate();

                    if ( !this.HasErrors ) {
                        this._searchService.PerformSearch( this.SearchParams );
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

        public override void Validate( string propertyName = null ) {
            base.Validate( propertyName );

            string currentProperty = nameof( this.SearchParams );

            if ( string.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                this.SearchParams.Validate();

                if ( this.SearchParams.SearchType == SearchType.Channel &&
                     !string.IsNullOrWhiteSpace( this.SearchParams.Channel ) &&
                     !this._twitchService.ChannelExists( this.SearchParams.Channel ) ) {
                    this.SearchParams.AddError( nameof( this.SearchParams.Channel ), "The specified channel does not exist on Twitch!" );
                }

                if ( this.SearchParams.HasErrors ) {
                    this.AddError( currentProperty, "Invalid Search Parameters!" );
                }
            }
        }

        protected override List<MenuCommand> BuildMenu() {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if ( menuCommands == null ) {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add( new MenuCommand( this.SearchCommand, "Search", "Search" ) );
            menuCommands.Add( new MenuCommand( this.CancelCommand, "Cancel", "Times" ) );

            return menuCommands;
        }
    }
}