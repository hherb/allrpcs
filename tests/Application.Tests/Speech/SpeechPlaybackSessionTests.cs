using System.Runtime.CompilerServices;
using Allrpcs.Application.Speech;
using Xunit;

namespace Allrpcs.Application.Tests.Speech;

public sealed class SpeechPlaybackSessionTests
{
    [Fact]
    public async Task ReplacementRejectsLateOldAudioEvenWhenProviderIgnoresCancellation()
    {
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using SpeechPlaybackSession session = new(new LateProvider(release.Task));
        session.Start(Request("old"));
        await Wait(() => session.TryRead(out _));
        session.Start(Request("new"));
        release.SetResult();
        await Wait(() => session.Status == SpeechPlaybackStatus.Completed);
        Assert.True(session.TryRead(out SpeechAudioChunk? chunk));
        Assert.Equal(0.2f, chunk!.Samples[0]);
        Assert.False(session.TryRead(out _));
    }

    [Fact]
    public async Task StopImmediatelyDropsQueuedPcmAndRetiresBackpressuredProducer()
    {
        CountingProvider provider = new();
        await using SpeechPlaybackSession session = new(provider);
        session.Start(Request("text"));
        await Wait(() => provider.Yielded >= SpeechPlaybackSession.MaximumQueuedChunks + 1);
        Assert.Equal(SpeechPlaybackSession.MaximumQueuedChunks + 1, provider.Yielded);
        session.Stop();
        Assert.False(session.TryRead(out _));
        await Wait(() => provider.Disposed);
        Assert.Equal(SpeechPlaybackStatus.Cancelled, session.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmptyOrFailedOutputIsSafeTextFallback(bool throws)
    {
        await using SpeechPlaybackSession session = new(new FailingProvider(throws));
        session.Start(Request("exact text"));
        await Wait(() => session.Status == SpeechPlaybackStatus.Failed);
        Assert.False(session.TryRead(out _));
    }

    [Fact]
    public async Task CompletedChunkIsExactAndStopAfterCompletionIsSafe()
    {
        await using SpeechPlaybackSession session = new(new LateProvider(Task.CompletedTask));
        session.Start(Request("new"));
        await Wait(() => session.Status == SpeechPlaybackStatus.Completed);
        Assert.True(session.TryRead(out SpeechAudioChunk? chunk));
        Assert.Equal(24000, chunk!.SampleRate);
        session.Stop();
        session.Stop();
        Assert.False(session.TryRead(out _));
        await session.DisposeAsync();
        Assert.Throws<ObjectDisposedException>(() => session.Start(Request("new")));
    }

    private static SpeechSynthesisRequest Request(string text) => new(text, SpeechLanguage.English, "default");
    private static SpeechAudioChunk Chunk(float sample) => new(24000, [sample]);
    private static async Task Wait(Func<bool> ready)
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(5));
        while (!ready()) { await Task.Delay(5, deadline.Token); }
    }

    private sealed class LateProvider(Task release) : ISpeechSynthesizer
    {
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return Chunk(request.Text == "old" ? 0.1f : 0.2f);
            if (request.Text == "old") { await release; yield return Chunk(0.9f); }
        }
    }

    private sealed class CountingProvider : ISpeechSynthesizer
    {
        internal int Yielded;
        internal bool Disposed;
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            try
            {
                await Task.Yield();
                for (int index = 0; index < 100; index++)
                { Interlocked.Increment(ref Yielded); yield return Chunk(0.3f); }
            }
            finally { Disposed = true; }
        }
    }

    private sealed class FailingProvider(bool throws) : ISpeechSynthesizer
    {
        public async IAsyncEnumerable<SpeechAudioChunk> SynthesizeAsync(SpeechSynthesisRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();
            if (throws) { yield return Chunk(0.4f); throw new SpeechSynthesisException("unavailable"); }
            yield break;
        }
    }
}
