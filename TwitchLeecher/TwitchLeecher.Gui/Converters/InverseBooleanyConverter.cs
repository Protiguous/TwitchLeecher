using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Gui.Converters {
    public class InverseBooleanConverter : IValueConverter {

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            if ( !( value is bool ) ) {
                throw new ApplicationException( "Value has to be of type '" + typeof( bool ).FullName + "'!" );
            }

            return !( bool )value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            if ( !( value is bool ) ) {
                throw new ApplicationException( "Value has to be of type '" + typeof( bool ).FullName + "'!" );
            }

            return !( bool )value;
        }
    }
}