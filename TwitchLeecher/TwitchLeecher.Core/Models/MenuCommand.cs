namespace TwitchLeecher.Core.Models {

    using System;
    using System.Windows.Input;

    public class MenuCommand {

        public ICommand Command { get; private set; }

        public String Icon { get; private set; }

        public String Label { get; private set; }

        public Double Width { get; private set; }

        public MenuCommand( ICommand command, String label, String icon, Int32 width = 120 ) {
            if ( String.IsNullOrWhiteSpace( label ) ) {
                throw new ArgumentNullException( nameof( label ) );
            }

            if ( String.IsNullOrWhiteSpace( icon ) ) {
                throw new ArgumentNullException( nameof( icon ) );
            }

            this.Command = command ?? throw new ArgumentNullException( nameof( command ) );
            this.Label = label;
            this.Icon = icon;
            this.Width = width;
        }
    }
}