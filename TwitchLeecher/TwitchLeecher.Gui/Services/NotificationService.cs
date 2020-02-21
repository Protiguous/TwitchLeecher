using Ninject;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;

namespace TwitchLeecher.Gui.Services {
    internal class NotificationService : INotificationService {

        private IKernel _kernel;

        public NotificationService( IKernel kernel ) {
            this._kernel = kernel;
        }

        public void ShowNotification( string text ) {
            this._kernel.Get<MainWindow>().ShowNotification( text );
        }
    }
}