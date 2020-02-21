namespace TwitchLeecher.Shared.Events {

    using System;

    public interface IEventSubscription {

        SubscriptionToken SubscriptionToken { get; set; }

        Action<Object[]> GetExecutionStrategy();
    }
}