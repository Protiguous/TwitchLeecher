using Ninject;
using System;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Notification;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels {
    public class MainWindowVM : BindableBase {

        private bool _isAuthorized;
        private bool _showDonationButton;

        private int _videosCount;
        private int _downloadsCount;

        private ViewModelBase _mainView;

        private readonly IKernel _kernel;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITwitchService _twitchService;
        private readonly IDialogService _dialogService;
        private readonly IDonationService _donationService;
        private readonly INavigationService _navigationService;
        private readonly ISearchService _searchService;
        private readonly IPreferencesService _preferencesService;
        private readonly IRuntimeDataService _runtimeDataService;
        private readonly IUpdateService _updateService;

        private ICommand _showSearchCommand;
        private ICommand _showDownloadsCommand;
        private ICommand _showAuthorizeCommand;
        private ICommand _showPreferencesCommand;
        private ICommand _donateCommand;
        private ICommand _showInfoCommand;
        private ICommand _doMinimizeCommand;
        private ICommand _doMmaximizeRestoreCommand;
        private ICommand _doCloseCommand;
        private ICommand _requestCloseCommand;

        private readonly object _commandLockObject;

        public MainWindowVM( IKernel kernel,
            IEventAggregator eventAggregator,
            ITwitchService twitchService,
            IDialogService dialogService,
            IDonationService donationService,
            INavigationService navigationService,
            ISearchService searchService,
            IPreferencesService preferencesService,
            IRuntimeDataService runtimeDataService,
            IUpdateService updateService ) {
            AssemblyUtil au = AssemblyUtil.Get;

            this.Title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this._kernel = kernel;
            this._eventAggregator = eventAggregator;
            this._twitchService = twitchService;
            this._dialogService = dialogService;
            this._donationService = donationService;
            this._navigationService = navigationService;
            this._searchService = searchService;
            this._preferencesService = preferencesService;
            this._runtimeDataService = runtimeDataService;
            this._updateService = updateService;

            this._commandLockObject = new object();

            this._eventAggregator.GetEvent<ShowViewEvent>().Subscribe( this.ShowView );
            this._eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe( this.IsAuthorizedChanged );
            this._eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe( this.PreferencesSaved );
            this._eventAggregator.GetEvent<VideosCountChangedEvent>().Subscribe( this.VideosCountChanged );
            this._eventAggregator.GetEvent<DownloadsCountChangedEvent>().Subscribe( this.DownloadsCountChanged );

            this._showDonationButton = this._preferencesService.CurrentPreferences.AppShowDonationButton;
        }

        public bool IsAuthorized {
            get {
                return this._isAuthorized;
            }
            private set {
                this.SetProperty( ref this._isAuthorized, value, nameof( this.IsAuthorized ) );
            }
        }

        public int VideosCount {
            get {
                return this._videosCount;
            }
            private set {
                this.SetProperty( ref this._videosCount, value, nameof( this.VideosCount ) );
            }
        }

        public int DownloadsCount {
            get {
                return this._downloadsCount;
            }
            private set {
                this.SetProperty( ref this._downloadsCount, value, nameof( this.DownloadsCount ) );
            }
        }

        public string Title { get; }

        public bool ShowDonationButton {
            get {
                return this._showDonationButton;
            }

            private set {
                this.SetProperty( ref this._showDonationButton, value, nameof( this.ShowDonationButton ) );
            }
        }

        public ViewModelBase MainView {
            get {
                return this._mainView;
            }
            set {
                this.SetProperty( ref this._mainView, value, nameof( this.MainView ) );
            }
        }

        public ICommand ShowSearchCommand {
            get {
                if ( this._showSearchCommand == null ) {
                    this._showSearchCommand = new DelegateCommand( this.ShowSearch );
                }

                return this._showSearchCommand;
            }
        }

        public ICommand ShowDownloadsCommand {
            get {
                if ( this._showDownloadsCommand == null ) {
                    this._showDownloadsCommand = new DelegateCommand( this.ShowDownloads );
                }

                return this._showDownloadsCommand;
            }
        }

        public ICommand ShowAuthorizeCommand {
            get {
                if ( this._showAuthorizeCommand == null ) {
                    this._showAuthorizeCommand = new DelegateCommand( this.ShowAuthorize );
                }

                return this._showAuthorizeCommand;
            }
        }

        public ICommand ShowPreferencesCommand {
            get {
                if ( this._showPreferencesCommand == null ) {
                    this._showPreferencesCommand = new DelegateCommand( this.ShowPreferences );
                }

                return this._showPreferencesCommand;
            }
        }

        public ICommand DonateCommand {
            get {
                if ( this._donateCommand == null ) {
                    this._donateCommand = new DelegateCommand( this.Donate );
                }

                return this._donateCommand;
            }
        }

        public ICommand ShowInfoCommand {
            get {
                if ( this._showInfoCommand == null ) {
                    this._showInfoCommand = new DelegateCommand( this.ShowInfo );
                }

                return this._showInfoCommand;
            }
        }

        public ICommand DoMinimizeCommand {
            get {
                if ( this._doMinimizeCommand == null ) {
                    this._doMinimizeCommand = new DelegateCommand<Window>( this.DoMinimize );
                }

                return this._doMinimizeCommand;
            }
        }

        public ICommand DoMaximizeRestoreCommand {
            get {
                if ( this._doMmaximizeRestoreCommand == null ) {
                    this._doMmaximizeRestoreCommand = new DelegateCommand<Window>( this.DoMaximizeRestore );
                }

                return this._doMmaximizeRestoreCommand;
            }
        }

        public ICommand DoCloseCommand {
            get {
                if ( this._doCloseCommand == null ) {
                    this._doCloseCommand = new DelegateCommand<Window>( this.DoClose );
                }

                return this._doCloseCommand;
            }
        }

        public ICommand RequestCloseCommand {
            get {
                if ( this._requestCloseCommand == null ) {
                    this._requestCloseCommand = new DelegateCommand( () => { }, this.CloseApplication );
                }

                return this._requestCloseCommand;
            }
        }

        private void ShowSearch() {
            try {
                lock ( this._commandLockObject ) {
                    if ( this._videosCount > 0 ) {
                        this._navigationService.ShowSearchResults();
                    }
                    else {
                        this._navigationService.ShowSearch();
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowDownloads() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.ShowDownloads();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowAuthorize() {
            try {
                lock ( this._commandLockObject ) {
                    if ( this._twitchService.IsAuthorized ) {
                        this._navigationService.ShowRevokeAuthorization();
                    }
                    else {
                        this._navigationService.ShowAuthorize();
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowPreferences() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.ShowPreferences();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Donate() {
            try {
                lock ( this._commandLockObject ) {
                    this._donationService.OpenDonationPage();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowInfo() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.ShowInfo();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void DoMinimize( Window window ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( window == null ) {
                        throw new ArgumentNullException( nameof( window ) );
                    }

                    window.WindowState = WindowState.Minimized;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void DoMaximizeRestore( Window window ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( window == null ) {
                        throw new ArgumentNullException( nameof( window ) );
                    }

                    window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void DoClose( Window window ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( window == null ) {
                        throw new ArgumentNullException( nameof( window ) );
                    }

                    window.Close();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ShowView( ViewModelBase contentVM ) {
            if ( contentVM != null ) {
                this.MainView = contentVM;
            }
        }

        private void IsAuthorizedChanged( bool isAuthorized ) {
            this.IsAuthorized = isAuthorized;
        }

        private void PreferencesSaved() {
            try {
                this.ShowDonationButton = this._preferencesService.CurrentPreferences.AppShowDonationButton;
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void VideosCountChanged( int count ) {
            this.VideosCount = count;
        }

        private void DownloadsCountChanged( int count ) {
            this.DownloadsCount = count;
        }

        public void Loaded() {
            try {
                Preferences currentPrefs = this._preferencesService.CurrentPreferences.Clone();

                bool updateAvailable = false;

                if ( currentPrefs.AppCheckForUpdates ) {
                    UpdateInfo updateInfo = this._updateService.CheckForUpdate();

                    if ( updateInfo != null ) {
                        updateAvailable = true;
                        this._navigationService.ShowUpdateInfo( updateInfo );
                    }
                }

                bool searchOnStartup = false;

                if ( !updateAvailable && currentPrefs.SearchOnStartup ) {
                    currentPrefs.Validate();

                    if ( !currentPrefs.HasErrors ) {
                        searchOnStartup = true;

                        SearchParameters searchParams = new SearchParameters( SearchType.Channel ) {
                            Channel = currentPrefs.SearchChannelName,
                            VideoType = currentPrefs.SearchVideoType,
                            LoadLimitType = currentPrefs.SearchLoadLimitType,
                            LoadFrom = DateTime.Now.Date.AddDays( -currentPrefs.SearchLoadLastDays ),
                            LoadFromDefault = DateTime.Now.Date.AddDays( -currentPrefs.SearchLoadLastDays ),
                            LoadTo = DateTime.Now.Date,
                            LoadToDefault = DateTime.Now.Date,
                            LoadLastVods = currentPrefs.SearchLoadLastVods
                        };

                        this._searchService.PerformSearch( searchParams );
                    }
                }

                if ( !updateAvailable && !searchOnStartup ) {
                    this._navigationService.ShowWelcome();
                }

                this._twitchService.Authorize( this._runtimeDataService.RuntimeData.AccessToken );
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private bool CloseApplication() {
            try {
                this._twitchService.Pause();

                if ( !this._twitchService.CanShutdown() ) {
                    MessageBoxResult result = this._dialogService.ShowMessageBox( "Do you want to abort all running downloads and exit the application?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Warning );

                    if ( result == MessageBoxResult.No ) {
                        this._twitchService.Resume();
                        return false;
                    }
                }

                this._twitchService.Shutdown();
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }

            return true;
        }
    }
}