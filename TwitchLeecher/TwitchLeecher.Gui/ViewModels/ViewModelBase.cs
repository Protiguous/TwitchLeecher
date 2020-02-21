using System.Collections.Generic;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.ViewModels {
    public abstract class ViewModelBase : BindableBase {

        private List<MenuCommand> _menuCommands;

        public bool HasMenu {
            get {
                List<MenuCommand> menuCommands = this.MenuCommands;

                return menuCommands != null && menuCommands.Count > 0;
            }
        }

        public List<MenuCommand> MenuCommands {
            get {
                if ( this._menuCommands == null ) {
                    List<MenuCommand> menuCommands = this.BuildMenu();

                    if ( menuCommands == null ) {
                        menuCommands = new List<MenuCommand>();
                    }

                    this._menuCommands = menuCommands;
                }

                return this._menuCommands;
            }
        }

        public virtual void OnBeforeShown() {
        }

        public virtual void OnBeforeHidden() {
        }

        protected virtual List<MenuCommand> BuildMenu() {
            return new List<MenuCommand>();
        }
    }
}