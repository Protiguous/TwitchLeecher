namespace TwitchLeecher.Services.Interfaces {

    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using TwitchLeecher.Core.Models;

    public interface ITwitchService : INotifyPropertyChanged {

        ObservableCollection<TwitchVideoDownload> Downloads { get; }

        System.Boolean IsAuthorized { get; }

        ObservableCollection<TwitchVideo> Videos { get; }

        System.Boolean Authorize( System.String accessToken );

        void Cancel( System.String id );

        System.Boolean CanShutdown();

        System.Boolean ChannelExists( System.String channel );

        void Enqueue( DownloadParameters downloadParams );

        System.String GetChannelIdByName( System.String channel );

        System.Boolean IsFileNameUsed( System.String fullPath );

        void Pause();

        void Remove( System.String id );

        void Resume();

        VodAuthInfo RetrieveVodAuthInfo( System.String id );

        void Retry( System.String id );

        void RevokeAuthorization();

        void Search( SearchParameters searchParams );

        void Shutdown();
    }
}