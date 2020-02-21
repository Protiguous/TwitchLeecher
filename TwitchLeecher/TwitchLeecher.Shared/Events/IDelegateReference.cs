namespace TwitchLeecher.Shared.Events {

    using System;

    public interface IDelegateReference {

        Delegate Target { get; }
    }
}