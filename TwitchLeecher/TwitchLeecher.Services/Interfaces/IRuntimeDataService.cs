namespace TwitchLeecher.Services.Interfaces {

    using TwitchLeecher.Core.Models;

    public interface IRuntimeDataService {

        RuntimeData RuntimeData { get; }

        void Save();
    }
}