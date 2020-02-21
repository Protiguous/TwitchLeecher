namespace TwitchLeecher.Shared.Commands {

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class DelegateCommandBase : ICommand {

        private readonly HashSet<String> _propertiesToObserve;
        private INotifyPropertyChanged _inpc;

        protected readonly Func<Object, Task> _executeMethod;
        protected Func<Object, Boolean> _canExecuteMethod;

        protected DelegateCommandBase( Action<Object> executeMethod, Func<Object, Boolean> canExecuteMethod ) {
            if ( executeMethod == null || canExecuteMethod == null ) {
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );
            }

            this._propertiesToObserve = new HashSet<String>();
            this._executeMethod = ( arg ) => { executeMethod( arg ); return Task.Delay( 0 ); };
            this._canExecuteMethod = canExecuteMethod;
        }

        protected DelegateCommandBase( Func<Object, Task> executeMethod, Func<Object, Boolean> canExecuteMethod ) {
            if ( executeMethod == null || canExecuteMethod == null ) {
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );
            }
            this._propertiesToObserve = new HashSet<String>();
            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        public virtual event EventHandler CanExecuteChanged;

        private void Inpc_PropertyChanged( Object sender, PropertyChangedEventArgs e ) {
            if ( this._propertiesToObserve.Contains( e.PropertyName ) )
                this.FireCanExecuteChanged();
        }

        protected void AddPropertyToObserve( String property ) {
            if ( this._propertiesToObserve.Contains( property ) )
                throw new ArgumentException( String.Format( "{0} is already being observed.", property ) );

            this._propertiesToObserve.Add( property );
        }

        protected void HookInpc( MemberExpression expression ) {
            if ( expression == null )
                return;

            if ( this._inpc == null ) {
                if ( expression.Expression is ConstantExpression constantExpression ) {
                    this._inpc = constantExpression.Value as INotifyPropertyChanged;
                    if ( this._inpc != null )
                        this._inpc.PropertyChanged += this.Inpc_PropertyChanged;
                }
            }
        }

        protected virtual void OnCanExecuteChanged() {
            CanExecuteChanged?.Invoke( this, EventArgs.Empty );
        }

        protected internal void ObservesCanExecuteInternal( Expression<Func<Object, Boolean>> canExecuteExpression ) {
            this._canExecuteMethod = canExecuteExpression.Compile();
            this.AddPropertyToObserve( PropertySupport.ExtractPropertyNameFromLambda( canExecuteExpression ) );
            this.HookInpc( canExecuteExpression.Body as MemberExpression );
        }

        protected internal void ObservesPropertyInternal<T>( Expression<Func<T>> propertyExpression ) {
            this.AddPropertyToObserve( PropertySupport.ExtractPropertyName( propertyExpression ) );
            this.HookInpc( propertyExpression.Body as MemberExpression );
        }

        public void FireCanExecuteChanged() {
            this.OnCanExecuteChanged();
        }

        protected virtual Boolean CanExecute( Object parameter ) {
            return this._canExecuteMethod( parameter );
        }

        protected virtual async Task Execute( Object parameter ) {
            await this._executeMethod( parameter );
        }

        async void ICommand.Execute( Object parameter ) {
            await this.Execute( parameter );
        }

        Boolean ICommand.CanExecute( Object parameter ) {
            return this.CanExecute( parameter );
        }
    }
}