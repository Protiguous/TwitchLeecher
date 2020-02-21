namespace TwitchLeecher.Services.Interfaces {

    public interface IFolderService {

        System.String GetAppDataFolder();

        System.String GetDownloadFolder();

        System.String GetTempFolder();
    }
}