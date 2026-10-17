using Allrpcs.Infrastructure.Speech;
using Allrpcs.TestSupport;

namespace Allrpcs.Infrastructure.Tests.Speech;

public sealed class SpeechWorkerLifetimeTests
{
    [Fact]
    public async Task ReadTimeoutCancelsActualPendingReceive()
    {
        ManualTimeProvider clock = new();
        await using BlockedWorker worker = new();
        Task reading = PlayerSpeechProtocol.Read(worker, default, clock);
        Assert.False(reading.IsCompleted);
        clock.Advance(TimeSpan.FromSeconds(10));
        await Assert.ThrowsAsync<TimeoutException>(() => reading);
        Assert.True(worker.ReadToken.IsCancellationRequested);
    }

    [Fact]
    public async Task CallerCancellationRemainsCancellationAndCancelsReceive()
    {
        ManualTimeProvider clock = new();
        await using BlockedWorker worker = new();
        using CancellationTokenSource caller = new();
        Task reading = PlayerSpeechProtocol.Read(worker, caller.Token, clock);
        caller.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reading);
        Assert.True(worker.ReadToken.IsCancellationRequested);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ShutdownCancelsHungExitAndStderrWithinOneDeadline(bool exitHangs)
    {
        ManualTimeProvider clock = new();
        HangingReader error = new();
        TaskCompletionSource exit = new(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken exitToken = default;
        int killed = 0;
        bool released = false;
        OwnedSpeechWorkerLifetime lifetime = new(() => killed++,
            token => { exitToken = token; return exitHangs ? exit.Task : Task.CompletedTask; },
            error, () => released = true, clock);
        Task disposal = lifetime.DisposeAsync().AsTask();
        try
        {
            Assert.Equal(1, killed);
            clock.Advance(TimeSpan.FromSeconds(2));
            Assert.True(error.Token.IsCancellationRequested);
            if (exitHangs) { Assert.True(exitToken.IsCancellationRequested); }
            await disposal.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(released);
        }
        finally
        {
            exit.TrySetResult();
            error.Completion.TrySetResult(0);
            await disposal;
        }
    }

    [Fact]
    public async Task DisposalTerminatesAndReleasesOnlyItsOwnedProcessOnce()
    {
        int firstKills = 0, secondKills = 0, releases = 0;
        OwnedSpeechWorkerLifetime first = new(() => firstKills++, _ => Task.CompletedTask,
            new StringReader("discarded diagnostics"), () => releases++, TimeProvider.System);
        await using OwnedSpeechWorkerLifetime second = new(() => secondKills++, _ => Task.CompletedTask,
            new StringReader(""), () => { }, TimeProvider.System);
        await first.DisposeAsync();
        await first.DisposeAsync();
        Assert.Equal(1, firstKills);
        Assert.Equal(1, releases);
        Assert.Equal(0, secondKills);
    }

    private sealed class HangingReader : TextReader
    {
        internal CancellationToken Token { get; private set; }
        internal TaskCompletionSource<int> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
        { Token = cancellationToken; return new(Completion.Task); }
    }

    private sealed class BlockedWorker : IPlayerSpeechWorker
    {
        internal CancellationToken ReadToken;
        public async ValueTask<string?> ReceiveAsync(CancellationToken token)
        {
            ReadToken = token;
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return null;
        }
        public ValueTask SendAsync(string message, CancellationToken token) => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
