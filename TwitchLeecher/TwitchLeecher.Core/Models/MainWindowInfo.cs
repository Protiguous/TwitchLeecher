namespace TwitchLeecher.Core.Models {

    using System;
    using System.Xml.Linq;
    using TwitchLeecher.Shared.Extensions;

    public class MainWindowInfo {

        private const String MAINWINDOW_HEIGHT_EL = "Height";
        private const String MAINWINDOW_ISMAXIMIZED_EL = "IsMaximized";
        private const String MAINWINDOW_LEFT_EL = "Left";
        private const String MAINWINDOW_TOP_EL = "Top";
        private const String MAINWINDOW_WIDTH_EL = "Width";
        public const String MAINWINDOW_EL = "MainWindow";

        public Double Height { get; set; }

        public Boolean IsMaximized { get; set; }

        public Double Left { get; set; }

        public Double Top { get; set; }

        public Double Width { get; set; }

        public static MainWindowInfo GetFromXml( XElement mainWindowInfoEl ) {
            MainWindowInfo mainWindowInfo = new MainWindowInfo();

            if ( mainWindowInfoEl != null ) {
                XElement widthEl = mainWindowInfoEl.Element( MAINWINDOW_WIDTH_EL );

                if ( widthEl != null ) {
                    try {
                        mainWindowInfo.Width = ( Int32 )Math.Round( widthEl.GetValueAsDouble() );
                    }
                    catch {

                        // Malformed XML
                        return null;
                    }
                }
                else {

                    // Malformed XML
                    return null;
                }

                XElement heightEl = mainWindowInfoEl.Element( MAINWINDOW_HEIGHT_EL );

                if ( heightEl != null ) {
                    try {
                        mainWindowInfo.Height = ( Int32 )Math.Round( heightEl.GetValueAsDouble() );
                    }
                    catch {

                        // Malformed XML
                        return null;
                    }
                }
                else {

                    // Malformed XML
                    return null;
                }

                XElement topEl = mainWindowInfoEl.Element( MAINWINDOW_TOP_EL );

                if ( topEl != null ) {
                    try {
                        mainWindowInfo.Top = ( Int32 )Math.Round( topEl.GetValueAsDouble() );
                    }
                    catch {

                        // Malformed XML
                        return null;
                    }
                }
                else {

                    // Malformed XML
                    return null;
                }

                XElement leftEl = mainWindowInfoEl.Element( MAINWINDOW_LEFT_EL );

                if ( leftEl != null ) {
                    try {
                        mainWindowInfo.Left = ( Int32 )Math.Round( leftEl.GetValueAsDouble() );
                    }
                    catch {

                        // Malformed XML
                        return null;
                    }
                }
                else {

                    // Malformed XML
                    return null;
                }

                XElement isMaximizedEl = mainWindowInfoEl.Element( MAINWINDOW_ISMAXIMIZED_EL );

                if ( isMaximizedEl != null ) {
                    try {
                        mainWindowInfo.IsMaximized = isMaximizedEl.GetValueAsBool();
                    }
                    catch {

                        // Malformed XML
                        return null;
                    }
                }
                else {

                    // Malformed XML
                    return null;
                }
            }

            return mainWindowInfo;
        }

        public XElement GetXml() {
            XElement mainWindowInfoEl = new XElement( MAINWINDOW_EL );

            XElement widthEl = new XElement( MAINWINDOW_WIDTH_EL );
            widthEl.SetValue( Math.Round( this.Width ) );
            mainWindowInfoEl.Add( widthEl );

            XElement heightEl = new XElement( MAINWINDOW_HEIGHT_EL );
            heightEl.SetValue( Math.Round( this.Height ) );
            mainWindowInfoEl.Add( heightEl );

            XElement topEl = new XElement( MAINWINDOW_TOP_EL );
            topEl.SetValue( Math.Round( this.Top ) );
            mainWindowInfoEl.Add( topEl );

            XElement leftEl = new XElement( MAINWINDOW_LEFT_EL );
            leftEl.SetValue( Math.Round( this.Left ) );
            mainWindowInfoEl.Add( leftEl );

            XElement isMaximizedEl = new XElement( MAINWINDOW_ISMAXIMIZED_EL );
            isMaximizedEl.SetValue( this.IsMaximized );
            mainWindowInfoEl.Add( isMaximizedEl );

            return mainWindowInfoEl;
        }
    }
}