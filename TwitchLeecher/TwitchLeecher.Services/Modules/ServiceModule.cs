namespace TwitchLeecher.Services.Modules {

    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Services.Services;

    public class ServiceModule : NinjectModule {

        public override void Load() {
            Bind<IFilenameService>().To<FilenameService>().InSingletonScope();
            Bind<IFolderService>().To<FolderService>().InSingletonScope();
            Bind<ILogService>().To<LogService>().InSingletonScope();
            Bind<IPreferencesService>().To<PreferencesService>().InSingletonScope();
            Bind<IProcessingService>().To<ProcessingService>().InSingletonScope();
            Bind<IRuntimeDataService>().To<RuntimeDataService>().InSingletonScope();
            Bind<ITwitchService>().To<TwitchService>().InSingletonScope();
            Bind<IUpdateService>().To<UpdateService>().InSingletonScope();
        }
    }
}