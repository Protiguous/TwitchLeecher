namespace TwitchLeecher.Core.Models {

    using System;
    using TwitchLeecher.Core.Enums;
    using TwitchLeecher.Shared.Notification;

    public class SearchParameters : BindableBase {

        private String _channel;
        private String _ids;
        private DateTime? _loadFrom;
        private DateTime? _loadFromDefault;
        private Int32 _loadLastVods;
        private LoadLimitType _loadLimitType;
        private DateTime? _loadTo;
        private DateTime? _loadToDefault;
        private SearchType _searchType;
        private String _urls;
        private VideoType _videoType;

        public String Channel {
            get {
                return this._channel;
            }

            set {
                this.SetProperty( ref this._channel, value );
            }
        }

        public String Ids {
            get {
                return this._ids;
            }

            set {
                this.SetProperty( ref this._ids, value );
            }
        }

        public DateTime? LoadFrom {
            get {
                return this._loadFrom;
            }

            set {
                this.SetProperty( ref this._loadFrom, value );
            }
        }

        public DateTime? LoadFromDefault {
            get {
                return this._loadFromDefault;
            }

            set {
                this.SetProperty( ref this._loadFromDefault, value );
            }
        }

        public Int32 LoadLastVods {
            get {
                return this._loadLastVods;
            }

            set {
                this.SetProperty( ref this._loadLastVods, value );
            }
        }

        public LoadLimitType LoadLimitType {
            get {
                return this._loadLimitType;
            }

            set {
                this.SetProperty( ref this._loadLimitType, value );
            }
        }

        public DateTime? LoadTo {
            get {
                return this._loadTo;
            }

            set {
                this.SetProperty( ref this._loadTo, value );
            }
        }

        public DateTime? LoadToDefault {
            get {
                return this._loadToDefault;
            }

            set {
                this.SetProperty( ref this._loadToDefault, value );
            }
        }

        public SearchType SearchType {
            get {
                return this._searchType;
            }

            set {
                this.SetProperty( ref this._searchType, value );
            }
        }

        public String Urls {
            get {
                return this._urls;
            }

            set {
                this.SetProperty( ref this._urls, value );
            }
        }

        public VideoType VideoType {
            get {
                return this._videoType;
            }

            set {
                this.SetProperty( ref this._videoType, value );
            }
        }

        public SearchParameters( SearchType searchType ) {
            this._searchType = searchType;
        }

        public SearchParameters Clone() {
            return new SearchParameters( this._searchType ) {
                VideoType = _videoType,
                Channel = _channel,
                Urls = _urls,
                Ids = _ids,
                LoadLimitType = _loadLimitType,
                LoadFrom = _loadFrom,
                LoadFromDefault = _loadFromDefault,
                LoadTo = _loadTo,
                LoadToDefault = _loadToDefault,
                LoadLastVods = _loadLastVods
            };
        }

        public override void Validate( String propertyName = null ) {
            base.Validate( propertyName );

            var currentProperty = nameof( this.Channel );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Channel && String.IsNullOrWhiteSpace( this._channel ) ) {
                    this.AddError( currentProperty, "Please specify a channel name!" );
                }
            }

            currentProperty = nameof( this.LoadFrom );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Channel && this._loadLimitType == LoadLimitType.Timespan ) {
                    if ( !this._loadFrom.HasValue ) {
                        this.AddError( currentProperty, "Please specify a date!" );
                    }
                    else {
                        DateTime minimum = new DateTime( 2010, 01, 01 );

                        if ( this._loadFrom.Value.Date < minimum.Date ) {
                            this.AddError( currentProperty, "Date has to be greater than '" + minimum.ToShortDateString() + "'!" );
                        }

                        if ( this._loadFrom.Value.Date > DateTime.Now.Date ) {
                            this.AddError( currentProperty, "Date cannot be greater than today!" );
                        }
                    }
                }
            }

            currentProperty = nameof( this.LoadTo );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Channel && this._loadLimitType == LoadLimitType.Timespan ) {
                    if ( !this._loadTo.HasValue ) {
                        this.AddError( currentProperty, "Please specify a date!" );
                    }
                    else {
                        if ( this._loadTo.Value.Date > DateTime.Now.Date ) {
                            this.AddError( currentProperty, "Date cannot be greater than today!" );
                        }

                        if ( this._loadFrom.HasValue && this._loadFrom.Value.Date > this._loadTo.Value.Date ) {
                            this.AddError( currentProperty, "Date has to be greater than '" + this._loadFrom.Value.ToShortDateString() + "'!" );
                        }
                    }
                }
            }

            currentProperty = nameof( this.LoadLastVods );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Channel && this._loadLimitType == LoadLimitType.LastVods ) {
                    if ( this._loadLastVods < 1 || this._loadLastVods > 999 ) {
                        this.AddError( currentProperty, "Value has to be between 1 and 999!" );
                    }
                }
            }

            currentProperty = nameof( this.Urls );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Urls ) {
                    if ( String.IsNullOrWhiteSpace( this._urls ) ) {
                        this.AddError( currentProperty, "Please specify one or more Twitch video urls!" );
                    }
                    else {
                        void AddUrlError() {
                            this.AddError( currentProperty, "One or more urls are invalid!" );
                        }

                        String[] urls = this._urls.Split( new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

                        if ( urls.Length > 0 ) {
                            foreach ( var url in urls ) {
                                if ( !Uri.TryCreate( url, UriKind.Absolute, out Uri validUrl ) ) {
                                    AddUrlError();
                                    break;
                                }

                                String[] segments = validUrl.Segments;

                                if ( segments.Length < 2 ) {
                                    AddUrlError();
                                    break;
                                }

                                var validId = false;

                                for ( var i = 0; i < segments.Length; i++ ) {
                                    if ( segments[ i ].Equals( "videos/", StringComparison.OrdinalIgnoreCase ) ) {
                                        if ( segments.Length > ( i + 1 ) ) {
                                            var idStr = segments[ i + 1 ];

                                            if ( !String.IsNullOrWhiteSpace( idStr ) ) {
                                                idStr = idStr.Trim( new Char[] { '/' } );

                                                if ( Int32.TryParse( idStr, out Int32 idInt ) && idInt > 0 ) {
                                                    validId = true;
                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }

                                if ( !validId ) {
                                    AddUrlError();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            currentProperty = nameof( this.Ids );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._searchType == SearchType.Ids ) {
                    if ( String.IsNullOrWhiteSpace( this._ids ) ) {
                        this.AddError( currentProperty, "Please specify one or more Twitch video IDs!" );
                    }
                    else {
                        String[] ids = this._ids.Split( new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

                        if ( ids.Length > 0 ) {
                            foreach ( var id in ids ) {
                                if ( !Int32.TryParse( id, out Int32 idInt ) || idInt <= 0 ) {
                                    this.AddError( currentProperty, "One or more IDs are invalid!" );
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}