namespace TwitchLeecher.Shared.Events {

    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class EventAggregator : IEventAggregator {

        private readonly Dictionary<Type, EventBase> _events;
        private readonly SynchronizationContext _syncContext;

        public EventAggregator() {
            this._events = new Dictionary<Type, EventBase>();
            this._syncContext = SynchronizationContext.Current;
        }

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new() {
            lock ( this._events ) {
                if ( !this._events.TryGetValue( typeof( TEventType ), out EventBase existingEvent ) ) {
                    TEventType newEvent = new TEventType();
                    newEvent.SynchronizationContext = this._syncContext;
                    this._events[ typeof( TEventType ) ] = newEvent;

                    return newEvent;
                }
                else {
                    return ( TEventType )existingEvent;
                }
            }
        }
    }
}