namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Collections.Generic;

    public class EmptyPresentationEvent : EventBase {

        private readonly PubSubEvent<Object> _innerEvent;
        private readonly Dictionary<Action, Action<Object>> _subscriberActions;

        public EmptyPresentationEvent() {
            this._innerEvent = new PubSubEvent<Object>();
            this._subscriberActions = new Dictionary<Action, Action<Object>>();
        }

        public void Publish() {
            this._innerEvent.Publish( null );
        }

        public void Subscribe( Action action ) {
            this.Subscribe( action, false );
        }

        public void Subscribe( Action action, Boolean keepSubscriberReferenceAlive ) {
            this.Subscribe( action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive );
        }

        public void Subscribe( Action action, ThreadOption threadOption ) {
            this.Subscribe( action, threadOption, false );
        }

        public void Subscribe( Action action, ThreadOption threadOption, Boolean keepSubscriberReferenceAlive ) {
            void wrappedAction( Object o ) {
                action();
            }

            this._subscriberActions.Add( action, wrappedAction );
            this._innerEvent.Subscribe( wrappedAction, threadOption, keepSubscriberReferenceAlive );
        }

        public void Unsubscribe( Action action ) {
            if ( !this._subscriberActions.ContainsKey( action ) ) {
                return;
            }

            Action<Object> wrappedActionToUnsubscribe = this._subscriberActions[ action ];
            this._innerEvent.Unsubscribe( wrappedActionToUnsubscribe );
            this._subscriberActions.Remove( action );
        }
    }
}