using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Browser;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views {

    public partial class TwitchConnectView : UserControl {

        public TwitchConnectView() {
            this.InitializeComponent();

            this.Loaded += this.TwitchConnectView_Loaded;
            this.Unloaded += this.TwitchConnectView_Unloaded;
        }

        private void Chrome_IsBrowserInitializedChanged( object sender, DependencyPropertyChangedEventArgs e ) {
            if ( ( ( bool )e.NewValue ) == true ) {
                this.chrome.Load( "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=37v97169hnj8kaoq8fs3hzz8v6jezdj&redirect_uri=http://www.tl.com&scope=user_subscriptions&force_verify=true" );
            }
        }

        private void TwitchConnectView_Loaded( object sender, RoutedEventArgs e ) {
            this.chrome.IsBrowserInitializedChanged += this.Chrome_IsBrowserInitializedChanged;
            this.chrome.RequestHandler = new AuthRequestHandler( this.Dispatcher, this.DataContext as TwitchConnectViewVM );
        }

        private void TwitchConnectView_Unloaded( object sender, RoutedEventArgs e ) {
            this.chrome.Dispose();
        }
    }
}