namespace TwitchLeecher.Shared.Events {

    using System;

    public sealed class SubscriptionToken : IEquatable<SubscriptionToken>, IDisposable {

        private readonly Guid _token;
        private Action<SubscriptionToken> _unsubscribeAction;

        public SubscriptionToken( Action<SubscriptionToken> unsubscribeAction ) {
            this._unsubscribeAction = unsubscribeAction;
            this._token = Guid.NewGuid();
        }

        public void Dispose() {
            if ( this._unsubscribeAction != null ) {
                this._unsubscribeAction( this );
                this._unsubscribeAction = null;
            }

            GC.SuppressFinalize( this );
        }

        public Boolean Equals( SubscriptionToken other ) {
            if ( other == null ) {
                return false;
            }

            return Equals( this._token, other._token );
        }

        public override Boolean Equals( Object obj ) {
            if ( ReferenceEquals( this, obj ) ) {
                return true;
            }

            return this.Equals( obj as SubscriptionToken );
        }

        public override Int32 GetHashCode() {
            return this._token.GetHashCode();
        }
    }
}