using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels {
    public class WelcomeViewVM : ViewModelBase {

        public WelcomeViewVM() {
            AssemblyUtil au = AssemblyUtil.Get;

            this.ProductName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }

        public string ProductName { get; }
    }
}