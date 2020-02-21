namespace TwitchLeecher.Services.Services {

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using TwitchLeecher.Core.Enums;
    using TwitchLeecher.Core.Events;
    using TwitchLeecher.Core.Models;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Events;
    using TwitchLeecher.Shared.Extensions;
    using TwitchLeecher.Shared.IO;
    using TwitchLeecher.Shared.Notification;
    using TwitchLeecher.Shared.Reflection;

    internal class TwitchService : BindableBase, ITwitchService, IDisposable {

        private const String ACCESS_TOKEN_URL = "https://api.twitch.tv/api/vods/{0}/access_token";
        private const String ALL_PLAYLISTS_URL = "https://usher.ttvnw.net/vod/{0}.m3u8?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true";
        private const String CHANNEL_URL = "https://api.twitch.tv/kraken/channels/{0}";
        private const String CHANNEL_VIDEOS_URL = "https://api.twitch.tv/kraken/channels/{0}/videos";
        private const Int32 DOWNLOAD_RETRIES = 3;
        private const Int32 DOWNLOAD_RETRY_TIME = 20;
        private const String GAMES_URL = "https://api.twitch.tv/kraken/games/top";
        private const String KRAKEN_URL = "https://api.twitch.tv/kraken";
        private const String TEMP_PREFIX = "TL_";
        private const Int32 TIMER_INTERVALL = 2;
        private const String TWITCH_AUTHORIZATION_HEADER = "Authorization";
        private const String TWITCH_AUTHORIZATION_PARAM = "oauth_token";
        private const String TWITCH_AUTHORIZATION_VALUE = "OAuth {0}";
        private const String TWITCH_CLIENT_ID = "37v97169hnj8kaoq8fs3hzz8v6jezdj";
        private const String TWITCH_CLIENT_ID_HEADER = "Client-ID";
        private const String TWITCH_CLIENT_ID_WEB = "kimne78kx3ncx6brgo4mv6wki5h1ko";
        private const Int32 TWITCH_MAX_LOAD_LIMIT = 100;
        private const String TWITCH_V5_ACCEPT = "application/vnd.twitchtv.v5+json";
        private const String TWITCH_V5_ACCEPT_HEADER = "Accept";
        private const String UNKNOWN_GAME_URL = "https://static-cdn.jtvnw.net/ttv-boxart/404_boxart.png";
        private const String USERS_URL = "https://api.twitch.tv/kraken/users";
        private const String VIDEO_URL = "https://api.twitch.tv/kraken/videos/{0}";

        private readonly String _appDir;
        private readonly Object _changeDownloadLockObject;
        private ObservableCollection<TwitchVideoDownload> _downloads;
        private ConcurrentDictionary<String, DownloadTask> _downloadTasks;
        private Timer _downloadTimer;
        private IEventAggregator _eventAggregator;
        private Dictionary<String, Uri> _gameThumbnails;
        private volatile Boolean _paused;
        private IPreferencesService _preferencesService;
        private IProcessingService _processingService;
        private IRuntimeDataService _runtimeDataService;
        private TwitchAuthInfo _twitchAuthInfo;
        private ObservableCollection<TwitchVideo> _videos;
        private Boolean disposedValue = false;

        public ObservableCollection<TwitchVideoDownload> Downloads {
            get {
                return this._downloads;
            }

            private set {
                if ( this._downloads != null ) {
                    this._downloads.CollectionChanged -= this.Downloads_CollectionChanged;
                }

                this.SetProperty( ref this._downloads, value, nameof( this.Downloads ) );

                if ( this._downloads != null ) {
                    this._downloads.CollectionChanged += this.Downloads_CollectionChanged;
                }

                this.FireDownloadsCountChanged();
            }
        }

        public Boolean IsAuthorized {
            get {
                return this._twitchAuthInfo != null;
            }
        }

        public ObservableCollection<TwitchVideo> Videos {
            get {
                return this._videos;
            }

            private set {
                if ( this._videos != null ) {
                    this._videos.CollectionChanged -= this.Videos_CollectionChanged;
                }

                this.SetProperty( ref this._videos, value, nameof( this.Videos ) );

                if ( this._videos != null ) {
                    this._videos.CollectionChanged += this.Videos_CollectionChanged;
                }

                this.FireVideosCountChanged();
            }
        }

        public TwitchService(
                                    IPreferencesService preferencesService,
            IProcessingService processingService,
            IRuntimeDataService runtimeDataService,
            IEventAggregator eventAggregator ) {
            this._preferencesService = preferencesService;
            this._processingService = processingService;
            this._runtimeDataService = runtimeDataService;
            this._eventAggregator = eventAggregator;

            this._videos = new ObservableCollection<TwitchVideo>();
            this._videos.CollectionChanged += this.Videos_CollectionChanged;

            this._downloads = new ObservableCollection<TwitchVideoDownload>();
            this._downloads.CollectionChanged += this.Downloads_CollectionChanged;

            this._downloadTasks = new ConcurrentDictionary<String, DownloadTask>();

            this._appDir = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            this._changeDownloadLockObject = new Object();

            this._downloadTimer = new Timer( this.DownloadTimerCallback, null, 0, TIMER_INTERVALL );

            this._eventAggregator.GetEvent<RemoveDownloadEvent>().Subscribe( this.Remove, ThreadOption.UIThread );
        }

        private void CheckTempDirectory( Action<String> log, String tempDir ) {
            if ( !Directory.Exists( tempDir ) ) {
                log( Environment.NewLine + Environment.NewLine + "Creating temporary download directory '" + tempDir + "'..." );
                FileSystem.CreateDirectory( tempDir );
                log( " done!" );
            }

            if ( Directory.EnumerateFileSystemEntries( tempDir ).Any() ) {
                throw new ApplicationException( "Temporary download directory '" + tempDir + "' is not empty!" );
            }
        }

        private void CleanUp( String directory, Action<String> log ) {
            try {
                log( Environment.NewLine + "Deleting directory '" + directory + "'..." );
                FileSystem.DeleteDirectory( directory );
                log( " done!" );
            }
            catch {
            }
        }

        private WebClient CreatePrivateApiWebClient() {
            WebClient wc = new WebClient();
            wc.Headers.Add( TWITCH_CLIENT_ID_HEADER, TWITCH_CLIENT_ID_WEB );
            wc.Headers.Add( TWITCH_V5_ACCEPT_HEADER, TWITCH_V5_ACCEPT );
            wc.Encoding = Encoding.UTF8;

            return wc;
        }

        private WebClient CreatePublicApiWebClient() {
            WebClient wc = new WebClient();
            wc.Headers.Add( TWITCH_CLIENT_ID_HEADER, TWITCH_CLIENT_ID );
            wc.Headers.Add( TWITCH_V5_ACCEPT_HEADER, TWITCH_V5_ACCEPT );
            wc.Encoding = Encoding.UTF8;
            return wc;
        }

        private CropInfo CropVodPlaylist( VodPlaylist vodPlaylist, Boolean cropStart, Boolean cropEnd, TimeSpan cropStartTime, TimeSpan cropEndTime ) {
            var start = cropStartTime.TotalMilliseconds;
            var end = cropEndTime.TotalMilliseconds;
            var length = cropEndTime.TotalMilliseconds;

            if ( cropStart ) {
                length -= start;
            }

            start = Math.Round( start / 1000, 3 );
            end = Math.Round( end / 1000, 3 );
            length = Math.Round( length / 1000, 3 );

            List<VodPlaylistPart> deleteStart = new List<VodPlaylistPart>();
            List<VodPlaylistPart> deleteEnd = new List<VodPlaylistPart>();

            if ( cropStart ) {
                Double lengthSum = 0;

                foreach ( VodPlaylistPart part in vodPlaylist ) {
                    var partLength = part.Length;

                    if ( lengthSum + partLength < start ) {
                        lengthSum += partLength;
                        deleteStart.Add( part );
                    }
                    else {
                        start = Math.Round( start - lengthSum, 3 );
                        break;
                    }
                }
            }

            if ( cropEnd ) {
                Double lengthSum = 0;

                foreach ( VodPlaylistPart part in vodPlaylist ) {
                    if ( lengthSum >= end ) {
                        deleteEnd.Add( part );
                    }

                    lengthSum += part.Length;
                }
            }

            deleteStart.ForEach( part => {
                vodPlaylist.Remove( part );
            } );

            deleteEnd.ForEach( part => {
                vodPlaylist.Remove( part );
            } );

            return new CropInfo( cropStart, cropEnd, cropStart ? start : 0, length );
        }

        private void DownloadParts( Action<String> log, Action<String> setStatus, Action<Double> setProgress,
            VodPlaylist vodPlaylist, CancellationToken cancellationToken ) {
            var partsCount = vodPlaylist.Count;
            var maxConnectionCount = ServicePointManager.DefaultConnectionLimit;

            log( Environment.NewLine + Environment.NewLine + "Starting parallel video chunk download" );
            log( Environment.NewLine + "Number of video chunks to download: " + partsCount );
            log( Environment.NewLine + "Maximum connection count: " + maxConnectionCount );

            setStatus( "Downloading" );

            log( Environment.NewLine + Environment.NewLine + "Parallel video chunk download is running..." );

            Int64 completedPartDownloads = 0;

            Parallel.ForEach( vodPlaylist, new ParallelOptions() { MaxDegreeOfParallelism = maxConnectionCount - 1 }, ( part, loopState ) => {
                var retryCounter = 0;

                var success = false;

                do {
                    try {
                        using ( WebClient downloadClient = new WebClient() ) {
                            Byte[] bytes = downloadClient.DownloadData( part.RemoteFile );

                            Interlocked.Increment( ref completedPartDownloads );

                            FileSystem.DeleteFile( part.LocalFile );

                            File.WriteAllBytes( part.LocalFile, bytes );

                            var completed = Interlocked.Read( ref completedPartDownloads );

                            setProgress( ( Double )completed / partsCount * 100 );

                            success = true;
                        }
                    }
                    catch ( WebException ex ) {
                        if ( retryCounter < DOWNLOAD_RETRIES ) {
                            retryCounter++;
                            log( Environment.NewLine + Environment.NewLine + "Downloading file '" + part.RemoteFile + "' failed! Trying again in " + DOWNLOAD_RETRY_TIME + "s" );
                            log( Environment.NewLine + ex.ToString() );
                            Thread.Sleep( DOWNLOAD_RETRY_TIME * 1000 );
                        }
                        else {
                            throw new ApplicationException( "Could not download file '" + part.RemoteFile + "' after " + DOWNLOAD_RETRIES + " retries!" );
                        }
                    }
                }
                while ( !success );

                if ( cancellationToken.IsCancellationRequested ) {
                    loopState.Stop();
                }
            } );

            setProgress( 100 );

            log( Environment.NewLine + Environment.NewLine + "Download of all video chunks complete!" );
        }

        private void Downloads_CollectionChanged( Object sender, NotifyCollectionChangedEventArgs e ) {
            this.FireDownloadsCountChanged();
        }

        private void DownloadTimerCallback( Object state ) {
            if ( this._paused ) {
                return;
            }

            this.StartQueuedDownloadIfExists();
        }

        private void FireDownloadsCountChanged() {
            this._eventAggregator.GetEvent<DownloadsCountChangedEvent>().Publish( this._downloads != null ? this._downloads.Count : 0 );
        }

        private void FireIsAuthorizedChanged() {
            this._runtimeDataService.RuntimeData.AccessToken = this._twitchAuthInfo?.AccessToken;
            this._runtimeDataService.Save();

            this.FirePropertyChanged( nameof( this.IsAuthorized ) );
            this._eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Publish( this.IsAuthorized );
        }

        private void FireVideosCountChanged() {
            this._eventAggregator.GetEvent<VideosCountChangedEvent>().Publish( this._videos != null ? this._videos.Count : 0 );
        }

        private TwitchVideo GetTwitchVideoFromId( Int32 id ) {
            using ( WebClient webClient = this.CreatePublicApiWebClient() ) {
                try {
                    var result = webClient.DownloadString( String.Format( VIDEO_URL, id ) );

                    JObject videoJson = JObject.Parse( result );

                    if ( videoJson != null ) {
                        return this.ParseVideo( videoJson );
                    }
                }
                catch ( WebException ex ) {
                    if ( ex.Response is HttpWebResponse resp && resp.StatusCode == HttpStatusCode.NotFound ) {
                        return null;
                    }
                    else {
                        throw;
                    }
                }
            }

            return null;
        }

        private Int32? GetVideoIdFromUrl( String url ) {
            if ( String.IsNullOrWhiteSpace( url ) ) {
                return null;
            }

            if ( !Uri.TryCreate( url, UriKind.Absolute, out Uri validUrl ) ) {
                return null;
            }

            String[] segments = validUrl.Segments;

            if ( segments.Length < 2 ) {
                return null;
            }

            for ( var i = 0; i < segments.Length; i++ ) {
                if ( segments[ i ].Equals( "videos/", StringComparison.OrdinalIgnoreCase ) ) {
                    if ( segments.Length > ( i + 1 ) ) {
                        var idStr = segments[ i + 1 ];

                        if ( !String.IsNullOrWhiteSpace( idStr ) ) {
                            idStr = idStr.Trim( new Char[] { '/' } );

                            if ( Int32.TryParse( idStr, out Int32 idInt ) && idInt > 0 ) {
                                return idInt;
                            }
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private String RetrievePlaylistUrlForQuality( Action<String> log, TwitchVideoQuality quality, String vodId, VodAuthInfo vodAuthInfo ) {
            using ( WebClient webClient = this.CreatePrivateApiWebClient() ) {
                webClient.Headers.Add( "Accept", "*/*" );
                webClient.Headers.Add( "Accept-Encoding", "gzip, deflate, br" );

                log( Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities..." );
                var allPlaylistsStr = webClient.DownloadString( String.Format( ALL_PLAYLISTS_URL, vodId, vodAuthInfo.Signature, vodAuthInfo.Token ) );
                log( " done!" );

                List<String> allPlaylistsList = allPlaylistsStr.Split( new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries ).Where( s => !s.StartsWith( "#" ) ).ToList();

                allPlaylistsList.ForEach( url => {
                    log( Environment.NewLine + url );
                } );

                var playlistUrl = allPlaylistsList.Where( s => s.ToLowerInvariant().Contains( "/" + quality.QualityId + "/" ) ).First();

                log( Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + quality.DisplayString + " is " + playlistUrl );

                return playlistUrl;
            }
        }

        private VodPlaylist RetrieveVodPlaylist( Action<String> log, String tempDir, String playlistUrl ) {
            using ( WebClient webClient = new WebClient() ) {
                log( Environment.NewLine + Environment.NewLine + "Retrieving playlist..." );
                var playlistStr = webClient.DownloadString( playlistUrl );
                log( " done!" );

                if ( String.IsNullOrWhiteSpace( playlistStr ) ) {
                    throw new ApplicationException( "The playlist is empty!" );
                }

                var urlPrefix = playlistUrl.Substring( 0, playlistUrl.LastIndexOf( "/" ) + 1 );

                log( Environment.NewLine + "Parsing playlist..." );
                VodPlaylist vodPlaylist = VodPlaylist.Parse( tempDir, playlistStr, urlPrefix );
                log( " done!" );

                log( Environment.NewLine + "Number of video chunks: " + vodPlaylist.Count() );

                return vodPlaylist;
            }
        }

        private void SearchChannel( String channel, VideoType videoType, LoadLimitType loadLimit, DateTime loadFrom, DateTime loadTo, Int32 loadLastVods ) {
            if ( String.IsNullOrWhiteSpace( channel ) ) {
                throw new ArgumentNullException( nameof( channel ) );
            }

            var channelId = this.GetChannelIdByName( channel );

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            String broadcastTypeParam = null;

            if ( videoType == VideoType.Broadcast ) {
                broadcastTypeParam = "archive";
            }
            else if ( videoType == VideoType.Highlight ) {
                broadcastTypeParam = "highlight";
            }
            else if ( videoType == VideoType.Upload ) {
                broadcastTypeParam = "upload";
            }
            else {
                throw new ApplicationException( "Unsupported video type '" + videoType.ToString() + "'" );
            }

            var channelVideosUrl = String.Format( CHANNEL_VIDEOS_URL, channelId );

            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;

            if ( loadLimit == LoadLimitType.Timespan ) {
                fromDate = loadFrom;
                toDate = loadTo;
            }

            var offset = 0;
            var total = 0;
            var sum = 0;

            var stop = false;

            do {
                using ( WebClient webClient = this.CreatePublicApiWebClient() ) {
                    webClient.QueryString.Add( "broadcast_type", broadcastTypeParam );
                    webClient.QueryString.Add( "limit", TWITCH_MAX_LOAD_LIMIT.ToString() );
                    webClient.QueryString.Add( "offset", offset.ToString() );

                    var result = webClient.DownloadString( channelVideosUrl );

                    JObject videosResponseJson = JObject.Parse( result );

                    if ( videosResponseJson != null ) {
                        if ( total == 0 ) {
                            total = videosResponseJson.Value<Int32>( "_total" );
                        }

                        foreach ( JObject videoJson in videosResponseJson.Value<JArray>( "videos" ) ) {
                            sum++;

                            if ( videoJson.Value<String>( "_id" ).StartsWith( "v" ) ) {
                                TwitchVideo video = this.ParseVideo( videoJson );

                                if ( loadLimit == LoadLimitType.LastVods ) {
                                    videos.Add( video );

                                    if ( sum >= loadLastVods ) {
                                        stop = true;
                                        break;
                                    }
                                }
                                else {
                                    DateTime recordedDate = video.RecordedDate;

                                    if ( recordedDate.Date >= fromDate.Date && recordedDate.Date <= toDate.Date ) {
                                        videos.Add( video );
                                    }

                                    if ( recordedDate.Date < fromDate.Date ) {
                                        stop = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                offset += TWITCH_MAX_LOAD_LIMIT;
            } while ( !stop && sum < total );

            this.Videos = videos;
        }

        private void SearchIds( String ids ) {
            if ( String.IsNullOrWhiteSpace( ids ) ) {
                throw new ArgumentNullException( nameof( ids ) );
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            String[] idsArr = ids.Split( new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

            if ( idsArr.Length > 0 ) {
                HashSet<Int32> addedIds = new HashSet<Int32>();

                foreach ( var id in idsArr ) {
                    if ( Int32.TryParse( id, out Int32 idInt ) && !addedIds.Contains( idInt ) ) {
                        TwitchVideo video = this.GetTwitchVideoFromId( idInt );

                        if ( video != null ) {
                            videos.Add( video );
                            addedIds.Add( idInt );
                        }
                    }
                }
            }

            this.Videos = videos;
        }

        private void SearchUrls( String urls ) {
            if ( String.IsNullOrWhiteSpace( urls ) ) {
                throw new ArgumentNullException( nameof( urls ) );
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            String[] urlArr = urls.Split( new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );

            if ( urlArr.Length > 0 ) {
                HashSet<Int32> addedIds = new HashSet<Int32>();

                foreach ( var url in urlArr ) {
                    Int32? id = this.GetVideoIdFromUrl( url );

                    if ( id.HasValue && !addedIds.Contains( id.Value ) ) {
                        TwitchVideo video = this.GetTwitchVideoFromId( id.Value );

                        if ( video != null ) {
                            videos.Add( video );
                            addedIds.Add( id.Value );
                        }
                    }
                }
            }

            this.Videos = videos;
        }

        private void StartQueuedDownloadIfExists() {
            if ( this._paused ) {
                return;
            }

            if ( Monitor.TryEnter( this._changeDownloadLockObject ) ) {
                try {
                    if ( !this._downloads.Where( d => d.DownloadState == DownloadState.Downloading ).Any() ) {
                        TwitchVideoDownload download = this._downloads.Where( d => d.DownloadState == DownloadState.Queued ).FirstOrDefault();

                        if ( download == null ) {
                            return;
                        }

                        DownloadParameters downloadParams = download.DownloadParams;

                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken cancellationToken = cancellationTokenSource.Token;

                        var downloadId = download.Id;
                        var vodId = downloadParams.Video.Id;
                        var tempDir = Path.Combine( this._preferencesService.CurrentPreferences.DownloadTempFolder, TEMP_PREFIX + downloadId );
                        var ffmpegFile = this._processingService.FFMPEGExe;
                        var concatFile = Path.Combine( tempDir, Path.GetFileNameWithoutExtension( downloadParams.FullPath ) + ".ts" );
                        var outputFile = downloadParams.FullPath;

                        var disableConversion = downloadParams.DisableConversion;
                        var cropStart = downloadParams.CropStart;
                        var cropEnd = downloadParams.CropEnd;

                        TimeSpan cropStartTime = downloadParams.CropStartTime;
                        TimeSpan cropEndTime = downloadParams.CropEndTime;

                        TwitchVideoQuality quality = downloadParams.Quality;

                        VodAuthInfo vodAuthInfo = downloadParams.VodAuthInfo;

                        Action<DownloadState> setDownloadState = download.SetDownloadState;
                        Action<String> log = download.AppendLog;
                        Action<String> setStatus = download.SetStatus;
                        Action<Double> setProgress = download.SetProgress;
                        Action<Boolean> setIsIndeterminate = download.SetIsIndeterminate;

                        Task downloadVideoTask = new Task( () => {
                            setStatus( "Initializing" );

                            log( "Download task has been started!" );

                            this.WriteDownloadInfo( log, downloadParams, ffmpegFile, tempDir );

                            this.CheckTempDirectory( log, tempDir );

                            cancellationToken.ThrowIfCancellationRequested();

                            var playlistUrl = this.RetrievePlaylistUrlForQuality( log, quality, vodId, vodAuthInfo );

                            cancellationToken.ThrowIfCancellationRequested();

                            VodPlaylist vodPlaylist = this.RetrieveVodPlaylist( log, tempDir, playlistUrl );

                            cancellationToken.ThrowIfCancellationRequested();

                            CropInfo cropInfo = this.CropVodPlaylist( vodPlaylist, cropStart, cropEnd, cropStartTime, cropEndTime );

                            cancellationToken.ThrowIfCancellationRequested();

                            this.DownloadParts( log, setStatus, setProgress, vodPlaylist, cancellationToken );

                            cancellationToken.ThrowIfCancellationRequested();

                            this._processingService.ConcatParts( log, setStatus, setProgress, vodPlaylist, disableConversion ? outputFile : concatFile );

                            if ( !disableConversion ) {
                                cancellationToken.ThrowIfCancellationRequested();
                                this._processingService.ConvertVideo( log, setStatus, setProgress, setIsIndeterminate, concatFile, outputFile, cropInfo );
                            }
                        }, cancellationToken );

                        Task continueTask = downloadVideoTask.ContinueWith( task => {
                            log( Environment.NewLine + Environment.NewLine + "Starting temporary download folder cleanup!" );
                            this.CleanUp( tempDir, log );

                            setProgress( 100 );
                            setIsIndeterminate( false );

                            var success = false;

                            if ( task.IsFaulted ) {
                                setDownloadState( DownloadState.Error );
                                log( Environment.NewLine + Environment.NewLine + "Download task ended with an error!" );

                                if ( task.Exception != null ) {
                                    log( Environment.NewLine + Environment.NewLine + task.Exception.ToString() );
                                }
                            }
                            else if ( task.IsCanceled ) {
                                setDownloadState( DownloadState.Canceled );
                                log( Environment.NewLine + Environment.NewLine + "Download task was canceled!" );
                            }
                            else {
                                success = true;
                                setDownloadState( DownloadState.Done );
                                log( Environment.NewLine + Environment.NewLine + "Download task ended successfully!" );
                            }

                            if ( !this._downloadTasks.TryRemove( downloadId, out DownloadTask downloadTask ) ) {
                                throw new ApplicationException( "Could not remove download task with ID '" + downloadId + "' from download task collection!" );
                            }

                            if ( success && this._preferencesService.CurrentPreferences.DownloadRemoveCompleted ) {
                                this._eventAggregator.GetEvent<RemoveDownloadEvent>().Publish( downloadId );
                            }
                        } );

                        if ( this._downloadTasks.TryAdd( downloadId, new DownloadTask( downloadVideoTask, continueTask, cancellationTokenSource ) ) ) {
                            downloadVideoTask.Start();
                            setDownloadState( DownloadState.Downloading );
                        }
                    }
                }
                finally {
                    Monitor.Exit( this._changeDownloadLockObject );
                }
            }
        }

        private void Videos_CollectionChanged( Object sender, NotifyCollectionChangedEventArgs e ) {
            this.FireVideosCountChanged();
        }

        private void WriteDownloadInfo( Action<String> log, DownloadParameters downloadParams, String ffmpegFile, String tempDir ) {
            log( Environment.NewLine + Environment.NewLine + "TWITCH LEECHER INFO" );
            log( Environment.NewLine + "--------------------------------------------------------------------------------------------" );
            log( Environment.NewLine + "Version: " + AssemblyUtil.Get.GetAssemblyVersion().Trim() );

            log( Environment.NewLine + Environment.NewLine + "VOD INFO" );
            log( Environment.NewLine + "--------------------------------------------------------------------------------------------" );
            log( Environment.NewLine + "VOD ID: " + downloadParams.Video.Id );
            log( Environment.NewLine + "Selected Quality: " + downloadParams.Quality.DisplayString );
            log( Environment.NewLine + "Download Url: " + downloadParams.Video.Url );
            log( Environment.NewLine + "Crop Start: " + ( downloadParams.CropStart ? "Yes (" + downloadParams.CropStartTime.ToDaylessString() + ")" : "No" ) );
            log( Environment.NewLine + "Crop End: " + ( downloadParams.CropEnd ? "Yes (" + downloadParams.CropEndTime.ToDaylessString() + ")" : "No" ) );

            log( Environment.NewLine + Environment.NewLine + "OUTPUT INFO" );
            log( Environment.NewLine + "--------------------------------------------------------------------------------------------" );
            log( Environment.NewLine + "Disable Conversion: " + ( downloadParams.DisableConversion ? "Yes" : "No" ) );
            log( Environment.NewLine + "Output File: " + downloadParams.FullPath );
            log( Environment.NewLine + "FFMPEG Path: " + ffmpegFile );
            log( Environment.NewLine + "Temporary Download Folder: " + tempDir );

            VodAuthInfo vodAuthInfo = downloadParams.VodAuthInfo;

            log( Environment.NewLine + Environment.NewLine + "ACCESS INFO" );
            log( Environment.NewLine + "--------------------------------------------------------------------------------------------" );
            log( Environment.NewLine + "Token: " + vodAuthInfo.Token );
            log( Environment.NewLine + "Signature: " + vodAuthInfo.Signature );
            log( Environment.NewLine + "Sub-Only: " + ( vodAuthInfo.SubOnly ? "Yes" : "No" ) );
            log( Environment.NewLine + "Privileged: " + ( vodAuthInfo.Privileged ? "Yes" : "No" ) );
        }

        protected virtual void Dispose( Boolean disposing ) {
            if ( !this.disposedValue ) {
                if ( disposing ) {
                    this._downloadTimer.Dispose();
                }

                this._videos = null;
                this._downloads = null;
                this._downloadTasks = null;

                this.disposedValue = true;
            }
        }

        public Boolean Authorize( String accessToken ) {
            if ( !String.IsNullOrWhiteSpace( accessToken ) ) {
                using ( WebClient webClient = this.CreatePublicApiWebClient() ) {
                    webClient.Headers.Add( TWITCH_AUTHORIZATION_HEADER, String.Format( TWITCH_AUTHORIZATION_VALUE, accessToken ) );

                    var result = webClient.DownloadString( KRAKEN_URL );

                    JObject verifyRequestJson = JObject.Parse( result );

                    if ( verifyRequestJson != null ) {
                        JObject tokenJson = verifyRequestJson.Value<JObject>( "token" );

                        if ( tokenJson != null ) {
                            Boolean valid = tokenJson.Value<Boolean>( "valid" );

                            if ( valid ) {
                                String username = tokenJson.Value<String>( "user_name" );
                                String clientId = tokenJson.Value<String>( "client_id" );

                                if ( !String.IsNullOrWhiteSpace( username ) &&
                                    !String.IsNullOrWhiteSpace( clientId ) &&
                                    clientId.Equals( TWITCH_CLIENT_ID, StringComparison.OrdinalIgnoreCase ) ) {
                                    this._twitchAuthInfo = new TwitchAuthInfo( accessToken, username );
                                    this.FireIsAuthorizedChanged();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            this.RevokeAuthorization();
            return false;
        }

        public void Cancel( String id ) {
            lock ( this._changeDownloadLockObject ) {
                if ( this._downloadTasks.TryGetValue( id, out DownloadTask downloadTask ) ) {
                    downloadTask.CancellationTokenSource.Cancel();
                }
            }
        }

        public Boolean CanShutdown() {
            Monitor.Enter( this._changeDownloadLockObject );

            try {
                return !this._downloads.Where( d => d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued ).Any();
            }
            finally {
                Monitor.Exit( this._changeDownloadLockObject );
            }
        }

        public Boolean ChannelExists( String channel ) {
            if ( String.IsNullOrWhiteSpace( channel ) ) {
                throw new ArgumentNullException( nameof( channel ) );
            }

            return this.GetChannelIdByName( channel ) != null;
        }

        public void Dispose() {
            this.Dispose( true );
        }

        public void Enqueue( DownloadParameters downloadParams ) {
            if ( this._paused ) {
                return;
            }

            lock ( this._changeDownloadLockObject ) {
                this._downloads.Add( new TwitchVideoDownload( downloadParams ) );
            }
        }

        public String GetChannelIdByName( String channel ) {
            if ( String.IsNullOrWhiteSpace( channel ) ) {
                throw new ArgumentNullException( nameof( channel ) );
            }

            using ( WebClient webClient = this.CreatePublicApiWebClient() ) {
                webClient.QueryString.Add( "login", channel );

                String result = null;

                try {
                    result = webClient.DownloadString( USERS_URL );
                }
                catch ( WebException ) {
                    return null;
                }

                if ( !String.IsNullOrWhiteSpace( result ) ) {
                    JObject searchResultJson = JObject.Parse( result );

                    JArray usersJson = searchResultJson.Value<JArray>( "users" );

                    if ( usersJson != null && usersJson.HasValues ) {
                        JToken userJson = usersJson.FirstOrDefault();

                        if ( userJson != null ) {
                            String id = userJson.Value<String>( "_id" );

                            if ( !String.IsNullOrWhiteSpace( id ) ) {
                                using ( WebClient webClientChannel = this.CreatePublicApiWebClient() ) {
                                    try {
                                        webClientChannel.DownloadString( String.Format( CHANNEL_URL, id ) );

                                        return id;
                                    }
                                    catch ( WebException ) {
                                        return null;
                                    }
                                    catch ( Exception ) {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }

        public Uri GetGameThumbnail( String game ) {
            Uri unknownGameUri = new Uri( UNKNOWN_GAME_URL );

            if ( String.IsNullOrWhiteSpace( game ) ) {
                return unknownGameUri;
            }

            var hashIndex = game.IndexOf( " #" );

            if ( hashIndex >= 0 ) {
                game = game.Substring( 0, game.Length - ( game.Length - hashIndex ) );
            }

            var gameLower = game.ToLowerInvariant();

            if ( this._gameThumbnails == null ) {
                this.InitGameThumbnails();
            }

            if ( this._gameThumbnails.TryGetValue( gameLower, out Uri thumb ) ) {
                return thumb;
            }

            return unknownGameUri;
        }

        public void InitGameThumbnails() {
            this._gameThumbnails = new Dictionary<String, Uri>();

            try {
                var offset = 0;
                var total = 0;

                do {
                    using ( WebClient webClient = this.CreatePublicApiWebClient() ) {
                        webClient.QueryString.Add( "limit", TWITCH_MAX_LOAD_LIMIT.ToString() );
                        webClient.QueryString.Add( "offset", offset.ToString() );

                        var result = webClient.DownloadString( GAMES_URL );

                        JObject gamesResponseJson = JObject.Parse( result );

                        if ( total == 0 ) {
                            total = gamesResponseJson.Value<Int32>( "_total" );
                        }

                        foreach ( JObject gamesJson in gamesResponseJson.Value<JArray>( "top" ) ) {
                            JObject gameJson = gamesJson.Value<JObject>( "game" );

                            String name = gameJson.Value<String>( "name" ).ToLowerInvariant();
                            Uri gameThumb = new Uri( gameJson.Value<JObject>( "box" ).Value<String>( "medium" ) );

                            if ( !this._gameThumbnails.ContainsKey( name ) ) {
                                this._gameThumbnails.Add( name, gameThumb );
                            }
                        }
                    }

                    offset += TWITCH_MAX_LOAD_LIMIT;
                } while ( offset < total );
            }
            catch {

                // Thumbnail loading should not affect the rest of the application
            }
        }

        public Boolean IsFileNameUsed( String fullPath ) {
            IEnumerable<TwitchVideoDownload> downloads = this._downloads.Where( d => d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued );

            foreach ( TwitchVideoDownload download in downloads ) {
                if ( download.DownloadParams.FullPath.Equals( fullPath, StringComparison.OrdinalIgnoreCase ) ) {
                    return true;
                }
            }

            return false;
        }

        public List<TwitchVideoQuality> ParseQualities( JObject resolutionsJson, JObject fpsJson ) {
            List<TwitchVideoQuality> qualities = new List<TwitchVideoQuality>();

            Dictionary<String, String> fpsList = new Dictionary<String, String>();

            if ( fpsJson != null ) {
                foreach ( JProperty fps in fpsJson.Values<JProperty>() ) {
                    fpsList.Add( fps.Name, ( ( Int32 )Math.Round( fps.Value.Value<Double>(), 0 ) ).ToString() );
                }
            }

            if ( resolutionsJson != null ) {
                foreach ( JProperty resolution in resolutionsJson.Values<JProperty>() ) {
                    String value = resolution.Value.Value<String>();
                    String qualityId = resolution.Name;
                    var fps = fpsList.ContainsKey( qualityId ) ? fpsList[ qualityId ] : null;

                    qualities.Add( new TwitchVideoQuality( qualityId, value, fps ) );
                }
            }

            if ( fpsList.ContainsKey( TwitchVideoQuality.QUALITY_AUDIO ) ) {
                qualities.Add( new TwitchVideoQuality( TwitchVideoQuality.QUALITY_AUDIO ) );
            }

            if ( !qualities.Any() ) {
                qualities.Add( new TwitchVideoQuality( TwitchVideoQuality.QUALITY_SOURCE ) );
            }

            qualities.Sort();

            return qualities;
        }

        public TwitchVideo ParseVideo( JObject videoJson ) {
            String channel = videoJson.Value<JObject>( "channel" ).Value<String>( "display_name" );
            String title = videoJson.Value<String>( "title" );
            String id = videoJson.Value<String>( "_id" );
            String game = videoJson.Value<String>( "game" );
            Int32 views = videoJson.Value<Int32>( "views" );
            TimeSpan length = new TimeSpan( 0, 0, videoJson.Value<Int32>( "length" ) );
            List<TwitchVideoQuality> qualities = ParseQualities( videoJson.Value<JObject>( "resolutions" ), videoJson.Value<JObject>( "fps" ) );
            Uri url = new Uri( videoJson.Value<String>( "url" ) );
            Uri thumbnail = new Uri( videoJson.Value<JObject>( "preview" ).Value<String>( "large" ) );
            Uri gameThumbnail = this.GetGameThumbnail( game );

            String dateStr = videoJson.Value<String>( "published_at" );

            if ( String.IsNullOrWhiteSpace( dateStr ) ) {
                dateStr = videoJson.Value<String>( "created_at" );
            }

            DateTime recordedDate = DateTime.Parse( dateStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal );

            if ( id.StartsWith( "v", StringComparison.OrdinalIgnoreCase ) ) {
                id = id.Substring( 1 );
            }

            return new TwitchVideo( channel, title, id, game, views, length, qualities, recordedDate, thumbnail, gameThumbnail, url );
        }

        public void Pause() {
            this._paused = true;
            this._downloadTimer.Change( Timeout.Infinite, Timeout.Infinite );
        }

        public void Remove( String id ) {
            lock ( this._changeDownloadLockObject ) {
                if ( !this._downloadTasks.TryGetValue( id, out DownloadTask downloadTask ) ) {
                    TwitchVideoDownload download = this._downloads.Where( d => d.Id == id ).FirstOrDefault();

                    if ( download != null ) {
                        this._downloads.Remove( download );
                    }
                }
            }
        }

        public void Resume() {
            this._paused = false;
            this._downloadTimer.Change( 0, TIMER_INTERVALL );
        }

        public VodAuthInfo RetrieveVodAuthInfo( String id ) {
            if ( String.IsNullOrWhiteSpace( id ) ) {
                throw new ArgumentNullException( nameof( id ) );
            }

            using ( WebClient webClient = this.CreatePrivateApiWebClient() ) {
                if ( this.IsAuthorized ) {
                    webClient.QueryString.Add( TWITCH_AUTHORIZATION_PARAM, this._twitchAuthInfo.AccessToken );
                    webClient.Headers.Add( TWITCH_AUTHORIZATION_HEADER, String.Format( TWITCH_AUTHORIZATION_VALUE, this._twitchAuthInfo.AccessToken ) );
                }

                var accessTokenStr = webClient.DownloadString( String.Format( ACCESS_TOKEN_URL, id ) );

                JObject accessTokenJson = JObject.Parse( accessTokenStr );

                var token = Uri.EscapeDataString( accessTokenJson.Value<String>( "token" ) );
                String signature = accessTokenJson.Value<String>( "sig" );

                if ( String.IsNullOrWhiteSpace( token ) ) {
                    throw new ApplicationException( "VOD access token is null!" );
                }

                if ( String.IsNullOrWhiteSpace( signature ) ) {
                    throw new ApplicationException( "VOD signature is null!" );
                }

                var privileged = false;
                var subOnly = false;

                JObject tokenJson = JObject.Parse( HttpUtility.UrlDecode( token ) );

                if ( tokenJson == null ) {
                    throw new ApplicationException( "Decoded VOD access token is null!" );
                }

                privileged = tokenJson.Value<Boolean>( "privileged" );

                if ( privileged ) {
                    subOnly = true;
                }
                else {
                    JObject chansubJson = tokenJson.Value<JObject>( "chansub" );

                    if ( chansubJson == null ) {
                        throw new ApplicationException( "Token property 'chansub' is null!" );
                    }

                    JArray restrictedQualitiesJson = chansubJson.Value<JArray>( "restricted_bitrates" );

                    if ( restrictedQualitiesJson == null ) {
                        throw new ApplicationException( "Token property 'chansub -> restricted_bitrates' is null!" );
                    }

                    if ( restrictedQualitiesJson.Count > 0 ) {
                        subOnly = true;
                    }
                }

                return new VodAuthInfo( token, signature, privileged, subOnly );
            }
        }

        public void Retry( String id ) {
            if ( this._paused ) {
                return;
            }

            lock ( this._changeDownloadLockObject ) {
                if ( !this._downloadTasks.TryGetValue( id, out DownloadTask downloadTask ) ) {
                    TwitchVideoDownload download = this._downloads.Where( d => d.Id == id ).FirstOrDefault();

                    if ( download != null && ( download.DownloadState == DownloadState.Canceled || download.DownloadState == DownloadState.Error ) ) {
                        download.ResetLog();
                        download.SetProgress( 0 );
                        download.SetDownloadState( DownloadState.Queued );
                        download.SetStatus( "Initializing" );
                    }
                }
            }
        }

        public void RevokeAuthorization() {
            this._twitchAuthInfo = null;
            this.FireIsAuthorizedChanged();
        }

        public void Search( SearchParameters searchParams ) {
            if ( searchParams == null ) {
                throw new ArgumentNullException( nameof( searchParams ) );
            }

            switch ( searchParams.SearchType ) {
                case SearchType.Channel:
                    this.SearchChannel( searchParams.Channel, searchParams.VideoType, searchParams.LoadLimitType, searchParams.LoadFrom.Value, searchParams.LoadTo.Value, searchParams.LoadLastVods );
                    break;

                case SearchType.Urls:
                    this.SearchUrls( searchParams.Urls );
                    break;

                case SearchType.Ids:
                    this.SearchIds( searchParams.Ids );
                    break;
            }
        }

        public void Shutdown() {
            this.Pause();

            foreach ( DownloadTask downloadTask in this._downloadTasks.Values ) {
                downloadTask.CancellationTokenSource.Cancel();
            }

            List<Task> tasks = this._downloadTasks.Values.Select( v => v.Task ).ToList();
            tasks.AddRange( this._downloadTasks.Values.Select( v => v.ContinueTask ).ToList() );

            try {
                Task.WaitAll( tasks.ToArray() );
            }
            catch ( Exception ) {

                // Don't care about aborted tasks
            }

            List<String> toRemove = this._downloads.Select( d => d.Id ).ToList();

            foreach ( var id in toRemove ) {
                this.Remove( id );
            }
        }
    }
}