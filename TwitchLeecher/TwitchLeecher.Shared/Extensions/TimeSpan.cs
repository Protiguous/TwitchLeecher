namespace TwitchLeecher.Shared.Extensions {

    using System;

    public static class TimeSpanExtensions {

        public static Int32 GetDaysInHours( this TimeSpan value ) {
            return ( value.Days * 24 ) + value.Hours;
        }

        public static String ToDaylessString( this TimeSpan value ) {
            return String.Format( "{0}:{1}:{2}", value.GetDaysInHours().ToString( "00" ), value.Minutes.ToString( "00" ), value.Seconds.ToString( "00" ) );
        }

        public static String ToShortDaylessString( this TimeSpan value ) {
            return String.Format( "{0}{1}{2}", value.GetDaysInHours().ToString( "00" ), value.Minutes.ToString( "00" ), value.Seconds.ToString( "00" ) );
        }
    }
}