using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Views {
    public partial class DownloadsView : UserControl {

        private INavigationState _state;

        public DownloadsView() {
            this.InitializeComponent();

            this.scroller.ScrollChanged += this.Scroller_ScrollChanged;
            this.Loaded += this.SearchResultView_Loaded;
        }

        private void Scroller_ScrollChanged( object sender, ScrollChangedEventArgs e ) {
            if ( this._state != null ) {
                this._state.ScrollPosition = e.VerticalOffset;
            }
        }

        private void SearchResultView_Loaded( object sender, RoutedEventArgs e ) {
            this._state = this.DataContext as INavigationState;

            if ( this._state != null ) {
                this.scroller.ScrollToVerticalOffset( this._state.ScrollPosition );
            }
        }
    }
}