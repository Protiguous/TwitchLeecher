namespace TwitchLeecher.Shared.Helpers {

    using System.Collections.Concurrent;

    public class FixedSizeQueue<T> : ConcurrentQueue<T> {

        private readonly System.Object _lockObject = new System.Object();

        public System.Int32 Size { get; }

        public FixedSizeQueue( System.Int32 size ) {
            this.Size = size;
        }

        public new void Enqueue( T obj ) {
            base.Enqueue( obj );

            lock ( this._lockObject ) {
                while ( this.Count > this.Size ) {
                    base.TryDequeue( out T result );
                }
            }
        }
    }
}