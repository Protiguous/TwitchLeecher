namespace TwitchLeecher.Services.Interfaces {

    using TwitchLeecher.Core.Models;

    public interface IPreferencesService {

        Preferences CurrentPreferences { get; }

        Preferences CreateDefault();

        System.Boolean IsChannelInFavourites( System.String channel );

        void Save( Preferences preferences );
    }
}