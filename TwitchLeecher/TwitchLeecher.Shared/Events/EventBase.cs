namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public abstract class EventBase {

        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();

        protected ICollection<IEventSubscription> Subscriptions {
            get {
                return this._subscriptions;
            }
        }

        public SynchronizationContext SynchronizationContext { get; set; }

        private List<Action<Object[]>> PruneAndReturnStrategies() {
            List<Action<Object[]>> returnList = new List<Action<Object[]>>();

            lock ( this.Subscriptions ) {
                for ( var i = this.Subscriptions.Count - 1; i >= 0; i-- ) {
                    Action<Object[]> listItem = this._subscriptions[ i ].GetExecutionStrategy();

                    if ( listItem == null ) {
                        this._subscriptions.RemoveAt( i );
                    }
                    else {
                        returnList.Add( listItem );
                    }
                }
            }

            return returnList;
        }

        protected virtual void InternalPublish( params Object[] arguments ) {
            List<Action<Object[]>> executionStrategies = this.PruneAndReturnStrategies();

            foreach ( var executionStrategy in executionStrategies ) {
                executionStrategy( arguments );
            }
        }

        protected virtual SubscriptionToken InternalSubscribe( IEventSubscription eventSubscription ) {
            if ( eventSubscription == null ) {
                throw new ArgumentNullException( nameof( eventSubscription ) );
            }

            eventSubscription.SubscriptionToken = new SubscriptionToken( this.Unsubscribe );

            lock ( this.Subscriptions ) {
                this.Subscriptions.Add( eventSubscription );
            }

            return eventSubscription.SubscriptionToken;
        }

        public virtual Boolean Contains( SubscriptionToken token ) {
            lock ( this.Subscriptions ) {
                return this.Subscriptions.FirstOrDefault( evt => evt.SubscriptionToken == token ) != null;
            }
        }

        public virtual void Unsubscribe( SubscriptionToken token ) {
            lock ( this.Subscriptions ) {
                IEventSubscription subscription = this.Subscriptions.FirstOrDefault( evt => evt.SubscriptionToken == token );

                if ( subscription != null ) {
                    this.Subscriptions.Remove( subscription );
                }
            }
        }
    }
}