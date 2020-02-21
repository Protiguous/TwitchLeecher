namespace TwitchLeecher.Core.Models {

    using System;

    public class VodPlaylistPart {

        public Double Length { get; }

        public String LocalFile { get; }

        public String RemoteFile { get; }

        public VodPlaylistPart( Double length, String remoteFile, String localFile ) {
            if ( String.IsNullOrWhiteSpace( remoteFile ) ) {
                throw new ArgumentNullException( nameof( remoteFile ) );
            }

            if ( String.IsNullOrWhiteSpace( localFile ) ) {
                throw new ArgumentNullException( nameof( localFile ) );
            }

            this.Length = length;
            this.RemoteFile = remoteFile;
            this.LocalFile = localFile;
        }
    }
}