namespace TwitchLeecher.Services.Interfaces {

    using System;
    using TwitchLeecher.Core.Models;

    public interface IFilenameService {

        String EnsureExtension( String filename, Boolean disableConversion );

        String SubstituteInvalidChars( String filename, String replaceStr );

        String SubstituteWildcards( String filename, TwitchVideo video, TwitchVideoQuality quality = null, TimeSpan? cropStart = null, TimeSpan? cropEnd = null );
    }
}