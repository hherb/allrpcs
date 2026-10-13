using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Allrpcs.Application.Speech;

/// <summary>Owns one bounded capture/recognition generation and rejects every obsolete callback.</summary>
public sealed class PlayerSpeechSession(IMicrophoneCapture capture, ISpeechRecognizer recognizer) : IAsyncDisposable
{
    private readonly object sync = new();
    private readonly HashSet<Generation> pending = [];
    private Generation? current;
    private bool disposed;
    private SpeechPlaybackStatus status;
    /// <summary>Gets presentation status without exposing recorded samples or text.</summary>
    public SpeechPlaybackStatus Status { get { lock (sync) { return status; } } }
    /// <summary>Reports pending cancelled cleanup so the UI does not prematurely claim the mic is off.</summary>
    public bool Retiring { get { lock (sync) { return pending.Any(value => !ReferenceEquals(value, current)); } } }
    /// <summary>Starts microphone capture only after an explicit UI action.</summary>
    public void Start(SpeechLanguage language)
    {
        if (!Enum.IsDefined(language)) { throw new ArgumentOutOfRangeException(nameof(language)); }
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this); StopCurrent();
            Generation generation = new(); current = generation; pending.Add(generation);
            status = SpeechPlaybackStatus.Generating;
            generation.Task = Task.Run(() => RunAsync(generation, language));
        }
    }
    /// <summary>Ends hold-to-talk capture but preserves its final transcription and draft.</summary>
    public void Finish()
    { lock (sync) { if (current is { Finished: false } value) { value.FinishRequested = true; value.Capture.Cancel(); } } }
    /// <summary>Drops queued text synchronously and cancels both microphone and recognition workers.</summary>
    public void Stop()
    { lock (sync) { StopCurrent(); status = SpeechPlaybackStatus.Cancelled; } }
    /// <summary>Reads only events belonging to the current generation.</summary>
    public bool TryRead(out SpeechRecognitionEvent? value)
    { lock (sync) { value = null; return current is not null && current.Events.Reader.TryRead(out value); } }
    private void StopCurrent()
    {
        Generation? value = current; current = null;
        if (value is null) { return; }
        if (value.Finished) { value.Dispose(); pending.Remove(value); }
        else { value.Cancel.Cancel(); value.Capture.Cancel(); }
    }
    private async Task RunAsync(Generation generation, SpeechLanguage language)
    {
        SpeechPlaybackStatus result = SpeechPlaybackStatus.Completed;
        try
        {
            await foreach (SpeechRecognitionEvent value in recognizer.RecognizeAsync(language,
                Audio(generation), generation.Cancel.Token).WithCancellation(generation.Cancel.Token).ConfigureAwait(false))
            {
                generation.Cancel.Token.ThrowIfCancellationRequested();
                await generation.Events.Writer.WriteAsync(value, generation.Cancel.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (generation.Cancel.IsCancellationRequested) { result = SpeechPlaybackStatus.Cancelled; }
        catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        { result = SpeechPlaybackStatus.Failed; }
        finally
        {
            generation.Capture.Cancel(); generation.Events.Writer.TryComplete();
            lock (sync)
            {
                generation.Finished = true;
                if (ReferenceEquals(current, generation)) { status = result; }
                else { generation.Dispose(); pending.Remove(generation); }
            }
        }
    }
    private async IAsyncEnumerable<SpeechAudioChunk> Audio(Generation generation,
        [EnumeratorCancellation] CancellationToken ignored = default)
    {
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(generation.Capture.Token, generation.Cancel.Token, ignored);
        await using IAsyncEnumerator<SpeechAudioChunk> source = capture.CaptureAsync(linked.Token).GetAsyncEnumerator(ignored);
        bool activated = false; double duration = 0; double loud = 0; double quiet = 0; bool triggered = false;
        while (true)
        {
            bool moved;
            try { moved = await source.MoveNextAsync().ConfigureAwait(false); }
            catch (OperationCanceledException) when (generation.FinishRequested && !generation.Cancel.IsCancellationRequested) { break; }
            if (!moved) { break; }
            generation.Cancel.Token.ThrowIfCancellationRequested();
            SpeechAudioChunk chunk = source.Current;
            if (!activated)
            {
                activated = true;
                await generation.Events.Writer.WriteAsync(new(SpeechRecognitionKind.CaptureStarted), generation.Cancel.Token).ConfigureAwait(false);
            }
            double seconds = (double)chunk.Samples.Count / chunk.SampleRate;
            duration += seconds;
            double rms = Math.Sqrt(chunk.Samples.Sum(sample => (double)sample * sample) / chunk.Samples.Count);
            if (rms >= 0.02) { loud += seconds; quiet = 0; }
            else { loud = 0; quiet += seconds; if (quiet >= 0.5) { triggered = false; } }
            if (!triggered && loud >= 0.096)
            {
                triggered = true;
                await generation.Events.Writer.WriteAsync(new(SpeechRecognitionKind.VoiceStarted), generation.Cancel.Token).ConfigureAwait(false);
            }
            yield return chunk;
            if (duration >= 30) { break; }
        }
    }
    /// <summary>Awaits every microphone/recognition producer before scene retirement.</summary>
    public async ValueTask DisposeAsync()
    {
        Task[] tasks;
        lock (sync)
        {
            if (disposed) { return; }
            disposed = true; StopCurrent();
            tasks = pending.Select(value => value.Task).ToArray();
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
    private sealed class Generation : IDisposable
    {
        internal CancellationTokenSource Cancel { get; } = new();
        internal CancellationTokenSource Capture { get; } = new();
        internal Channel<SpeechRecognitionEvent> Events { get; } = Channel.CreateBounded<SpeechRecognitionEvent>(16);
        internal Task Task { get; set; } = Task.CompletedTask;
        internal bool Finished;
        internal volatile bool FinishRequested;
        public void Dispose() { Cancel.Dispose(); Capture.Dispose(); }
    }
}
