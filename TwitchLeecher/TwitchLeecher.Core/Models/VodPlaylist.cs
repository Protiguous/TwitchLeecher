namespace TwitchLeecher.Core.Models {

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public class VodPlaylist : List<VodPlaylistPart> {

        public static VodPlaylist Parse( String tempDir, String playlistStr, String urlPrefix ) {
            VodPlaylist playlist = new VodPlaylist();

            List<String> lines = playlistStr.Split( new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            var partCounter = 0;

            for ( var i = 0; i < lines.Count; i++ ) {
                var line = lines[ i ];

                if ( line.StartsWith( "#EXTINF", StringComparison.OrdinalIgnoreCase ) ) {
                    var length = Math.Max( Double.Parse( line.Substring( line.LastIndexOf( ":" ) + 1 ).TrimEnd( ',' ), NumberStyles.Any, CultureInfo.InvariantCulture ), 0 );

                    playlist.Add( new VodPlaylistPart( length, urlPrefix + lines[ i + 1 ], Path.Combine( tempDir, partCounter.ToString( "D8" ) + ".ts" ) ) );
                    partCounter++;
                    i++;
                }
            }

            return playlist;
        }
    }
}