namespace TwitchLeecher.Shared.Extensions {

    using System;
    using System.Globalization;
    using System.Xml.Linq;

    public static class XmlExtensions {

        public static readonly CultureInfo XML_CULTURE = CultureInfo.InvariantCulture;

        private static T GetAttributeValue<T>( this XElement xel, String attributeName ) where T : struct, IConvertible {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            xat.CheckValueNotNullOrWhitespace();

            try {
                return ( T )Convert.ChangeType( xat.Value, typeof( T ), XML_CULTURE );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert attribute value '" + xat.Value + "' of attribute '" + attributeName + "' into type '" + typeof( T ).FullName + "'!", ex );
            }
        }

        private static T? GetNullableValue<T>( this XElement xel ) where T : struct, IConvertible {
            xel.CheckNotNull();

            if ( xel.IsEmpty || String.IsNullOrWhiteSpace( xel.Value ) ) {
                return null;
            }

            try {
                return ( T )Convert.ChangeType( xel.Value, typeof( T ), XML_CULTURE );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( T ).FullName + "'!", ex );
            }
        }

        public static void CheckName( this XElement xel, String expectedName ) {
            CheckNotNull( xel );

            if ( String.IsNullOrWhiteSpace( expectedName ) && xel.Name.LocalName != expectedName ) {
                throw new ApplicationException( "Unexpected element name '" + xel.Name.LocalName + "'! Expected name is '" + expectedName + "'." );
            }
        }

        public static void CheckNotNull( this XElement xel ) {
            if ( xel == null ) {
                throw new ApplicationException( "Element is null!" );
            }
        }

        public static void CheckNotNull( this XAttribute xat ) {
            if ( xat == null ) {
                throw new ApplicationException( "Attribute is null!" );
            }
        }

        public static void CheckValueNotNull( this XElement xel ) {
            xel.CheckNotNull();

            if ( xel.IsEmpty || xel.Value == null ) {
                throw new ApplicationException( "Incomplete element '" + xel.Name.LocalName + "': Value is null!" );
            }
        }

        public static void CheckValueNotNullOrWhitespace( this XAttribute xat ) {
            xat.CheckNotNull();

            if ( String.IsNullOrWhiteSpace( xat.Value ) ) {
                throw new ApplicationException( "Incomplete attribute '" + xat.Name.LocalName + "': Value not specified!" );
            }
        }

        public static void CheckValueNotNullOrWhitespace( this XElement xel ) {
            xel.CheckNotNull();

            if ( xel.IsEmpty || String.IsNullOrWhiteSpace( xel.Value ) ) {
                throw new ApplicationException( "Incomplete element '" + xel.Name.LocalName + "': Value not specified!" );
            }
        }

        public static Boolean GetAttributeValueAsBool( this XElement xel, String attributeName ) {
            return xel.GetAttributeValue<Boolean>( attributeName );
        }

        public static DateTime GetAttributeValueAsDateTime( this XElement xel, String attributeName ) {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            xat.CheckValueNotNullOrWhitespace();

            try {
                return DateTime.Parse( xat.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into type '" + typeof( DateTime ).FullName + "'!", ex );
            }
        }

        public static T GetAttributeValueAsEnum<T>( this XElement xel, String attributeName ) {
            if ( !typeof( T ).IsEnum ) {
                throw new ArgumentException( "T must be an enumerated type!" );
            }

            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            xat.CheckValueNotNullOrWhitespace();

            try {
                return ( T )Enum.Parse( typeof( T ), xat.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into enum '" + typeof( T ).FullName + "'!", ex );
            }
        }

        public static Int64 GetAttributeValueAsLong( this XElement xel, String attributeName ) {
            return xel.GetAttributeValue<Int64>( attributeName );
        }

        public static String GetAttributeValueAsString( this XElement xel, String attributeName ) {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            xat.CheckValueNotNullOrWhitespace();

            // XML stores new lines as \n
            return xat.Value.Replace( Environment.NewLine, "\n" )
                .Replace( "\r", "\n" )
                .Replace( "\n", Environment.NewLine );
        }

        public static Version GetAttributeValueAsVersion( this XElement xel, String attributeName ) {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            xat.CheckValueNotNullOrWhitespace();

            try {
                return Version.Parse( xat.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into type '" + typeof( Version ).FullName + "'!", ex );
            }
        }

        public static Uri GetValueAsAbsoluteUri( this XElement xel, Boolean nullAllowed = false ) {
            xel.CheckNotNull();

            if ( !nullAllowed ) {
                xel.CheckValueNotNull();
            }

            var valueStr = xel.Value;

            if ( String.IsNullOrWhiteSpace( valueStr ) ) {
                return null;
            }

            if ( Uri.TryCreate( valueStr, UriKind.Absolute, out Uri u ) ) {
                return u;
            }
            else {
                throw new ApplicationException( "Could not convert string '" + valueStr + "' into absolute uri!" );
            }
        }

        public static Boolean GetValueAsBool( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return Boolean.Parse( xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( Boolean ).FullName + "'!", ex );
            }
        }

        public static DateTime GetValueAsDateTime( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return DateTime.Parse( xel.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( DateTime ).FullName + "'!", ex );
            }
        }

        public static Double GetValueAsDouble( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return Double.Parse( xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( Double ).FullName + "'!", ex );
            }
        }

        public static T GetValueAsEnum<T>( this XElement xel ) {
            if ( !typeof( T ).IsEnum ) {
                throw new ArgumentException( "T must be an enumerated type!" );
            }

            xel.CheckValueNotNullOrWhitespace();

            try {
                return ( T )Enum.Parse( typeof( T ), xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into enum '" + typeof( T ).FullName + "'!", ex );
            }
        }

        public static Guid GetValueAsGuid( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            return new Guid( xel.Value );
        }

        public static Int32 GetValueAsInt( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return Int32.Parse( xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( Int32 ).FullName + "'!", ex );
            }
        }

        public static Int64 GetValueAsLong( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return Int64.Parse( xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( Int64 ).FullName + "'!", ex );
            }
        }

        public static Boolean? GetValueAsNullableBool( this XElement xel ) {
            return xel.GetNullableValue<Boolean>();
        }

        public static Decimal? GetValueAsNullableDecimal( this XElement xel ) {
            return xel.GetNullableValue<Decimal>();
        }

        public static Double? GetValueAsNullableDouble( this XElement xel ) {
            return xel.GetNullableValue<Double>();
        }

        public static Int32? GetValueAsNullableInt( this XElement xel ) {
            return xel.GetNullableValue<Int32>();
        }

        public static String GetValueAsString( this XElement xel, Boolean nullAllowed = false ) {
            xel.CheckNotNull();

            if ( !nullAllowed ) {
                xel.CheckValueNotNull();
            }

            // XML stores new lines as \n
            return xel.Value.Replace( Environment.NewLine, "\n" )
                .Replace( "\r", "\n" )
                .Replace( "\n", Environment.NewLine );
        }

        public static DateTime GetValueAsTime( this XElement xel ) {
            xel.CheckValueNotNullOrWhitespace();

            try {
                return DateTime.UtcNow.Date + TimeSpan.ParseExact( xel.Value, "hh\\:mm", CultureInfo.InvariantCulture );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( TimeSpan ).FullName + "'!", ex );
            }
        }

        public static Version GetValueAsVersion( this XElement xel ) {
            xel.CheckNotNull();

            try {
                return Version.Parse( xel.Value );
            }
            catch ( Exception ex ) {
                throw new ApplicationException( "Could not convert element value '" + xel.Value + "' into type '" + typeof( Version ).FullName + "'!", ex );
            }
        }

        public static Boolean HasAttributeWithValue( this XElement xel, String attributeName ) {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute( attributeName );

            return xat != null && !String.IsNullOrWhiteSpace( xat.Value );
        }

        public static String OuterXml( this XDocument doc ) {
            if ( doc == null )
                return null;

            return doc.Declaration + Environment.NewLine + doc.ToString();
        }

        public static XDocument ToDocument( this XElement xel ) {
            xel.CheckNotNull();

            return new XDocument( new XDeclaration( "1.0", "UTF-8", null ), xel );
            ;
        }
    }
}