using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels {
    public class PreferencesViewVM : ViewModelBase {

        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private readonly IPreferencesService _preferencesService;

        private Preferences _currentPreferences;

        private ICommand _addFavouriteChannelCommand;
        private ICommand _removeFavouriteChannelCommand;
        private ICommand _chooseDownloadTempFolderCommand;
        private ICommand _chooseDownloadFolderCommand;
        private ICommand _chooseExternalPlayerCommand;
        private ICommand _clearExternalPlayerCommand;
        private ICommand _saveCommand;
        private ICommand _undoCommand;
        private ICommand _defaultsCommand;

        private readonly object _commandLockObject;

        public PreferencesViewVM(
            IDialogService dialogService,
            INotificationService notificationService,
            IPreferencesService preferencesService ) {
            this._dialogService = dialogService;
            this._notificationService = notificationService;
            this._preferencesService = preferencesService;

            this._commandLockObject = new object();
        }

        public Preferences CurrentPreferences {
            get {
                if ( this._currentPreferences == null ) {
                    this._currentPreferences = this._preferencesService.CurrentPreferences.Clone();
                }

                return this._currentPreferences;
            }

            private set {
                this.SetProperty( ref this._currentPreferences, value );
            }
        }

        public ICommand AddFavouriteChannelCommand {
            get {
                if ( this._addFavouriteChannelCommand == null ) {
                    this._addFavouriteChannelCommand = new DelegateCommand( this.AddFavouriteChannel );
                }

                return this._addFavouriteChannelCommand;
            }
        }

        public ICommand RemoveFavouriteChannelCommand {
            get {
                if ( this._removeFavouriteChannelCommand == null ) {
                    this._removeFavouriteChannelCommand = new DelegateCommand( this.RemoveFavouriteChannel );
                }

                return this._removeFavouriteChannelCommand;
            }
        }

        public ICommand ChooseDownloadTempFolderCommand {
            get {
                if ( this._chooseDownloadTempFolderCommand == null ) {
                    this._chooseDownloadTempFolderCommand = new DelegateCommand( this.ChooseDownloadTempFolder );
                }

                return this._chooseDownloadTempFolderCommand;
            }
        }

        public ICommand ChooseDownloadFolderCommand {
            get {
                if ( this._chooseDownloadFolderCommand == null ) {
                    this._chooseDownloadFolderCommand = new DelegateCommand( this.ChooseDownloadFolder );
                }

                return this._chooseDownloadFolderCommand;
            }
        }

        public ICommand ChooseExternalPlayerCommand {
            get {
                if ( this._chooseExternalPlayerCommand == null ) {
                    this._chooseExternalPlayerCommand = new DelegateCommand( this.ChooseExternalPlayer );
                }

                return this._chooseExternalPlayerCommand;
            }
        }

        public ICommand ClearExternalPlayerCommand {
            get {
                if ( this._clearExternalPlayerCommand == null ) {
                    this._clearExternalPlayerCommand = new DelegateCommand( this.ClearExternalPlayer );
                }

                return this._clearExternalPlayerCommand;
            }
        }

        public ICommand SaveCommand {
            get {
                if ( this._saveCommand == null ) {
                    this._saveCommand = new DelegateCommand( this.Save );
                }

                return this._saveCommand;
            }
        }

        public ICommand UndoCommand {
            get {
                if ( this._undoCommand == null ) {
                    this._undoCommand = new DelegateCommand( this.Undo );
                }

                return this._undoCommand;
            }
        }

        public ICommand DefaultsCommand {
            get {
                if ( this._defaultsCommand == null ) {
                    this._defaultsCommand = new DelegateCommand( this.Defaults );
                }

                return this._defaultsCommand;
            }
        }

        private void AddFavouriteChannel() {
            try {
                lock ( this._commandLockObject ) {
                    string currentChannel = this.CurrentPreferences.SearchChannelName;

                    if ( !string.IsNullOrWhiteSpace( currentChannel ) ) {
                        string existingEntry = this.CurrentPreferences.SearchFavouriteChannels.FirstOrDefault( channel => channel.Equals( currentChannel, StringComparison.OrdinalIgnoreCase ) );

                        if ( !string.IsNullOrWhiteSpace( existingEntry ) ) {
                            this.CurrentPreferences.SearchChannelName = existingEntry;
                        }
                        else {
                            this.CurrentPreferences.SearchFavouriteChannels.Add( currentChannel );
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void RemoveFavouriteChannel() {
            try {
                lock ( this._commandLockObject ) {
                    string currentChannel = this.CurrentPreferences.SearchChannelName;

                    if ( !string.IsNullOrWhiteSpace( currentChannel ) ) {
                        string existingEntry = this.CurrentPreferences.SearchFavouriteChannels.FirstOrDefault( channel => channel.Equals( currentChannel, StringComparison.OrdinalIgnoreCase ) );

                        if ( !string.IsNullOrWhiteSpace( existingEntry ) ) {
                            this.CurrentPreferences.SearchFavouriteChannels.Remove( existingEntry );
                            this.CurrentPreferences.SearchChannelName = this.CurrentPreferences.SearchFavouriteChannels.FirstOrDefault();
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseDownloadTempFolder() {
            try {
                lock ( this._commandLockObject ) {
                    this._dialogService.ShowFolderBrowserDialog( this.CurrentPreferences.DownloadTempFolder, this.ChooseDownloadTempFolderCallback );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseDownloadTempFolderCallback( bool cancelled, string folder ) {
            try {
                if ( !cancelled ) {
                    this.CurrentPreferences.DownloadTempFolder = folder;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseDownloadFolder() {
            try {
                lock ( this._commandLockObject ) {
                    this._dialogService.ShowFolderBrowserDialog( this.CurrentPreferences.DownloadFolder, this.ChooseDownloadFolderCallback );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseDownloadFolderCallback( bool cancelled, string folder ) {
            try {
                if ( !cancelled ) {
                    this.CurrentPreferences.DownloadFolder = folder;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseExternalPlayer() {
            try {
                lock ( this._commandLockObject ) {
                    var filter = new CommonFileDialogFilter( "Executables", "*.exe" );
                    this._dialogService.ShowFileBrowserDialog( filter, this.CurrentPreferences.MiscExternalPlayer, this.ChooseExternalPlayerCallback );
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ClearExternalPlayer() {
            try {
                lock ( this._commandLockObject ) {
                    this.CurrentPreferences.MiscExternalPlayer = null;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void ChooseExternalPlayerCallback( bool cancelled, string file ) {
            try {
                if ( !cancelled ) {
                    this.CurrentPreferences.MiscExternalPlayer = file;
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Save() {
            try {
                lock ( this._commandLockObject ) {
                    this._dialogService.SetBusy();
                    this.Validate();

                    if ( !this.HasErrors ) {
                        this._preferencesService.Save( this._currentPreferences );
                        this.CurrentPreferences = null;
                        this._notificationService.ShowNotification( "Preferences saved" );
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Undo() {
            try {
                lock ( this._commandLockObject ) {
                    MessageBoxResult result = this._dialogService.ShowMessageBox( "Undo current changes and reload last saved preferences?", "Undo", MessageBoxButton.YesNo, MessageBoxImage.Question );

                    if ( result == MessageBoxResult.Yes ) {
                        this._dialogService.SetBusy();
                        this.CurrentPreferences = null;
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        private void Defaults() {
            try {
                lock ( this._commandLockObject ) {
                    MessageBoxResult result = this._dialogService.ShowMessageBox( "Load default preferences?", "Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question );

                    if ( result == MessageBoxResult.Yes ) {
                        this._dialogService.SetBusy();
                        this._preferencesService.Save( this._preferencesService.CreateDefault() );
                        this.CurrentPreferences = null;
                    }
                }
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        public override void Validate( string propertyName = null ) {
            base.Validate( propertyName );

            string currentProperty = nameof( this.CurrentPreferences );

            if ( string.IsNullOrWhiteSpace( propertyName ) || propertyName == currentProperty ) {
                this.CurrentPreferences?.Validate();

                if ( this.CurrentPreferences.HasErrors ) {
                    this.AddError( currentProperty, "Invalid Preferences!" );
                }
            }
        }

        public override void OnBeforeHidden() {
            try {
                this.CurrentPreferences = null;
            }
            catch ( Exception ex ) {
                this._dialogService.ShowAndLogException( ex );
            }
        }

        protected override List<MenuCommand> BuildMenu() {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if ( menuCommands == null ) {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add( new MenuCommand( this.SaveCommand, "Save", "Save" ) );
            menuCommands.Add( new MenuCommand( this.UndoCommand, "Undo", "Undo" ) );
            menuCommands.Add( new MenuCommand( this.DefaultsCommand, "Default", "Wrench" ) );

            return menuCommands;
        }
    }
}