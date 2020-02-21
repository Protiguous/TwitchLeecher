namespace TwitchLeecher.Shared.Extensions {

    using System.Collections.Specialized;
    using System.Linq;

    public static class NameValueCollectionExtensions {

        public static System.Boolean ContainsKey( this NameValueCollection collection, System.String key ) {
            if ( collection.Get( key ) == null ) {
                return collection.AllKeys.Contains( key );
            }

            return true;
        }
    }
}