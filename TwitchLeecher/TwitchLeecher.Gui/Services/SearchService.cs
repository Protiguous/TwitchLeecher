using System;
using System.Threading.Tasks;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.Services {
    internal class SearchService : ISearchService {

        private IEventAggregator _eventAggregator;
        private IDialogService _dialogService;
        private ITwitchService _twitchService;
        private INavigationService _navigationService;
        private IPreferencesService _preferencesService;

        private SearchParameters lastSearchParams;

        public SearchService(
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            IPreferencesService preferencesService ) {
            this._eventAggregator = eventAggregator;
            this._dialogService = dialogService;
            this._twitchService = twitchService;
            this._navigationService = navigationService;
            this._preferencesService = preferencesService;

            this._eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe( this.PreferencesSaved );
        }

        public SearchParameters LastSearchParams {
            get {
                if ( this.lastSearchParams == null ) {
                    Preferences currentPrefs = this._preferencesService.CurrentPreferences;

                    SearchParameters defaultParams = new SearchParameters( SearchType.Channel ) {
                        Channel = currentPrefs.SearchChannelName,
                        VideoType = currentPrefs.SearchVideoType,
                        LoadLimitType = currentPrefs.SearchLoadLimitType,
                        LoadFrom = DateTime.Now.Date.AddDays( -currentPrefs.SearchLoadLastDays ),
                        LoadFromDefault = DateTime.Now.Date.AddDays( -currentPrefs.SearchLoadLastDays ),
                        LoadTo = DateTime.Now.Date,
                        LoadToDefault = DateTime.Now.Date,
                        LoadLastVods = currentPrefs.SearchLoadLastVods
                    };

                    this.lastSearchParams = defaultParams;
                }

                return this.lastSearchParams;
            }
        }

        public void PerformSearch( SearchParameters searchParams ) {
            this.lastSearchParams = searchParams;

            this._navigationService.ShowLoading();

            Task searchTask = new Task( () => this._twitchService.Search( searchParams ) );

            searchTask.ContinueWith( task => {
                if ( task.IsFaulted ) {
                    this._dialogService.ShowAndLogException( task.Exception );
                }

                this._navigationService.ShowSearchResults();
            }, TaskScheduler.FromCurrentSynchronizationContext() );

            searchTask.Start();
        }

        private void PreferencesSaved() {
            try {
                this.lastSearchParams = null;
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }
    }
}