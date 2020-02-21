using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels {
    public class DownloadViewVM : ViewModelBase {

        private DownloadParameters _downloadParams;
        private bool _useCustomFilename;

        private ICommand _chooseCommand;
        private ICommand _downloadCommand;
        private ICommand _cancelCommand;

        private readonly IDialogService _dialogService;
        private readonly IFilenameService _filenameService;
        private readonly IPreferencesService _preferencesService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        private readonly object _commandLockObject;

        public DownloadViewVM(
            IDialogService dialogService,
            IFilenameService filenameService,
            IPreferencesService preferencesService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService ) {
            this._dialogService = dialogService;
            this._filenameService = filenameService;
            this._preferencesService = preferencesService;
            this._twitchService = twitchService;
            this._navigationService = navigationService;
            this._notificationService = notificationService;

            this._commandLockObject = new object();
        }

        public DownloadParameters DownloadParams {
            get {
                return this._downloadParams;
            }
            set {
                if ( this._downloadParams != null ) {
                    this._downloadParams.PropertyChanged -= this._downloadParams_PropertyChanged;
                }

                this.SetProperty( ref this._downloadParams, value, nameof( this.DownloadParams ) );

                this._downloadParams.PropertyChanged += this._downloadParams_PropertyChanged;
            }
        }

        public bool UseCustomFilename {
            get {
                return this._useCustomFilename;
            }
            set {
                this.SetProperty( ref this._useCustomFilename, value, nameof( this.UseCustomFilename ) );

                if ( !value ) {
                    this.UpdateFilenameFromTemplate();
                }
            }
        }

        public int CropStartHours {
            get {
                return this._downloadParams.CropStartTime.GetDaysInHours();
            }
            set {
                TimeSpan current = this._downloadParams.CropStartTime;
                this._downloadParams.CropStartTime = new TimeSpan( value, current.Minutes, current.Seconds );

                this.FirePropertyChanged( nameof( this.CropStartHours ) );
                this.FirePropertyChanged( nameof( this.CropStartMinutes ) );
                this.FirePropertyChanged( nameof( this.CropStartSeconds ) );
            }
        }

        public int CropStartMinutes {
            get {
                return this._downloadParams.CropStartTime.Minutes;
            }
            set {
                TimeSpan current = this._downloadParams.CropStartTime;
                this._downloadParams.CropStartTime = new TimeSpan( current.GetDaysInHours(), value, current.Seconds );

                this.FirePropertyChanged( nameof( this.CropStartHours ) );
                this.FirePropertyChanged( nameof( this.CropStartMinutes ) );
                this.FirePropertyChanged( nameof( this.CropStartSeconds ) );
            }
        }

        public int CropStartSeconds {
            get {
                return this._downloadParams.CropStartTime.Seconds;
            }
            set {
                TimeSpan current = this._downloadParams.CropStartTime;
                this._downloadParams.CropStartTime = new TimeSpan( current.GetDaysInHours(), current.Minutes, value );

                this.FirePropertyChanged( nameof( this.CropStartHours ) );
                this.FirePropertyChanged( nameof( this.CropStartMinutes ) );
                this.FirePropertyChanged( nameof( this.CropStartSeconds ) );
            }
        }

        public int CropEndHours {
            get {
                return this._downloadParams.CropEndTime.GetDaysInHours();
            }
            set {
                TimeSpan current = this._downloadParams.CropEndTime;
                this._downloadParams.CropEndTime = new TimeSpan( value, current.Minutes, current.Seconds );

                this.FirePropertyChanged( nameof( this.CropEndHours ) );
                this.FirePropertyChanged( nameof( this.CropEndMinutes ) );
                this.FirePropertyChanged( nameof( this.CropEndSeconds ) );
            }
        }

        public int CropEndMinutes {
            get {
                return this._downloadParams.CropEndTime.Minutes;
            }
            set {
                TimeSpan current = this._downloadParams.CropEndTime;
                this._downloadParams.CropEndTime = new TimeSpan( current.GetDaysInHours(), value, current.Seconds );

                this.FirePropertyChanged( nameof( this.CropEndHours ) );
                this.FirePropertyChanged( nameof( this.CropEndMinutes ) );
                this.FirePropertyChanged( nameof( this.CropEndSeconds ) );
            }
        }

        public int CropEndSeconds {
            get {
                return this._downloadParams.CropEndTime.Seconds;
            }
            set {
                TimeSpan current = this._downloadParams.CropEndTime;
                this._downloadParams.CropEndTime = new TimeSpan( current.GetDaysInHours(), current.Minutes, value );

                this.FirePropertyChanged( nameof( this.CropEndHours ) );
                this.FirePropertyChanged( nameof( this.CropEndMinutes ) );
                this.FirePropertyChanged( nameof( this.CropEndSeconds ) );
            }
        }

        public ICommand ChooseCommand {
            get {
                if ( this._chooseCommand == null ) {
                    this._chooseCommand = new DelegateCommand( this.Choose );
                }

                return this._chooseCommand;
            }
        }

        public ICommand DownloadCommand {
            get {
                if ( this._downloadCommand == null ) {
                    this._downloadCommand = new DelegateCommand( this.Download );
                }

                return this._downloadCommand;
            }
        }

        public ICommand CancelCommand {
            get {
                if ( this._cancelCommand == null ) {
                    this._cancelCommand = new DelegateCommand( this.Cancel );
                }

                return this._cancelCommand;
            }
        }

        private void Choose() {
            try {
                lock ( this._commandLockObject ) {
                    this._dialogService.ShowFolderBrowserDialog( this._downloadParams.Folder, this.ChooseCallback );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseCallback( bool cancelled, string folder ) {
            try {
                if ( !cancelled ) {
                    this._downloadParams.Folder = folder;
                    this._downloadParams.Validate( nameof( DownloadParameters.Folder ) );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void UpdateFilenameFromTemplate() {
            Preferences currentPrefs = this._preferencesService.CurrentPreferences.Clone();

            TimeSpan? cropStartTime = this._downloadParams.CropStart ? this._downloadParams.CropStartTime : TimeSpan.Zero;
            TimeSpan? cropEndTime = this._downloadParams.CropEnd ? this._downloadParams.CropEndTime : this._downloadParams.Video.Length;

            string fileName = this._filenameService.SubstituteWildcards( currentPrefs.DownloadFileName, this._downloadParams.Video, this._downloadParams.Quality, cropStartTime, cropEndTime );
            fileName = this._filenameService.EnsureExtension( fileName, currentPrefs.DownloadDisableConversion );

            this._downloadParams.Filename = fileName;
        }

        private void Download() {
            try {
                lock ( this._commandLockObject ) {
                    this.Validate();

                    if ( !this.HasErrors ) {
                        if ( File.Exists( this._downloadParams.FullPath ) ) {
                            MessageBoxResult result = this._dialogService.ShowMessageBox( "The file already exists. Do you want to overwrite it?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question );

                            if ( result != MessageBoxResult.Yes ) {
                                return;
                            }
                        }

                        this._twitchService.Enqueue( this._downloadParams );
                        this._navigationService.NavigateBack();
                        this._notificationService.ShowNotification( "Download added" );
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Cancel() {
            try {
                lock ( this._commandLockObject ) {
                    this._navigationService.NavigateBack();
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        public override void Validate( string propertyName = null ) {
            base.Validate( propertyName );

            string currentProperty = nameof( this.DownloadParams );

            if ( string.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                this.DownloadParams?.Validate();

                if ( this._twitchService.IsFileNameUsed( this._downloadParams.FullPath ) ) {
                    this.DownloadParams.AddError( nameof( this.DownloadParams.Filename ), "Another video is already being downloaded to this file!" );
                }

                if ( this.DownloadParams.HasErrors ) {
                    this.AddError( currentProperty, "Invalid Download Parameters!" );

                    if ( this.DownloadParams.GetErrors( nameof( DownloadParameters.CropStartTime ) ) is List<string> cropStartErrors && cropStartErrors.Count > 0 ) {
                        string firstError = cropStartErrors.First();
                        this.AddError( nameof( this.CropStartHours ), firstError );
                        this.AddError( nameof( this.CropStartMinutes ), firstError );
                        this.AddError( nameof( this.CropStartSeconds ), firstError );
                    }

                    if ( this.DownloadParams.GetErrors( nameof( DownloadParameters.CropEndTime ) ) is List<string> cropEndErrors && cropEndErrors.Count > 0 ) {
                        string firstError = cropEndErrors.First();
                        this.AddError( nameof( this.CropEndHours ), firstError );
                        this.AddError( nameof( this.CropEndMinutes ), firstError );
                        this.AddError( nameof( this.CropEndSeconds ), firstError );
                    }
                }
            }
        }

        protected override List<MenuCommand> BuildMenu() {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if ( menuCommands == null ) {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add( new MenuCommand( this.DownloadCommand, "Download", "Download" ) );
            menuCommands.Add( new MenuCommand( this.CancelCommand, "Cancel", "Times" ) );

            return menuCommands;
        }

        private void _downloadParams_PropertyChanged( object sender, PropertyChangedEventArgs e ) {
            if ( this._useCustomFilename ) {
                return;
            }

            if ( e.PropertyName == nameof( DownloadParameters.Quality )
                || e.PropertyName == nameof( DownloadParameters.CropStart )
                || e.PropertyName == nameof( DownloadParameters.CropEnd )
                || e.PropertyName == nameof( DownloadParameters.CropStartTime )
                || e.PropertyName == nameof( DownloadParameters.CropEndTime ) ) {
                this.UpdateFilenameFromTemplate();
            }
        }
    }
}