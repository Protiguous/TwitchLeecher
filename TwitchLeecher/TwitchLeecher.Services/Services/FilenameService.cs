﻿namespace TwitchLeecher.Services.Services {

    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using TwitchLeecher.Core.Models;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Extensions;

    internal class FilenameService : IFilenameService {

        public String EnsureExtension( String filename, Boolean disableConversion ) {
            if ( String.IsNullOrWhiteSpace( filename ) ) {
                return filename;
            }

            if ( disableConversion && !filename.EndsWith( ".ts" ) ) {
                return filename + ".ts";
            }
            else if ( disableConversion && filename.EndsWith( ".mp4" ) ) {
                return filename.Substring( 0, filename.Length - 4 ) + ".ts";
            }
            else if ( !disableConversion && !filename.EndsWith( ".mp4" ) ) {
                return filename + ".mp4";
            }
            else if ( !disableConversion && filename.EndsWith( ".ts" ) ) {
                return filename.Substring( 0, filename.Length - 3 ) + ".mp4";
            }

            return filename;
        }

        public String SubstituteInvalidChars( String filename, String replaceStr ) {
            if ( String.IsNullOrWhiteSpace( filename ) ) {
                return filename;
            }

            if ( String.IsNullOrEmpty( replaceStr ) ) {
                throw new ArgumentNullException( nameof( replaceStr ) );
            }

            foreach ( var c in Path.GetInvalidFileNameChars() ) {
                filename = filename.Replace( c.ToString(), replaceStr );
            }

            return filename;
        }

        public String SubstituteWildcards( String filename, TwitchVideo video, TwitchVideoQuality quality = null, TimeSpan? cropStart = null, TimeSpan? cropEnd = null ) {
            if ( video == null ) {
                throw new ArgumentNullException( nameof( video ) );
            }

            if ( String.IsNullOrWhiteSpace( filename ) ) {
                return filename;
            }

            var result = filename;

            DateTime recorded = video.RecordedDate;

            TwitchVideoQuality selectedQuality = quality ?? video.Qualities.First();
            TimeSpan selectedCropStart = cropStart ?? TimeSpan.Zero;
            TimeSpan selectedCropEnd = cropEnd ?? video.Length;

            result = result.Replace( FilenameWildcards.CHANNEL, video.Channel );
            result = result.Replace( FilenameWildcards.GAME, video.Game );
            result = result.Replace( FilenameWildcards.DATE, recorded.ToString( "yyyyMMdd" ) );
            result = result.Replace( FilenameWildcards.TIME, recorded.ToString( "hhmmsstt", CultureInfo.InvariantCulture ) );
            result = result.Replace( FilenameWildcards.TIME24, recorded.ToString( "HHmmss", CultureInfo.InvariantCulture ) );
            result = result.Replace( FilenameWildcards.ID, video.Id );
            result = result.Replace( FilenameWildcards.TITLE, video.Title );
            result = result.Replace( FilenameWildcards.RES, !String.IsNullOrWhiteSpace( selectedQuality.Resolution ) ? selectedQuality.Resolution : TwitchVideoQuality.UNKNOWN );
            result = result.Replace( FilenameWildcards.FPS, selectedQuality.Fps.HasValue ? selectedQuality.Fps.ToString() : TwitchVideoQuality.UNKNOWN );
            result = result.Replace( FilenameWildcards.START, selectedCropStart.ToShortDaylessString() );
            result = result.Replace( FilenameWildcards.END, selectedCropEnd.ToShortDaylessString() );

            result = this.SubstituteInvalidChars( result, "_" );

            return result;
        }
    }
}