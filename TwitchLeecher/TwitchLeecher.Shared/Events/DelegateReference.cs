namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Reflection;

    public class DelegateReference : IDelegateReference {

        private readonly Delegate _delegate;
        private readonly Type _delegateType;
        private readonly MethodInfo _method;
        private readonly WeakReference _weakReference;

        public Delegate Target {
            get {
                if ( this._delegate != null ) {
                    return this._delegate;
                }
                else {
                    return this.TryGetDelegate();
                }
            }
        }

        public DelegateReference( Delegate @delegate, Boolean keepReferenceAlive ) {
            if ( @delegate == null ) {
                throw new ArgumentNullException( nameof( @delegate ) );
            }

            if ( keepReferenceAlive ) {
                this._delegate = @delegate;
            }
            else {
                this._weakReference = new WeakReference( @delegate.Target );
                this._method = @delegate.GetMethodInfo();
                this._delegateType = @delegate.GetType();
            }
        }

        private Delegate TryGetDelegate() {
            if ( this._method.IsStatic ) {
                return this._method.CreateDelegate( this._delegateType, null );
            }

            var target = this._weakReference.Target;

            if ( target != null ) {
                return this._method.CreateDelegate( this._delegateType, target );
            }
            return null;
        }
    }
}