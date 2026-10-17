using System.Diagnostics;

namespace Allrpcs.Infrastructure.Speech;

/// <summary>Bounds shutdown of one explicitly owned speech process and its discarded stderr.</summary>
internal sealed class OwnedSpeechWorkerLifetime : IAsyncDisposable
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(2);
    private readonly Action terminate;
    private readonly Func<CancellationToken, Task> waitForExit;
    private readonly Action release;
    private readonly TimeProvider clock;
    private readonly CancellationTokenSource stopping = new();
    private readonly Task stderr;
    private bool disposed;

    internal OwnedSpeechWorkerLifetime(Process process) : this(
        () => Terminate(process),
        token => process.WaitForExitAsync(token), process.StandardError, process.Dispose, TimeProvider.System)
    { }

    internal OwnedSpeechWorkerLifetime(Action terminate, Func<CancellationToken, Task> waitForExit,
        TextReader error, Action release, TimeProvider clock)
    {
        this.terminate = terminate;
        this.waitForExit = waitForExit;
        this.release = release;
        this.clock = clock;
        stderr = Drain(error);
        ObserveFault(stderr);
    }

    private static void Terminate(Process process)
    {
        try { if (!process.HasExited) { process.Kill(entireProcessTree: true); } }
        catch (InvalidOperationException) when (process.HasExited) { }
    }

    private async Task Drain(TextReader error)
    {
        char[] discarded = new char[4096];
        while (await error.ReadAsync(discarded.AsMemory(), stopping.Token).ConfigureAwait(false) > 0) { }
    }

    private static void ObserveFault(Task task) =>
        _ = task.ContinueWith(completed => _ = completed.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    public async ValueTask DisposeAsync()
    {
        if (disposed) { return; }
        disposed = true;
        try
        {
            using CancellationTokenSource deadline = new(ShutdownTimeout, clock);
            using CancellationTokenRegistration cancellation = deadline.Token.Register(stopping.Cancel);
            terminate();
            Task exit = waitForExit(deadline.Token);
            ObserveFault(exit);
            try
            {
                // WaitAsync also bounds adapters/streams that fail to honor their cancellation token.
                Task completion = Task.WhenAll(exit, stderr);
                ObserveFault(completion);
                await completion.WaitAsync(deadline.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested) { }
            catch (Exception error) when (error is IOException or ObjectDisposedException) { }
        }
        finally
        {
            stopping.Cancel();
            release();
            stopping.Dispose();
        }
    }
}
