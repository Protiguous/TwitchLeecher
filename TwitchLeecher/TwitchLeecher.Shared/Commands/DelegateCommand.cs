namespace TwitchLeecher.Shared.Commands {

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    public class DelegateCommand<T> : DelegateCommandBase {

        protected DelegateCommand( Func<T, Task> executeMethod )
            : this( executeMethod, ( o ) => true ) {
        }

        protected DelegateCommand( Func<T, Task> executeMethod, Func<T, Boolean> canExecuteMethod )
            : base( ( o ) => executeMethod( ( T )o ), ( o ) => canExecuteMethod( ( T )o ) ) {
            if ( executeMethod == null || canExecuteMethod == null )
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );
        }

        public DelegateCommand( Action<T> executeMethod )
                               : this( executeMethod, ( o ) => true ) {
        }

        public DelegateCommand( Action<T> executeMethod, Func<T, Boolean> canExecuteMethod )
            : base( ( o ) => executeMethod( ( T )o ), ( o ) => canExecuteMethod( ( T )o ) ) {
            if ( executeMethod == null || canExecuteMethod == null )
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );

            TypeInfo genericTypeInfo = typeof( T ).GetTypeInfo();

            if ( genericTypeInfo.IsValueType ) {
                if ( ( !genericTypeInfo.IsGenericType ) || ( !typeof( Nullable<> ).GetTypeInfo().IsAssignableFrom( genericTypeInfo.GetGenericTypeDefinition().GetTypeInfo() ) ) ) {
                    throw new InvalidCastException( "T for DelegateCommand<T> is not an object nor Nullable" );
                }
            }
        }

        public static DelegateCommand<T> FromAsyncHandler( Func<T, Task> executeMethod ) {
            return new DelegateCommand<T>( executeMethod );
        }

        public static DelegateCommand<T> FromAsyncHandler( Func<T, Task> executeMethod, Func<T, Boolean> canExecuteMethod ) {
            return new DelegateCommand<T>( executeMethod, canExecuteMethod );
        }

        public virtual Boolean CanExecute( T parameter ) {
            return base.CanExecute( parameter );
        }

        public virtual Task Execute( T parameter ) {
            return base.Execute( parameter );
        }

        public DelegateCommand<T> ObservesCanExecute( Expression<Func<Object, Boolean>> canExecuteExpression ) {
            this.ObservesCanExecuteInternal( canExecuteExpression );
            return this;
        }

        public DelegateCommand<T> ObservesProperty<TP>( Expression<Func<TP>> propertyExpression ) {
            this.ObservesPropertyInternal( propertyExpression );
            return this;
        }
    }

    public class DelegateCommand : DelegateCommandBase {

        protected DelegateCommand( Func<Task> executeMethod )
            : this( executeMethod, () => true ) {
        }

        protected DelegateCommand( Func<Task> executeMethod, Func<Boolean> canExecuteMethod )
            : base( ( o ) => executeMethod(), ( o ) => canExecuteMethod() ) {
            if ( executeMethod == null || canExecuteMethod == null )
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );
        }

        public DelegateCommand( Action executeMethod )
                             : this( executeMethod, () => true ) {
        }

        public DelegateCommand( Action executeMethod, Func<Boolean> canExecuteMethod )
             : base( ( o ) => executeMethod(), ( o ) => canExecuteMethod() ) {
            if ( executeMethod == null || canExecuteMethod == null )
                throw new ArgumentNullException( nameof( executeMethod ), "Neither the executeMethod nor the canExecuteMethod delegates can be null" );
        }

        public static DelegateCommand FromAsyncHandler( Func<Task> executeMethod ) {
            return new DelegateCommand( executeMethod );
        }

        public static DelegateCommand FromAsyncHandler( Func<Task> executeMethod, Func<Boolean> canExecuteMethod ) {
            return new DelegateCommand( executeMethod, canExecuteMethod );
        }

        public virtual Boolean CanExecute() {
            return this.CanExecute( null );
        }

        public virtual Task Execute() {
            return this.Execute( null );
        }

        public DelegateCommand ObservesCanExecute( Expression<Func<Object, Boolean>> canExecuteExpression ) {
            this.ObservesCanExecuteInternal( canExecuteExpression );
            return this;
        }

        public DelegateCommand ObservesProperty<T>( Expression<Func<T>> propertyExpression ) {
            this.ObservesPropertyInternal( propertyExpression );
            return this;
        }
    }
}