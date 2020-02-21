namespace TwitchLeecher.Core.Models {

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class DownloadTask {

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public Task ContinueTask { get; private set; }

        public Task Task { get; private set; }

        public DownloadTask( Task task, Task continueTask, CancellationTokenSource cancellationTokenSource ) {
            this.Task = task ?? throw new ArgumentNullException( nameof( task ) );
            this.ContinueTask = continueTask ?? throw new ArgumentNullException( nameof( continueTask ) );
            this.CancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException( nameof( cancellationTokenSource ) );
        }
    }
}