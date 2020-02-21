namespace TwitchLeecher.Core.Models {

    using System;

    public class TwitchAuthInfo {

        public String AccessToken { get; private set; }

        public String Username { get; private set; }

        public TwitchAuthInfo( String accessToken, String username ) {
            if ( String.IsNullOrWhiteSpace( accessToken ) ) {
                throw new ArgumentNullException( nameof( accessToken ) );
            }

            if ( String.IsNullOrWhiteSpace( username ) ) {
                throw new ArgumentNullException( nameof( username ) );
            }

            this.AccessToken = accessToken;
            this.Username = username;
        }
    }
}