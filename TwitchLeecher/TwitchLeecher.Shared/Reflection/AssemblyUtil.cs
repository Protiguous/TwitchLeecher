namespace TwitchLeecher.Shared.Reflection {

    using System;
    using System.Reflection;

    public class AssemblyUtil {

        private static AssemblyUtil _instance;

        private String _product;
        private Version _version;

        public static AssemblyUtil Get {
            get {
                if ( _instance == null ) {
                    _instance = new AssemblyUtil();
                }

                return _instance;
            }
        }

        public Version GetAssemblyVersion() {
            if ( this._version == null ) {
                Assembly a = Assembly.GetExecutingAssembly();

                if ( a == null ) {
                    throw new ApplicationException( "Executing assembly is null!" );
                }

                AssemblyFileVersionAttribute att = a.GetCustomAttribute<AssemblyFileVersionAttribute>();

                if ( att == null ) {
                    throw new ApplicationException( "Could not find attribute of type '" + typeof( AssemblyFileVersionAttribute ).FullName + "'!" );
                }

                if ( !Version.TryParse( att.Version, out this._version ) ) {
                    throw new ApplicationException( "Error while parsing assembly file version!" );
                }
            }

            return this._version;
        }

        public String GetProductName() {
            if ( String.IsNullOrEmpty( this._product ) ) {
                Assembly a = Assembly.GetExecutingAssembly();

                if ( a == null ) {
                    throw new ApplicationException( "Executing assembly is null!" );
                }

                AssemblyProductAttribute att = a.GetCustomAttribute<AssemblyProductAttribute>();

                if ( att == null ) {
                    throw new ApplicationException( "Could not find attribute of type '" + typeof( AssemblyProductAttribute ).FullName + "'!" );
                }

                this._product = att.Product;
            }

            return this._product;
        }
    }
}