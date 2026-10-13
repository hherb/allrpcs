using System.Threading.Channels;

namespace Allrpcs.Application.Speech;

/// <summary>Current presentation synthesis outcome; it never describes a dialogue/world transition.</summary>
public enum SpeechPlaybackStatus { Idle, Generating, Completed, Failed, Cancelled }

/// <summary>Bounds pending PCM and prevents replaced/cancelled utterances from reaching playback.</summary>
public sealed class SpeechPlaybackSession : IAsyncDisposable
{
    /// <summary>At most four immutable chunks wait behind a slow audio consumer.</summary>
    public const int MaximumQueuedChunks = 4;
    private readonly object sync = new();
    private readonly ISpeechSynthesizer synthesizer;
    private readonly HashSet<Generation> pending = [];
    private Generation? current;
    private SpeechPlaybackStatus status;
    private bool disposed;

    /// <summary>Retains a caller-owned synthesizer; disposal does not dispose that provider.</summary>
    public SpeechPlaybackSession(ISpeechSynthesizer synthesizer)
    {
        ArgumentNullException.ThrowIfNull(synthesizer);
        this.synthesizer = synthesizer;
    }

    /// <summary>Reports the current generation without exposing its text or provider.</summary>
    public SpeechPlaybackStatus Status { get { lock (sync) { return status; } } }

    /// <summary>Cancels the previous generation and starts one bounded, fresh PCM channel.</summary>
    public void Start(SpeechSynthesisRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            StopCurrent();
            Generation generation = new();
            current = generation;
            pending.Add(generation);
            status = SpeechPlaybackStatus.Generating;
            generation.Task = Task.Run(() => ProduceAsync(generation, request));
        }
    }

    /// <summary>Consumes only the current generation; late old PCM is never replayed.</summary>
    public bool TryRead(out SpeechAudioChunk? chunk)
    {
        lock (sync)
        {
            chunk = null;
            return current is not null && current.Channel.Reader.TryRead(out chunk);
        }
    }

    /// <summary>Immediately drops queued PCM and cancels the producer without waiting for cleanup.</summary>
    public void Stop()
    {
        lock (sync)
        {
            if (disposed) { return; }
            StopCurrent();
            status = SpeechPlaybackStatus.Cancelled;
        }
    }

    private void StopCurrent()
    {
        Generation? previous = current;
        current = null;
        if (previous is null) { return; }
        if (previous.Finished)
        {
            previous.Cancellation.Dispose();
            pending.Remove(previous);
        }
        else { previous.Cancellation.Cancel(); }
    }

    private async Task ProduceAsync(Generation generation, SpeechSynthesisRequest request)
    {
        SpeechPlaybackStatus result = SpeechPlaybackStatus.Completed;
        try
        {
            int chunks = 0;
            await foreach (SpeechAudioChunk chunk in synthesizer.SynthesizeAsync(request, generation.Cancellation.Token)
                .WithCancellation(generation.Cancellation.Token).ConfigureAwait(false))
            {
                generation.Cancellation.Token.ThrowIfCancellationRequested();
                await generation.Channel.Writer.WriteAsync(chunk, generation.Cancellation.Token).ConfigureAwait(false);
                chunks++;
            }
            if (chunks == 0) { result = SpeechPlaybackStatus.Failed; }
        }
        catch (OperationCanceledException) { result = SpeechPlaybackStatus.Cancelled; }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException
            and not AccessViolationException)
        { result = SpeechPlaybackStatus.Failed; }
        finally
        {
            generation.Channel.Writer.TryComplete();
            lock (sync)
            {
                generation.Finished = true;
                if (ReferenceEquals(current, generation))
                {
                    status = result;
                    if (result == SpeechPlaybackStatus.Failed)
                    {
                        while (generation.Channel.Reader.TryRead(out _)) { }
                    }
                }
                else
                {
                    generation.Cancellation.Dispose();
                    pending.Remove(generation);
                }
            }
        }
    }

    /// <summary>Retires every outstanding producer before releasing cancellation resources.</summary>
    public async ValueTask DisposeAsync()
    {
        Task[] tasks;
        lock (sync)
        {
            if (disposed) { return; }
            disposed = true;
            StopCurrent();
            tasks = pending.Select(generation => generation.Task).ToArray();
            status = SpeechPlaybackStatus.Cancelled;
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private sealed class Generation
    {
        internal CancellationTokenSource Cancellation { get; } = new();
        internal Channel<SpeechAudioChunk> Channel { get; } = System.Threading.Channels.Channel.CreateBounded<SpeechAudioChunk>(
            new BoundedChannelOptions(MaximumQueuedChunks) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true });
        internal Task Task { get; set; } = Task.CompletedTask;
        internal bool Finished { get; set; }
    }
}
