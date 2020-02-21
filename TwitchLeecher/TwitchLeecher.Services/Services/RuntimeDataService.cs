namespace TwitchLeecher.Services.Services {

    using System;
    using System.IO;
    using System.Xml.Linq;
    using TwitchLeecher.Core.Models;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Extensions;
    using TwitchLeecher.Shared.IO;
    using TwitchLeecher.Shared.Reflection;

    internal class RuntimeDataService : IRuntimeDataService {

        private const String APP_EL = "Application";
        private const String AUTH_ACCESSTOKEN_EL = "AccessToken";
        private const String AUTH_EL = "Authorization";
        private const String RUNTIMEDATA_EL = "RuntimeData";
        private const String RUNTIMEDATA_FILE = "runtime.xml";
        private const String RUNTIMEDATA_VERSION_ATTR = "Version";

        private readonly Object _commandLockObject;
        private IFolderService _folderService;

        private RuntimeData _runtimeData;
        private Version _tlVersion;

        public RuntimeData RuntimeData {
            get {
                if ( this._runtimeData == null ) {
                    this._runtimeData = this.Load();
                }

                return this._runtimeData;
            }
        }

        public RuntimeDataService( IFolderService folderService ) {
            this._folderService = folderService;
            this._tlVersion = AssemblyUtil.Get.GetAssemblyVersion().Trim();
            this._commandLockObject = new Object();
        }

        private RuntimeData Load() {
            lock ( this._commandLockObject ) {
                var configFile = Path.Combine( this._folderService.GetAppDataFolder(), RUNTIMEDATA_FILE );

                RuntimeData runtimeData = new RuntimeData() {
                    Version = _tlVersion
                };

                if ( File.Exists( configFile ) ) {
                    XDocument doc = XDocument.Load( configFile );

                    XElement runtimeDataEl = doc.Root;

                    if ( runtimeDataEl != null ) {
                        XAttribute rtVersionAttr = runtimeDataEl.Attribute( RUNTIMEDATA_VERSION_ATTR );

                        if ( rtVersionAttr != null && Version.TryParse( rtVersionAttr.Value, out Version rtVersion ) ) {
                            runtimeData.Version = rtVersion;
                        }
                        else {
                            runtimeData.Version = new Version( 1, 0 );
                        }

                        XElement authEl = runtimeDataEl.Element( AUTH_EL );

                        if ( authEl != null ) {
                            XElement accessTokenEl = authEl.Element( AUTH_ACCESSTOKEN_EL );

                            if ( accessTokenEl != null ) {
                                try {
                                    runtimeData.AccessToken = accessTokenEl.GetValueAsString();
                                }
                                catch {

                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }

                        XElement applicationEl = runtimeDataEl.Element( APP_EL );

                        if ( applicationEl != null ) {
                            XElement mainWindowInfoEl = applicationEl.Element( MainWindowInfo.MAINWINDOW_EL );

                            if ( mainWindowInfoEl != null ) {
                                try {
                                    runtimeData.MainWindowInfo = MainWindowInfo.GetFromXml( mainWindowInfoEl );
                                }
                                catch {

                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }
                    }
                }

                return runtimeData;
            }
        }

        public void Save() {
            lock ( this._commandLockObject ) {
                RuntimeData runtimeData = this.RuntimeData;

                XDocument doc = new XDocument( new XDeclaration( "1.0", "UTF-8", null ) );

                XElement runtimeDataEl = new XElement( RUNTIMEDATA_EL );
                runtimeDataEl.Add( new XAttribute( RUNTIMEDATA_VERSION_ATTR, this._tlVersion ) );
                doc.Add( runtimeDataEl );

                if ( !String.IsNullOrWhiteSpace( runtimeData.AccessToken ) ) {
                    XElement authEl = new XElement( AUTH_EL );
                    runtimeDataEl.Add( authEl );

                    XElement accessTokenEl = new XElement( AUTH_ACCESSTOKEN_EL );
                    accessTokenEl.SetValue( runtimeData.AccessToken );
                    authEl.Add( accessTokenEl );
                }

                if ( runtimeData.MainWindowInfo != null ) {
                    XElement mainWindowInfoEl = runtimeData.MainWindowInfo.GetXml();

                    if ( mainWindowInfoEl.HasElements ) {
                        XElement applicationEl = new XElement( APP_EL );
                        applicationEl.Add( mainWindowInfoEl );
                        runtimeDataEl.Add( applicationEl );
                    }
                }

                var appDataFolder = this._folderService.GetAppDataFolder();

                FileSystem.CreateDirectory( appDataFolder );

                var configFile = Path.Combine( appDataFolder, RUNTIMEDATA_FILE );

                doc.Save( configFile );
            }
        }
    }
}