namespace TwitchLeecher.Core.Models {

    using System;

    public class UpdateInfo {

        public String DownloadUrl { get; }

        public Version NewVersion { get; }

        public DateTime ReleaseDate { get; }

        public String ReleaseNotes { get; }

        public UpdateInfo( Version newVersion, DateTime releaseDate, String downloadUrl, String releaseNotes ) {
            if ( String.IsNullOrWhiteSpace( downloadUrl ) ) {
                throw new ArgumentNullException( nameof( downloadUrl ) );
            }

            if ( String.IsNullOrWhiteSpace( releaseNotes ) ) {
                throw new ArgumentNullException( nameof( releaseNotes ) );
            }

            this.NewVersion = newVersion ?? throw new ArgumentNullException( nameof( newVersion ) );
            this.ReleaseDate = releaseDate;
            this.DownloadUrl = downloadUrl;
            this.ReleaseNotes = releaseNotes;
        }
    }
}