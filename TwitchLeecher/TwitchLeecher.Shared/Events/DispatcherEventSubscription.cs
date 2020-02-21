namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Threading;

    public class DispatcherEventSubscription<TPayload> : EventSubscription<TPayload> {

        private readonly SynchronizationContext _syncContext;

        public DispatcherEventSubscription( IDelegateReference actionReference, IDelegateReference filterReference, SynchronizationContext syncContext )
             : base( actionReference, filterReference ) {
            this._syncContext = syncContext;
        }

        public override void InvokeAction( Action<TPayload> action, TPayload argument ) {
            this._syncContext.Post( ( o ) => action( ( TPayload )o ), argument );
        }
    }
}