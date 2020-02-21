namespace TwitchLeecher.Services.Services {

    using System;
    using System.IO;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Reflection;

    internal class FolderService : IFolderService {

        private String appDataFolder;
        private String downloadsFolder;
        private String downloadsTempFolder;

        public String GetAppDataFolder() {
            if ( String.IsNullOrWhiteSpace( this.appDataFolder ) ) {
                var productName = AssemblyUtil.Get.GetProductName();
                this.appDataFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), productName );
            }

            return this.appDataFolder;
        }

        public String GetDownloadFolder() {
            if ( String.IsNullOrWhiteSpace( this.downloadsFolder ) ) {
                this.downloadsFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Downloads" );
            }

            return this.downloadsFolder;
        }

        public String GetTempFolder() {
            if ( String.IsNullOrWhiteSpace( this.downloadsTempFolder ) ) {
                this.downloadsTempFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "Temp" );
            }

            return this.downloadsTempFolder;
        }
    }
}