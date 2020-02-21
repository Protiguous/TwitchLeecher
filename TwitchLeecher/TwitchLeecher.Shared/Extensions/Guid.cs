namespace TwitchLeecher.Shared.Extensions {

    using System;

    public static class GuidExtensions {

        public static Boolean IsEmpty( this Guid value ) {
            return value == Guid.Empty;
        }
    }
}