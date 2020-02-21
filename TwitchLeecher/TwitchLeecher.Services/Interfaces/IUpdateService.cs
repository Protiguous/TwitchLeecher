namespace TwitchLeecher.Services.Interfaces {

    using TwitchLeecher.Core.Models;

    public interface IUpdateService {

        UpdateInfo CheckForUpdate();
    }
}