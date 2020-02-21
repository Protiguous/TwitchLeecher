namespace TwitchLeecher.Core.Models {

    using System;
    using System.Text;
    using TwitchLeecher.Core.Enums;
    using TwitchLeecher.Shared.Notification;

    public class TwitchVideoDownload : BindableBase {

        private readonly Object _downloadStateLockObject;
        private readonly Object _isIndeterminateLockObject;
        private readonly Object _logLockObject;
        private readonly Object _progressLockObject;
        private readonly Object _statusLockObject;
        private DownloadState _downloadState;
        private Boolean _isIndeterminate;
        private StringBuilder _log;
        private Double _progress;
        private String _status;

        public Boolean CanRetry {
            get {
                return this.DownloadState == DownloadState.Canceled || this.DownloadState == DownloadState.Error;
            }
        }

        public DownloadParameters DownloadParams { get; }

        public DownloadState DownloadState {
            get {
                return this._downloadState;
            }

            private set {
                this._downloadState = value;
                this.FirePropertyChanged( nameof( this.DownloadState ) );
                this.FirePropertyChanged( nameof( this.CanRetry ) );
                this.FirePropertyChanged( nameof( this.Status ) );
            }
        }

        public String Id { get; }

        public Boolean IsIndeterminate {
            get {
                return this._isIndeterminate;
            }

            private set {
                this.SetProperty( ref this._isIndeterminate, value );
            }
        }

        public String Log {
            get {
                lock ( this._logLockObject ) {
                    return this._log.ToString();
                }
            }
        }

        public Double Progress {
            get {
                return this._progress;
            }

            private set {
                this.SetProperty( ref this._progress, value );
            }
        }

        public String Status {
            get {
                if ( this._downloadState != DownloadState.Downloading ) {
                    return this._downloadState.ToString();
                }

                return this._status;
            }

            private set {
                this.SetProperty( ref this._status, value );
            }
        }

        public TwitchVideoDownload( DownloadParameters downloadParams ) {
            this.Id = Guid.NewGuid().ToString();
            this.DownloadParams = downloadParams ?? throw new ArgumentNullException( nameof( downloadParams ) );

            this._log = new StringBuilder();

            this._downloadStateLockObject = new Object();
            this._logLockObject = new Object();
            this._progressLockObject = new Object();
            this._statusLockObject = new Object();
            this._isIndeterminateLockObject = new Object();
        }

        public void AppendLog( String text ) {
            lock ( this._logLockObject ) {
                this._log.Append( text );
                this.FirePropertyChanged( nameof( this.Log ) );
            }
        }

        public void ResetLog() {
            lock ( this._logLockObject ) {
                this._log.Clear();
                this.FirePropertyChanged( nameof( this.Log ) );
            }
        }

        public void SetDownloadState( DownloadState downloadState ) {
            lock ( this._downloadStateLockObject ) {
                this.DownloadState = downloadState;
            }
        }

        public void SetIsIndeterminate( Boolean isIndeterminate ) {
            lock ( this._isIndeterminateLockObject ) {
                this.IsIndeterminate = isIndeterminate;
            }
        }

        public void SetProgress( Double progress ) {
            lock ( this._progressLockObject ) {
                this.Progress = progress;
            }
        }

        public void SetStatus( String status ) {
            lock ( this._statusLockObject ) {
                this.Status = status;
            }
        }
    }
}