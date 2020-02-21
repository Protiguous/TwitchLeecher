namespace TwitchLeecher.Core.Models {

    using System;
    using System.IO;
    using TwitchLeecher.Core.Enums;
    using TwitchLeecher.Shared.Helpers;
    using TwitchLeecher.Shared.IO;
    using TwitchLeecher.Shared.Notification;

    public class Preferences : BindableBase {

        private Boolean _appCheckForUpdates;
        private Boolean _appShowDonationButton;
        private Boolean _downloadDisableConversion;
        private String _downloadFileName;
        private String _downloadFolder;
        private Boolean _downloadRemoveCompleted;
        private Boolean _downloadSubfoldersForFav;
        private String _downloadTempFolder;
        private String _miscExternalPlayer;
        private Boolean _miscUseExternalPlayer;
        private String _searchChannelName;
        private RangeObservableCollection<String> _searchFavouriteChannels;
        private Int32 _searchLoadLastDays;
        private Int32 _searchLoadLastVods;
        private LoadLimitType _searchLoadLimitType;
        private Boolean _searchOnStartup;
        private VideoType _searchVideoType;
        private Version _version;

        public Boolean AppCheckForUpdates {
            get {
                return this._appCheckForUpdates;
            }

            set {
                this.SetProperty( ref this._appCheckForUpdates, value );
            }
        }

        public Boolean AppShowDonationButton {
            get {
                return this._appShowDonationButton;
            }

            set {
                this.SetProperty( ref this._appShowDonationButton, value );
            }
        }

        public Boolean DownloadDisableConversion {
            get {
                return this._downloadDisableConversion;
            }

            set {
                this.SetProperty( ref this._downloadDisableConversion, value );
            }
        }

        public String DownloadFileName {
            get {
                return this._downloadFileName;
            }

            set {
                this.SetProperty( ref this._downloadFileName, value );
            }
        }

        public String DownloadFolder {
            get {
                return this._downloadFolder;
            }

            set {
                this.SetProperty( ref this._downloadFolder, value );
            }
        }

        public Boolean DownloadRemoveCompleted {
            get {
                return this._downloadRemoveCompleted;
            }

            set {
                this.SetProperty( ref this._downloadRemoveCompleted, value );
            }
        }

        public Boolean DownloadSubfoldersForFav {
            get {
                return this._downloadSubfoldersForFav;
            }

            set {
                this.SetProperty( ref this._downloadSubfoldersForFav, value );
            }
        }

        public String DownloadTempFolder {
            get {
                return this._downloadTempFolder;
            }

            set {
                this.SetProperty( ref this._downloadTempFolder, value );
            }
        }

        public String MiscExternalPlayer {
            get {
                return this._miscExternalPlayer;
            }

            set {
                this.SetProperty( ref this._miscExternalPlayer, value );
            }
        }

        public Boolean MiscUseExternalPlayer {
            get {
                return this._miscUseExternalPlayer;
            }

            set {
                this.SetProperty( ref this._miscUseExternalPlayer, value );
            }
        }

        public String SearchChannelName {
            get {
                return this._searchChannelName;
            }

            set {
                this.SetProperty( ref this._searchChannelName, value );
            }
        }

        public RangeObservableCollection<String> SearchFavouriteChannels {
            get {
                if ( this._searchFavouriteChannels == null ) {
                    this._searchFavouriteChannels = new RangeObservableCollection<String>();
                }

                return this._searchFavouriteChannels;
            }
        }

        public Int32 SearchLoadLastDays {
            get {
                return this._searchLoadLastDays;
            }

            set {
                this.SetProperty( ref this._searchLoadLastDays, value );
            }
        }

        public Int32 SearchLoadLastVods {
            get {
                return this._searchLoadLastVods;
            }

            set {
                this.SetProperty( ref this._searchLoadLastVods, value );
            }
        }

        public LoadLimitType SearchLoadLimitType {
            get {
                return this._searchLoadLimitType;
            }

            set {
                this.SetProperty( ref this._searchLoadLimitType, value );
            }
        }

        public Boolean SearchOnStartup {
            get {
                return this._searchOnStartup;
            }

            set {
                this.SetProperty( ref this._searchOnStartup, value );
            }
        }

        public VideoType SearchVideoType {
            get {
                return this._searchVideoType;
            }

            set {
                this.SetProperty( ref this._searchVideoType, value );
            }
        }

        public Version Version {
            get {
                return this._version;
            }

            set {
                this.SetProperty( ref this._version, value );
            }
        }

        public Preferences Clone() {
            Preferences clone = new Preferences() {
                Version = Version,
                AppCheckForUpdates = AppCheckForUpdates,
                AppShowDonationButton = AppShowDonationButton,
                MiscUseExternalPlayer = MiscUseExternalPlayer,
                MiscExternalPlayer = MiscExternalPlayer,
                SearchChannelName = SearchChannelName,
                SearchVideoType = SearchVideoType,
                SearchLoadLimitType = SearchLoadLimitType,
                SearchLoadLastDays = SearchLoadLastDays,
                SearchLoadLastVods = SearchLoadLastVods,
                SearchOnStartup = SearchOnStartup,
                DownloadTempFolder = DownloadTempFolder,
                DownloadFolder = DownloadFolder,
                DownloadFileName = DownloadFileName,
                DownloadSubfoldersForFav = DownloadSubfoldersForFav,
                DownloadRemoveCompleted = DownloadRemoveCompleted,
                DownloadDisableConversion = DownloadDisableConversion
            };

            clone.SearchFavouriteChannels.AddRange( this.SearchFavouriteChannels );

            return clone;
        }

        public override void Validate( String propertyName = null ) {
            base.Validate( propertyName );

            var currentProperty = nameof( this.MiscExternalPlayer );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this.MiscUseExternalPlayer ) {
                    if ( String.IsNullOrWhiteSpace( this._miscExternalPlayer ) ) {
                        this.AddError( currentProperty, "Please specify an external player!" );
                    }
                    else if ( !this._miscExternalPlayer.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase ) ) {
                        this.AddError( currentProperty, "Filename must be an executable!" );
                    }
                    else if ( !File.Exists( this._miscExternalPlayer ) ) {
                        this.AddError( currentProperty, "The specified file does not exist!" );
                    }
                }
            }

            currentProperty = nameof( this.SearchChannelName );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchOnStartup && String.IsNullOrWhiteSpace( this._searchChannelName ) ) {
                    this.AddError( currentProperty, "If 'Search on Startup' is enabled, you need to specify a default channel name!" );
                }
            }

            currentProperty = nameof( this.SearchLoadLastDays );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchLoadLimitType == LoadLimitType.Timespan && ( this._searchLoadLastDays < 1 || this._searchLoadLastDays > 999 ) ) {
                    this.AddError( currentProperty, "Value has to be between 1 and 999!" );
                }
            }

            currentProperty = nameof( this.SearchLoadLastVods );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchLoadLimitType == LoadLimitType.LastVods && ( this._searchLoadLastVods < 1 || this._searchLoadLastVods > 999 ) ) {
                    this.AddError( currentProperty, "Value has to be between 1 and 999!" );
                }
            }

            currentProperty = nameof( this.DownloadTempFolder );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( String.IsNullOrWhiteSpace( this.DownloadTempFolder ) ) {
                    this.AddError( currentProperty, "Please specify a temporary download folder!" );
                }
            }

            currentProperty = nameof( this.DownloadFolder );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( String.IsNullOrWhiteSpace( this._downloadFolder ) ) {
                    this.AddError( currentProperty, "Please specify a default download folder!" );
                }
            }

            currentProperty = nameof( this.DownloadFileName );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( String.IsNullOrWhiteSpace( this._downloadFileName ) ) {
                    this.AddError( currentProperty, "Please specify a default download filename!" );
                }
                else if ( this._downloadFileName.Contains( "." ) || FileSystem.FilenameContainsInvalidChars( this._downloadFileName ) ) {
                    var invalidChars = new String( Path.GetInvalidFileNameChars() );

                    this.AddError( currentProperty, $"Filename contains invalid characters ({invalidChars}.)!" );
                }
            }
        }
    }
}