using System.Runtime.CompilerServices;
using Allrpcs.Application.Speech;
using Xunit;

namespace Allrpcs.Application.Tests.Speech;

public sealed class PlayerSpeechTests
{
    [Fact]
    public void DraftPreservesTypedPrefixManualCorrectionAndFinalUtteranceOrder()
    {
        EditableSpeechDraft draft = new("  Typed prefix ");
        Assert.True(draft.Apply(new(SpeechRecognitionKind.Partial, "b", "first")));
        Assert.True(draft.Apply(new(SpeechRecognitionKind.Final, "b", "first sentence")));
        Assert.False(draft.Apply(new(SpeechRecognitionKind.Partial, "b", "late rewrite")));
        Assert.True(draft.Apply(new(SpeechRecognitionKind.Final, "a", "second sentence")));
        Assert.Equal("  Typed prefix first sentence second sentence", draft.Text);
        draft.Edited("corrected negation");
        Assert.False(draft.Apply(new(SpeechRecognitionKind.Final, "c", "late words")));
        Assert.True(draft.ManuallyEdited);
    }
    [Fact]
    public void DraftRejectsOverlongAndMalformedRecognition()
    {
        EditableSpeechDraft draft = new(new string('a', 2000));
        Assert.Throws<InvalidOperationException>(() => draft.Apply(new(SpeechRecognitionKind.Final, "1", "longer")));
        Assert.Throws<ArgumentException>(() => new SpeechRecognitionEvent(SpeechRecognitionKind.Final, "", "words"));
        Assert.Throws<ArgumentException>(() => new SpeechRecognitionEvent(SpeechRecognitionKind.Partial, "1", "\0"));
    }
    [Fact]
    public async Task CaptureIsOptInAndFinishRetainsFinalTextButStopDiscardsQueuedEvents()
    {
        Source source = new();
        await using PlayerSpeechSession session = new(source, new EchoRecognizer());
        Assert.Equal(0, source.Opens);
        session.Start(SpeechLanguage.German);
        await Wait(() => source.Opens == 1);
        session.Finish();
        await Wait(() => session.Status == SpeechPlaybackStatus.Completed);
        List<SpeechRecognitionEvent> events = [];
        while (session.TryRead(out SpeechRecognitionEvent? value)) { events.Add(value!); }
        Assert.Contains(events, value => value.Kind == SpeechRecognitionKind.Final && value.Text == "final words");
        Assert.True(source.Closed);
        session.Stop(); Assert.False(session.TryRead(out _));
    }
    [Fact]
    public async Task RetiredProviderCannotOverwriteReplacementEvenWhenItIgnoresCancellation()
    {
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        LateRecognizer recognizer = new(release.Task);
        await using PlayerSpeechSession session = new(new Source(), recognizer);
        session.Start(SpeechLanguage.English);
        await Wait(() => recognizer.Calls == 1);
        session.Stop(); Assert.True(session.Retiring);
        session.Start(SpeechLanguage.German);
        await Wait(() => session.Status == SpeechPlaybackStatus.Completed);
        release.SetResult(); await Wait(() => !session.Retiring);
        Assert.True(session.TryRead(out SpeechRecognitionEvent? value));
        Assert.Equal("current", value!.Text); Assert.False(session.TryRead(out _));
    }
    [Fact]
    public async Task EarlyAudioActivityInterruptSignalPrecedesDecoderAndRearmsAfterSilence()
    {
        await using PlayerSpeechSession session = new(new SequenceSource(), new EchoRecognizer());
        session.Start(SpeechLanguage.English);
        await Wait(() => session.Status == SpeechPlaybackStatus.Completed);
        List<SpeechRecognitionEvent> events = [];
        while (session.TryRead(out SpeechRecognitionEvent? value)) { events.Add(value!); }
        Assert.Equal(2, events.Count(value => value.Kind == SpeechRecognitionKind.VoiceStarted));
        Assert.Equal(SpeechRecognitionKind.CaptureStarted, events[0].Kind);
        Assert.Equal(SpeechRecognitionKind.Final, events[^1].Kind);
    }
    [Fact]
    public async Task CaptureFailureIsSafeFailureAndHasNoDraftEvent()
    {
        await using PlayerSpeechSession session = new(new FailedSource(), new EchoRecognizer());
        session.Start(SpeechLanguage.English);
        await Wait(() => session.Status == SpeechPlaybackStatus.Failed);
        Assert.False(session.TryRead(out _));
    }
    private static async Task Wait(Func<bool> condition)
    { using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5)); while (!condition()) { await Task.Delay(5, timeout.Token); } }
    private sealed class Source : IMicrophoneCapture
    {
        internal int Opens; internal bool Closed;
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        {
            Interlocked.Increment(ref Opens);
            try { yield return new(16000, new float[512]); await Task.Delay(Timeout.Infinite, token); }
            finally { Closed = true; }
        }
    }
    private sealed class SequenceSource : IMicrophoneCapture
    {
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        {
            await Task.Yield(); token.ThrowIfCancellationRequested();
            foreach (float level in new[] { 0.1f, 0.1f, 0.1f, 0, 0, 0, 0, 0, 0, 0.1f })
            { yield return new(16000, Enumerable.Repeat(level, 1600).ToArray()); }
        }
    }
    private sealed class FailedSource : IMicrophoneCapture
    {
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        { await Task.Yield(); token.ThrowIfCancellationRequested(); await Task.FromException(new IOException("permission/device unavailable")); yield break; }
    }
    private sealed class EchoRecognizer : ISpeechRecognizer
    {
        public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
            IAsyncEnumerable<SpeechAudioChunk> input, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (SpeechAudioChunk _ in input.WithCancellation(token)) { }
            yield return new(SpeechRecognitionKind.Final, "1", "final words");
        }
    }
    private sealed class LateRecognizer(Task release) : ISpeechRecognizer
    {
        internal int Calls;
        public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
            IAsyncEnumerable<SpeechAudioChunk> input, [EnumeratorCancellation] CancellationToken token)
        {
            Interlocked.Increment(ref Calls);
            if (language == SpeechLanguage.English) { await release; yield return new(SpeechRecognitionKind.Final, "1", "obsolete"); }
            else { yield return new(SpeechRecognitionKind.Final, "1", "current"); }
        }
    }
}
