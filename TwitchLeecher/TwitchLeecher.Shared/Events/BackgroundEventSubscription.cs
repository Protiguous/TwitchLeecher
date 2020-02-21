namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Threading.Tasks;

    public class BackgroundEventSubscription<TPayload> : EventSubscription<TPayload> {

        public BackgroundEventSubscription( IDelegateReference actionReference, IDelegateReference filterReference )
             : base( actionReference, filterReference ) {
        }

        public override void InvokeAction( Action<TPayload> action, TPayload argument ) {
            Task.Run( () => action( argument ) );
        }
    }
}