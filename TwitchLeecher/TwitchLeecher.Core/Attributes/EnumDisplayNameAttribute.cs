namespace TwitchLeecher.Core.Attributes {

    using System;

    public class EnumDisplayNameAttribute : Attribute {

        public String Name { get; private set; }

        public EnumDisplayNameAttribute( String name ) {
            if ( String.IsNullOrWhiteSpace( name ) ) {
                throw new ArgumentNullException( nameof( name ) );
            }

            this.Name = name;
        }
    }
}