using Ninject;
using System;
using System.Collections.Generic;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.Services {
    internal class NavigationService : INavigationService {

        private IKernel _kernel;
        private IEventAggregator _eventAggregator;

        private ViewModelBase _lastView;
        private ViewModelBase _currentView;

        private Dictionary<Type, ViewModelBase> _persistentViews;

        public NavigationService( IKernel kernel, IEventAggregator eventAggregator ) {
            this._kernel = kernel;
            this._eventAggregator = eventAggregator;

            this._persistentViews = new Dictionary<Type, ViewModelBase>();
        }

        public void ShowWelcome() {
            this.Navigate( this._kernel.Get<WelcomeViewVM>() );
        }

        public void ShowLoading() {
            this.Navigate( this._kernel.Get<LoadingViewVM>() );
        }

        public void ShowSearch() {
            this.Navigate( this._kernel.Get<SearchViewVM>() );
        }

        public void ShowSearchResults() {
            if ( !this._persistentViews.TryGetValue( typeof( SearchResultViewVM ), out ViewModelBase vm ) ) {
                vm = this._kernel.Get<SearchResultViewVM>();
                this._persistentViews.Add( typeof( SearchResultViewVM ), vm );
            }

            this.Navigate( vm );
        }

        public void ShowDownload( DownloadParameters downloadParams ) {
            DownloadViewVM vm = this._kernel.Get<DownloadViewVM>();
            vm.DownloadParams = downloadParams ?? throw new ArgumentNullException( nameof( downloadParams ) );

            this.Navigate( vm );
        }

        public void ShowDownloads() {
            if ( !this._persistentViews.TryGetValue( typeof( DownloadsViewVM ), out ViewModelBase vm ) ) {
                vm = this._kernel.Get<DownloadsViewVM>();
                this._persistentViews.Add( typeof( DownloadsViewVM ), vm );
            }

            this.Navigate( vm );
        }

        public void ShowAuthorize() {
            this.Navigate( this._kernel.Get<AuthorizeViewVM>() );
        }

        public void ShowRevokeAuthorization() {
            this.Navigate( this._kernel.Get<RevokeAuthorizationViewVM>() );
        }

        public void ShowTwitchConnect() {
            this.Navigate( this._kernel.Get<TwitchConnectViewVM>() );
        }

        public void ShowPreferences() {
            this.Navigate( this._kernel.Get<PreferencesViewVM>() );
        }

        public void ShowInfo() {
            this.Navigate( this._kernel.Get<InfoViewVM>() );
        }

        public void ShowLog( TwitchVideoDownload download ) {
            LogViewVM vm = this._kernel.Get<LogViewVM>();
            vm.Download = download ?? throw new ArgumentNullException( nameof( download ) );

            this.Navigate( vm );
        }

        public void ShowUpdateInfo( UpdateInfo updateInfo ) {
            UpdateInfoViewVM vm = this._kernel.Get<UpdateInfoViewVM>();
            vm.UpdateInfo = updateInfo ?? throw new ArgumentNullException( nameof( updateInfo ) );

            this.Navigate( vm );
        }

        public void NavigateBack() {
            if ( this._lastView != null ) {
                this.Navigate( this._lastView );
            }
        }

        private void Navigate( ViewModelBase nextView ) {
            if ( nextView == null || ( this._currentView != null && this._currentView.GetType() == nextView.GetType() ) ) {
                return;
            }

            this._currentView?.OnBeforeHidden();

            nextView.OnBeforeShown();

            this._lastView = this._currentView;

            this._currentView = nextView;

            this._eventAggregator.GetEvent<ShowViewEvent>().Publish( nextView );
        }
    }
}