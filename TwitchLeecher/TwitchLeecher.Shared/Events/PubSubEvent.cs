namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Linq;

    public class PubSubEvent<TPayload> : EventBase {

        public virtual Boolean Contains( Action<TPayload> subscriber ) {
            IEventSubscription eventSubscription;

            lock ( this.Subscriptions ) {
                eventSubscription = this.Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault( evt => evt.Action == subscriber );
            }

            return eventSubscription != null;
        }

        public virtual void Publish( TPayload payload ) {
            base.InternalPublish( payload );
        }

        public SubscriptionToken Subscribe( Action<TPayload> action ) {
            return this.Subscribe( action, ThreadOption.PublisherThread );
        }

        public SubscriptionToken Subscribe( Action<TPayload> action, ThreadOption threadOption ) {
            return this.Subscribe( action, threadOption, false );
        }

        public SubscriptionToken Subscribe( Action<TPayload> action, Boolean keepSubscriberReferenceAlive ) {
            return this.Subscribe( action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive );
        }

        public SubscriptionToken Subscribe( Action<TPayload> action, ThreadOption threadOption, Boolean keepSubscriberReferenceAlive ) {
            return this.Subscribe( action, threadOption, keepSubscriberReferenceAlive, null );
        }

        public virtual SubscriptionToken Subscribe( Action<TPayload> action, ThreadOption threadOption, Boolean keepSubscriberReferenceAlive, Predicate<TPayload> filter ) {
            IDelegateReference actionReference = new DelegateReference( action, keepSubscriberReferenceAlive );
            IDelegateReference filterReference;

            if ( filter != null ) {
                filterReference = new DelegateReference( filter, keepSubscriberReferenceAlive );
            }
            else {
                filterReference = new DelegateReference( new Predicate<TPayload>( delegate { return true; } ), true );
            }

            EventSubscription<TPayload> subscription;

            switch ( threadOption ) {
                case ThreadOption.PublisherThread:
                    subscription = new EventSubscription<TPayload>( actionReference, filterReference );
                    break;

                case ThreadOption.BackgroundThread:
                    subscription = new BackgroundEventSubscription<TPayload>( actionReference, filterReference );
                    break;

                case ThreadOption.UIThread:
                    if ( this.SynchronizationContext == null )
                        throw new InvalidOperationException( "To use the UIThread option for subscribing, the EventAggregator must be constructed on the UI thread" );
                    subscription = new DispatcherEventSubscription<TPayload>( actionReference, filterReference, this.SynchronizationContext );
                    break;

                default:
                    subscription = new EventSubscription<TPayload>( actionReference, filterReference );
                    break;
            }

            return base.InternalSubscribe( subscription );
        }

        public virtual void Unsubscribe( Action<TPayload> subscriber ) {
            lock ( this.Subscriptions ) {
                IEventSubscription eventSubscription = this.Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault( evt => evt.Action == subscriber );

                if ( eventSubscription != null ) {
                    this.Subscriptions.Remove( eventSubscription );
                }
            }
        }
    }
}