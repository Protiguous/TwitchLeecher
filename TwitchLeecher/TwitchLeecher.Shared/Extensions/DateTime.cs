﻿namespace TwitchLeecher.Shared.Extensions {

    using System;

    public static class DateTimeExtensions {

        public static DateTime? ToLocalTime( this DateTime? value ) {
            if ( value.HasValue ) {
                return value.Value.ToLocalTime();
            }

            return null;
        }

        public static DateTime? ToUniversalTime( this DateTime? value ) {
            if ( value.HasValue ) {
                return value.Value.ToUniversalTime();
            }

            return null;
        }
    }
}