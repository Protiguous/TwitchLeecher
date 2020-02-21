using System;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels {
    public class InfoViewVM : ViewModelBase {

        private ICommand _openlinkCommand;
        private ICommand _donateCommand;

        private readonly IDialogService _dialogService;
        private readonly IDonationService _donationService;

        private readonly object _commandLockObject;

        public InfoViewVM( IDialogService dialogService, IDonationService donationService ) {
            AssemblyUtil au = AssemblyUtil.Get;

            this.ProductName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this._dialogService = dialogService;
            this._donationService = donationService;

            this._commandLockObject = new object();
        }

        public string ProductName { get; }

        public ICommand OpenLinkCommand {
            get {
                if ( this._openlinkCommand == null ) {
                    this._openlinkCommand = new DelegateCommand<string>( this.OpenLink );
                }

                return this._openlinkCommand;
            }
        }

        public ICommand DonateCommand {
            get {
                if ( this._donateCommand == null ) {
                    this._donateCommand = new DelegateCommand( this.Donate );
                }

                return this._donateCommand;
            }
        }

        private void OpenLink( string link ) {
            try {
                lock ( this._commandLockObject ) {
                    if ( string.IsNullOrWhiteSpace( link ) ) {
                        throw new ArgumentNullException( nameof( link ) );
                    }

                    Process.Start( link );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Donate() {
            try {
                lock ( this._commandLockObject ) {
                    this._donationService.OpenDonationPage();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }
    }
}