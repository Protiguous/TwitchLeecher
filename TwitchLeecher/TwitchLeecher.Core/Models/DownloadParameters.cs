namespace TwitchLeecher.Core.Models {

    using System;
    using System.IO;
    using TwitchLeecher.Shared.Extensions;
    using TwitchLeecher.Shared.IO;
    using TwitchLeecher.Shared.Notification;

    public class DownloadParameters : BindableBase {

        private Boolean _cropEnd;
        private TimeSpan _cropEndTime;
        private Boolean _cropStart;
        private TimeSpan _cropStartTime;
        private Boolean _disableConversion;
        private String _filename;
        private String _folder;
        private TwitchVideoQuality _quality;

        public Boolean CropEnd {
            get {
                return this._cropEnd;
            }

            set {
                this.SetProperty( ref this._cropEnd, value, nameof( this.CropEnd ) );
                this.FirePropertyChanged( nameof( this.CroppedLength ) );
            }
        }

        public TimeSpan CropEndTime {
            get {
                return this._cropEndTime;
            }

            set {
                this.SetProperty( ref this._cropEndTime, value, nameof( this.CropEndTime ) );
                this.FirePropertyChanged( nameof( this.CroppedLength ) );
            }
        }

        public TimeSpan CroppedLength {
            get {
                if ( !this._cropStart && !this._cropEnd ) {
                    return this.Video.Length;
                }
                else if ( !this._cropStart && this._cropEnd ) {
                    return this._cropEndTime;
                }
                else if ( this._cropStart && !this._cropEnd ) {
                    return this.Video.Length - this._cropStartTime;
                }
                else {
                    return this._cropEndTime - this._cropStartTime;
                }
            }
        }

        public String CroppedLengthStr {
            get {
                return this.CroppedLength.ToDaylessString();
            }
        }

        public Boolean CropStart {
            get {
                return this._cropStart;
            }

            set {
                this.SetProperty( ref this._cropStart, value, nameof( this.CropStart ) );
                this.FirePropertyChanged( nameof( this.CroppedLength ) );
            }
        }

        public TimeSpan CropStartTime {
            get {
                return this._cropStartTime;
            }

            set {
                this.SetProperty( ref this._cropStartTime, value, nameof( this.CropStartTime ) );
                this.FirePropertyChanged( nameof( this.CroppedLength ) );
            }
        }

        public Boolean DisableConversion {
            get {
                return this._disableConversion;
            }

            set {
                this.SetProperty( ref this._disableConversion, value, nameof( this.DisableConversion ) );
            }
        }

        public String Filename {
            get {
                return this._filename;
            }

            set {
                this.SetProperty( ref this._filename, value, nameof( this.Filename ) );
                this.FirePropertyChanged( nameof( this.FullPath ) );
            }
        }

        public String Folder {
            get {
                return this._folder;
            }

            set {
                this.SetProperty( ref this._folder, value, nameof( this.Folder ) );
                this.FirePropertyChanged( nameof( this.FullPath ) );
            }
        }

        public String FullPath {
            get {
                return Path.Combine( this._folder, this._filename );
            }
        }

        public TwitchVideoQuality Quality {
            get {
                return this._quality;
            }

            set {
                this.SetProperty( ref this._quality, value, nameof( this.Quality ) );
            }
        }

        public TwitchVideo Video { get; }

        public VodAuthInfo VodAuthInfo { get; }

        public DownloadParameters( TwitchVideo video, VodAuthInfo vodAuthInfo, TwitchVideoQuality quality, String folder, String filename, Boolean disableConversion ) {
            if ( String.IsNullOrWhiteSpace( folder ) ) {
                throw new ArgumentNullException( nameof( folder ) );
            }

            if ( String.IsNullOrWhiteSpace( filename ) ) {
                throw new ArgumentNullException( nameof( filename ) );
            }

            this.Video = video ?? throw new ArgumentNullException( nameof( video ) );
            this._quality = quality ?? throw new ArgumentNullException( nameof( quality ) );
            this.VodAuthInfo = vodAuthInfo ?? throw new ArgumentNullException( nameof( vodAuthInfo ) );

            this._folder = folder;
            this._filename = filename;
            this._disableConversion = disableConversion;

            this._cropEndTime = video.Length;
        }

        public override void Validate( String propertyName = null ) {
            base.Validate( propertyName );

            var currentProperty = nameof( this.Quality );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._quality == null ) {
                    this.AddError( currentProperty, "Please select a quality!" );
                }
            }

            currentProperty = nameof( this.Folder );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( String.IsNullOrWhiteSpace( this._folder ) ) {
                    this.AddError( currentProperty, "Please specify a folder!" );
                }
            }

            currentProperty = nameof( this.Filename );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( String.IsNullOrWhiteSpace( this._filename ) ) {
                    this.AddError( currentProperty, "Please specify a filename!" );
                }
                else if ( this._disableConversion && !this._filename.EndsWith( ".ts", StringComparison.OrdinalIgnoreCase ) ) {
                    this.AddError( currentProperty, "Filename must end with '.ts'!" );
                }
                else if ( !this._disableConversion && !this._filename.EndsWith( ".mp4", StringComparison.OrdinalIgnoreCase ) ) {
                    this.AddError( currentProperty, "Filename must end with '.mp4'!" );
                }
                else if ( FileSystem.FilenameContainsInvalidChars( this._filename ) ) {
                    this.AddError( currentProperty, "Filename contains invalid characters!" );
                }
            }

            currentProperty = nameof( this.CropStartTime );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._cropStart ) {
                    TimeSpan videoLength = this.Video.Length;

                    if ( this._cropStartTime < TimeSpan.Zero || this._cropStartTime > videoLength ) {
                        this.AddError( currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToDaylessString() + "'!" );
                    }
                    else if ( this.CroppedLength.TotalSeconds < 5 ) {
                        this.AddError( currentProperty, "The cropped video has to be at least 5s long!" );
                    }
                }
            }

            currentProperty = nameof( this.CropEndTime );

            if ( String.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                if ( this._cropEnd ) {
                    TimeSpan videoLength = this.Video.Length;

                    if ( this._cropEndTime < TimeSpan.Zero || this._cropEndTime > videoLength ) {
                        this.AddError( currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToDaylessString() + "'!" );
                    }
                    else if ( this._cropStart && ( this._cropEndTime <= this._cropStartTime ) ) {
                        this.AddError( currentProperty, "End time has to be greater than start time!" );
                    }
                    else if ( this.CroppedLength.TotalSeconds < 5 ) {
                        this.AddError( currentProperty, "The cropped video has to be at least 5s long!" );
                    }
                }
            }
        }
    }
}