namespace TwitchLeecher.Services.Services {

    using System;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using TwitchLeecher.Core.Models;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Extensions;
    using TwitchLeecher.Shared.Reflection;

    internal class UpdateService : IUpdateService {

        private const String latestReleaseUrl = "https://github.com/Franiac/TwitchLeecher/releases/tag/v{0}";
        private const String releasesApiUrl = "https://api.github.com/repos/Franiac/TwitchLeecher/releases";

        public UpdateInfo CheckForUpdate() {
            try {
                using ( WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 } ) {
                    webClient.Headers.Add( HttpRequestHeader.UserAgent, "TwitchLeecher" );

                    var result = webClient.DownloadString( releasesApiUrl );

                    JToken releasesJson = JToken.Parse( result );

                    foreach ( JToken releaseJson in releasesJson ) {
                        Boolean draft = releaseJson.Value<Boolean>( "draft" );
                        Boolean prerelease = releaseJson.Value<Boolean>( "prerelease" );

                        if ( !draft && !prerelease ) {
                            String tagStr = releaseJson.Value<String>( "tag_name" );
                            String releasedStr = releaseJson.Value<String>( "published_at" );
                            String infoStr = releaseJson.Value<String>( "body" );

                            Version releaseVersion = Version.Parse( tagStr.Substring( 1 ) ).Pad();
                            Version localVersion = AssemblyUtil.Get.GetAssemblyVersion().Pad();

                            DateTime released = DateTime.Parse( releasedStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal );

                            if ( releaseVersion > localVersion ) {
                                return new UpdateInfo( releaseVersion, released, String.Format( latestReleaseUrl, releaseVersion.ToString( 3 ) ), infoStr );
                            }
                            else {
                                return null;
                            }
                        }
                    }
                }
            }
            catch {

                // Update check should not distract the application
            }

            return null;
        }
    }
}