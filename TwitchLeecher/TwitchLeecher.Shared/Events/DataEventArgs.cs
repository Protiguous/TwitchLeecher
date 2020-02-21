namespace TwitchLeecher.Shared.Events {

    using System;

    public class DataEventArgs<TData> : EventArgs {

        public TData Value { get; }

        public DataEventArgs( TData value ) {
            this.Value = value;
        }
    }
}