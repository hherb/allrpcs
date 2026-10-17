namespace Allrpcs.Game;

/// <summary>Observes detached work and releases its cancellation owners when it finally settles.</summary>
internal static class RetiredRestoreTask
{
    internal static void Retire(Task? task, params CancellationTokenSource?[] sources)
    {
        foreach (CancellationTokenSource? source in sources)
        {
            try { source?.Cancel(); }
            catch (AggregateException) { /* A callback cannot prevent the remaining owners retiring. */ }
        }
        if (task is null)
        {
            foreach (CancellationTokenSource? source in sources) { source?.Dispose(); }
            return;
        }
        _ = task.ContinueWith(completed =>
        {
            _ = completed.Exception;
            foreach (CancellationTokenSource? source in sources) { source?.Dispose(); }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
}
