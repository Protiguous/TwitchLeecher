namespace TwitchLeecher.Shared.Events {

    using System;

    public class EventSubscription<TPayload> : IEventSubscription {

        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        public Action<TPayload> Action {
            get {
                return ( Action<TPayload> )this._actionReference.Target;
            }
        }

        public Predicate<TPayload> Filter {
            get {
                return ( Predicate<TPayload> )this._filterReference.Target;
            }
        }

        public SubscriptionToken SubscriptionToken { get; set; }

        public EventSubscription( IDelegateReference actionReference, IDelegateReference filterReference ) {
            if ( actionReference == null ) {
                throw new ArgumentNullException( nameof( actionReference ) );
            }

            if ( !( actionReference.Target is Action<TPayload> ) ) {
                throw new ArgumentException( "Invalid action reference target type", nameof( actionReference ) );
            }

            if ( filterReference == null ) {
                throw new ArgumentNullException( nameof( filterReference ) );
            }

            if ( !( filterReference.Target is Predicate<TPayload> ) ) {
                throw new ArgumentException( "Invalid filter reference target type", nameof( filterReference ) );
            }

            this._actionReference = actionReference;
            this._filterReference = filterReference;
        }

        public virtual Action<Object[]> GetExecutionStrategy() {
            Action<TPayload> action = this.Action;
            Predicate<TPayload> filter = this.Filter;

            if ( action != null && filter != null ) {
                return arguments => {
                    TPayload argument = default( TPayload );
                    if ( arguments != null && arguments.Length > 0 && arguments[ 0 ] != null ) {
                        argument = ( TPayload )arguments[ 0 ];
                    }
                    if ( filter( argument ) ) {
                        this.InvokeAction( action, argument );
                    }
                };
            }

            return null;
        }

        public virtual void InvokeAction( Action<TPayload> action, TPayload argument ) {
            if ( action == null ) {
                throw new ArgumentNullException( nameof( action ) );
            }

            action( argument );
        }
    }
}