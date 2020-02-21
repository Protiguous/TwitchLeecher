namespace TwitchLeecher.Services.Services {

    using System;
    using System.IO;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.IO;

    internal class LogService : ILogService {

        private const String LOGS_FOLDER_NAME = "logs";

        private readonly String _logDir;

        public LogService( IFolderService folderService ) {
            if ( folderService == null ) {
                throw new ArgumentNullException( nameof( folderService ) );
            }

            this._logDir = Path.Combine( folderService.GetAppDataFolder(), LOGS_FOLDER_NAME );
        }

        public String LogException( Exception ex ) {
            try {
                FileSystem.CreateDirectory( this._logDir );

                var logFile = Path.Combine( this._logDir, DateTime.UtcNow.ToString( "MMddyyyy_hhmmss_fff_tt" ) + "_error.log" );

                File.WriteAllText( logFile, ex.ToString() );

                return logFile;
            }
            catch {

                // Do not crash application if logging fails
            }

            return null;
        }
    }
}