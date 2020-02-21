namespace TwitchLeecher.Shared.Notification {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public abstract class BindableBase : INotifyPropertyChanged, INotifyDataErrorInfo {

        protected Dictionary<String, String> _currentErrors;

        public Boolean HasErrors {
            get {
                return this._currentErrors.Count > 0;
            }
        }

        public BindableBase() {
            this._currentErrors = new Dictionary<String, String>();
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void ClearErrors() {
            List<String> errorKeys = this._currentErrors.Keys.ToList();

            foreach ( var propertyName in errorKeys ) {
                this._currentErrors.Remove( propertyName );
                this.FireErrorsChanged( propertyName );
            }
        }

        protected void FireErrorsChanged( String propertyName ) {
            this.OnErrorsChanged( propertyName );
        }

        protected virtual void FirePropertyChanged( [CallerMemberName]String propertyName = null ) {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected virtual void OnErrorsChanged( String propertyName ) {
            ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propertyName ) );
        }

        protected void RemoveError( String propertyName ) {
            if ( String.IsNullOrWhiteSpace( propertyName ) ) {
                throw new ArgumentNullException( "propertyName" );
            }

            this._currentErrors.Remove( propertyName );

            this.FireErrorsChanged( propertyName );
        }

        protected virtual Boolean SetProperty<T>( ref T storage, T value, [CallerMemberName] String propertyName = null ) {
            if ( Equals( storage, value ) ) {
                return false;
            }

            storage = value;

            this.FirePropertyChanged( propertyName );

            return true;
        }

        public void AddError( String propertyName, String error ) {
            if ( String.IsNullOrWhiteSpace( propertyName ) ) {
                throw new ArgumentNullException( "propertyName" );
            }

            if ( String.IsNullOrWhiteSpace( error ) ) {
                throw new ArgumentNullException( "error" );
            }

            if ( !this._currentErrors.ContainsKey( propertyName ) ) {
                this._currentErrors.Add( propertyName, error );
            }

            this.FireErrorsChanged( propertyName );
        }

        public IEnumerable GetErrors( String propertyName = null ) {
            if ( String.IsNullOrWhiteSpace( propertyName ) ) {
                return this._currentErrors.Values.ToList();
            }
            else if ( this._currentErrors.ContainsKey( propertyName ) ) {
                return new List<String>() { this._currentErrors[ propertyName ] };
            }

            return null;
        }

        public virtual void Validate( String propertyName = null ) {
            if ( String.IsNullOrWhiteSpace( propertyName ) ) {
                this.ClearErrors();
            }
            else {
                this.RemoveError( propertyName );
            }
        }
    }
}