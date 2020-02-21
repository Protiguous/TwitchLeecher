namespace TwitchLeecher.Core.Models {

    using System;

    public class TwitchVideoQuality : IComparable<TwitchVideoQuality> {

        public const String QUALITY_AUDIO = "audio_only";
        public const String QUALITY_HIGH = "high";
        public const String QUALITY_LOW = "low";
        public const String QUALITY_MEDIUM = "medium";
        public const String QUALITY_MOBILE = "mobile";
        public const String QUALITY_SOURCE = "chunked";
        public const String UNKNOWN = "Unknown";

        public String DisplayString { get; private set; }

        public Int32? Fps { get; private set; }

        public Boolean IsSource {
            get {
                return this.QualityId == QUALITY_SOURCE;
            }
        }

        public String QualityId { get; private set; }

        public String QualityString { get; private set; }

        public String ResFpsString { get; private set; }

        public String Resolution { get; private set; }

        public TwitchVideoQuality( String qualityId, String resolution = null, String fps = null ) {
            if ( String.IsNullOrWhiteSpace( qualityId ) ) {
                throw new ArgumentNullException( nameof( qualityId ) );
            }

            this.Initialize( qualityId, resolution, fps );
        }

        private Int32? GetFps( String qualityId, String fps ) {
            var start = qualityId.IndexOf( "p" ) + 1;

            if ( start > 0 && start < qualityId.Length ) {
                Int32? qualityFps = Decimal.TryParse( qualityId.Substring( start, qualityId.Length - start ), out Decimal qualityFpsDec ) ? ( Int32? )Math.Round( qualityFpsDec, 0 ) : null;

                if ( qualityFps.HasValue ) {
                    return qualityFps;
                }
            }

            return Decimal.TryParse( fps, out Decimal fpsDec ) ? ( Int32? )Math.Round( fpsDec, 0 ) : null;
        }

        private String GetResolution( String qualityId, String resolution ) {
            if ( !String.IsNullOrWhiteSpace( resolution ) ) {
                if ( resolution.Equals( "0x0", StringComparison.OrdinalIgnoreCase ) ) {
                    switch ( qualityId ) {
                        case QUALITY_HIGH:
                            return "1280x720";

                        case QUALITY_MEDIUM:
                            return "852x480";

                        case QUALITY_LOW:
                            return "640x360";

                        case QUALITY_MOBILE:
                            return "284x160";
                    }
                }
                else {
                    return resolution;
                }
            }

            return null;
        }

        private Int32? GetVerticalResolution( String resolution ) {
            if ( String.IsNullOrWhiteSpace( resolution ) || !resolution.Contains( "x" ) || resolution.IndexOf( "x" ) >= resolution.Length - 1 ) {
                return null;
            }
            else {
                var start = resolution.IndexOf( "x" ) + 1;
                return Int32.Parse( resolution.Substring( start, resolution.Length - start ) );
            }
        }

        private void Initialize( String qualityId, String resolution, String fps ) {
            this.QualityId = qualityId;
            this.QualityString = this.GetQualityString( qualityId );
            this.Resolution = this.GetResolution( qualityId, resolution );
            this.Fps = this.GetFps( qualityId, fps );

            if ( qualityId == QUALITY_AUDIO ) {
                this.DisplayString = this.QualityString;
            }
            else {
                if ( !String.IsNullOrWhiteSpace( this.Resolution ) && this.Fps.HasValue ) {
                    this.DisplayString = this.Resolution + "@" + this.Fps + "fps" + ( !String.IsNullOrWhiteSpace( this.QualityString ) ? " (" + this.QualityString + ")" : null );
                }
                else if ( !String.IsNullOrWhiteSpace( this.Resolution ) && !this.Fps.HasValue ) {
                    this.DisplayString = this.Resolution + ( !String.IsNullOrWhiteSpace( this.QualityString ) ? " (" + this.QualityString + ")" : null );
                }
                else {
                    this.DisplayString = UNKNOWN;
                }
            }

            var displayStr = this.DisplayString;
            var index = displayStr.IndexOf( " (" );

            this.ResFpsString = index >= 0 ? displayStr.Substring( 0, displayStr.Length - ( displayStr.Length - index ) ) : displayStr;
        }

        public Int32 CompareTo( TwitchVideoQuality other ) {
            if ( other == null ) {
                return -1;
            }

            if ( this.IsSource && !other.IsSource ) {
                return -1;
            }
            else if ( !this.IsSource && other.IsSource ) {
                return 1;
            }
            else {
                Int32? thisRes = this.GetVerticalResolution( this.Resolution );
                Int32? otherRes = this.GetVerticalResolution( other.Resolution );

                if ( !thisRes.HasValue && !otherRes.HasValue ) {
                    return 0;
                }
                else if ( !thisRes.HasValue && otherRes.HasValue ) {
                    return 1;
                }
                else if ( thisRes.HasValue && !otherRes.HasValue ) {
                    return -1;
                }
                else {
                    if ( thisRes.Value == otherRes.Value ) {
                        Int32? thisFps = this.Fps;
                        Int32? otherFps = other.Fps;

                        if ( !thisFps.HasValue && !otherFps.HasValue ) {
                            return 0;
                        }
                        else if ( !thisFps.HasValue && otherFps.HasValue ) {
                            return 1;
                        }
                        else if ( thisFps.HasValue && !otherFps.HasValue ) {
                            return -1;
                        }
                        else {
                            return thisFps > otherFps ? -1 : 1;
                        }
                    }
                    else {
                        return thisRes > otherRes ? -1 : 1;
                    }
                }
            }
        }

        public String GetQualityString( String qualityId ) {
            switch ( qualityId ) {
                case QUALITY_SOURCE:
                    return "Source";

                case QUALITY_HIGH:
                    return "High";

                case QUALITY_MEDIUM:
                    return "Medium";

                case QUALITY_LOW:
                    return "Low";

                case QUALITY_MOBILE:
                    return "Mobile";

                case QUALITY_AUDIO:
                    return "Audio Only";

                default:
                    return null;
            }
        }

        public override String ToString() {
            return this.DisplayString;
        }
    }
}