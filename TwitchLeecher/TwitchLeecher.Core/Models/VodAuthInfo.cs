namespace TwitchLeecher.Core.Models {

    using System;

    public class VodAuthInfo {

        public Boolean Privileged { get; set; }

        public String Signature { get; private set; }

        public Boolean SubOnly { get; set; }

        public String Token { get; private set; }

        public VodAuthInfo( String token, String signature, Boolean privileged, Boolean subOnly ) {
            if ( String.IsNullOrWhiteSpace( token ) ) {
                throw new ArgumentNullException( nameof( token ) );
            }

            if ( String.IsNullOrWhiteSpace( signature ) ) {
                throw new ArgumentNullException( nameof( signature ) );
            }

            this.Token = token;
            this.Signature = signature;
            this.Privileged = privileged;
            this.SubOnly = subOnly;
        }
    }
}