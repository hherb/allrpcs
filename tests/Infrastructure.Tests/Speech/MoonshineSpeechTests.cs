using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Allrpcs.Application.Speech;
using Allrpcs.Infrastructure.Speech;
using Xunit;

namespace Allrpcs.Infrastructure.Tests.Speech;

public sealed class MoonshineSpeechTests
{
    [Fact]
    public async Task RecognizerStreamsPartialsFinalsExactPcmAndSelectedLanguage()
    {
        await using Factory factory = new();
        List<SpeechRecognitionEvent> values = [];
        await foreach (SpeechRecognitionEvent value in new MoonshineSpeechRecognizer(factory).RecognizeAsync(
            SpeechLanguage.German, Input(), default)) { values.Add(value); }
        Assert.Equal(SpeechLanguage.German, factory.Language);
        Assert.False(factory.Microphone);
        Assert.Equal(new[] { SpeechRecognitionKind.Partial, SpeechRecognitionKind.Final }, values.Select(value => value.Kind));
        Assert.Equal("Do not take it", values[^1].Text);
        Assert.True(factory.Worker!.Disposed);
        JsonElement audio = factory.Worker.Requests.Single(value => value.GetProperty("type").GetString() == "audio");
        Assert.Equal(0.25f, BinaryPrimitives.ReadSingleLittleEndian(Convert.FromBase64String(audio.GetProperty("pcm").GetString()!)));
    }
    [Fact]
    public async Task MicrophoneDoesNotOpenBeforeEnumerationAndCancellationClosesIt()
    {
        await using Factory factory = new(); PortAudioMicrophoneCapture capture = new(factory);
        using CancellationTokenSource token = new();
        await using IAsyncEnumerator<SpeechAudioChunk> iterator = capture.CaptureAsync(token.Token).GetAsyncEnumerator();
        Assert.Null(factory.Worker);
        Assert.True(await iterator.MoveNextAsync()); Assert.True(factory.Microphone);
        Assert.Equal(0.25f, iterator.Current.Samples[0]);
        token.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => iterator.MoveNextAsync().AsTask());
        Assert.True(factory.Worker!.Disposed);
    }
    [Theory]
    [InlineData(11025)]
    [InlineData(22050)]
    [InlineData(44100)]
    [InlineData(48000)]
    public async Task CaptureFinishedNormallyClosesWorkerAtSupportedRates(int rate)
    {
        await using Factory factory = new() { Rate = rate, FinishCapture = true };
        List<SpeechAudioChunk> chunks = [];
        await foreach (SpeechAudioChunk chunk in new PortAudioMicrophoneCapture(factory).CaptureAsync(default))
        { chunks.Add(chunk); }
        Assert.Equal(rate, Assert.Single(chunks).SampleRate);
        Assert.True(factory.Worker!.Disposed);
    }

    [Theory]
    [InlineData("denied")]
    [InlineData("device_lost")]
    public async Task MicrophoneErrorsRetireCaptureWithoutReturningAudio(string error)
    {
        await using Factory factory = new() { Error = error };
        await using IAsyncEnumerator<SpeechAudioChunk> iterator = new PortAudioMicrophoneCapture(factory).CaptureAsync(default).GetAsyncEnumerator();
        await Assert.ThrowsAsync<IOException>(() => iterator.MoveNextAsync().AsTask());
        Assert.True(factory.Worker!.Disposed);
    }
    [Theory]
    [InlineData("{\"type\":\"finished\",\"unexpected\":true}")]
    [InlineData("{\"type\":\"unknown\"}")]
    [InlineData("{\"type\":\"error\",\"code\":\"device_lost\"}")]
    public async Task NonCompletionMessagesFailCaptureAndCloseWorker(string message)
    {
        await using Factory factory = new() { CaptureMessage = message };
        await using IAsyncEnumerator<SpeechAudioChunk> iterator = new PortAudioMicrophoneCapture(factory)
            .CaptureAsync(default).GetAsyncEnumerator();
        await Assert.ThrowsAsync<IOException>(() => iterator.MoveNextAsync().AsTask());
        Assert.True(factory.Worker!.Disposed);
    }

    [Fact]
    public async Task MalformedRecognitionSequenceClosesWorker()
    {
        await using Factory factory = new() { WrongSequence = true };
        await using IAsyncEnumerator<SpeechRecognitionEvent> iterator = new MoonshineSpeechRecognizer(factory)
            .RecognizeAsync(SpeechLanguage.English, Input(), default).GetAsyncEnumerator();
        await Assert.ThrowsAsync<IOException>(() => iterator.MoveNextAsync().AsTask());
        Assert.True(factory.Worker!.Disposed);
    }
    // The input fails only after the recognizer is provably blocked on a receive the worker never
    // answers, so an IOException (not the receive's 10 s TimeoutException) proves the failure is not
    // queued behind that receive. The enumeration starts under a context that never runs posted
    // work: propagation must not depend on the consumer's context (a source continuation posted to
    // xUnit's shared context once missed a 2 s wall-clock bound on a loaded CI runner). The 30 s
    // bound only turns a deadlocked regression into a failure; it is not what the test measures.
    [Fact]
    public async Task InputFailureCancelsReceiverAndClosesWorker()
    {
        await using Factory factory = new();
        TaskCompletionSource inputFailure = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using IAsyncEnumerator<SpeechRecognitionEvent> iterator = new MoonshineSpeechRecognizer(factory)
            .RecognizeAsync(SpeechLanguage.English, FailedInput(inputFailure.Task), default).GetAsyncEnumerator();
        HeldContext consumer = new();
        SynchronizationContext? previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(consumer);
        Task<bool> next;
        try { next = iterator.MoveNextAsync().AsTask(); }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
        Assert.False(next.IsCompleted);
        Assert.Equal(2, factory.Worker!.Receives);
        inputFailure.SetException(new IOException("source lost"));
        IOException error = await Assert.ThrowsAsync<IOException>(() => next.WaitAsync(TimeSpan.FromSeconds(30)));
        Assert.Equal("source lost", error.Message);
        Assert.True(factory.Worker.Disposed);
        Assert.Empty(consumer.Posted);
    }
    [Fact]
    public void LocalOptionsRejectRelativeExecutableAndModelPaths()
    { Assert.Throws<ArgumentException>(() => new PlayerSpeechWorkerOptions("python", "/worker", "/en", "/de", "/manifest")); }
    private static async IAsyncEnumerable<SpeechAudioChunk> Input([EnumeratorCancellation] CancellationToken token = default)
    { await Task.Yield(); token.ThrowIfCancellationRequested(); yield return new(16000, [0.25f]); }
    private static async IAsyncEnumerable<SpeechAudioChunk> FailedInput(Task failure, [EnumeratorCancellation] CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); await failure.ConfigureAwait(false); yield break; }
    private sealed class HeldContext : SynchronizationContext
    {
        internal readonly ConcurrentQueue<SendOrPostCallback> Posted = new();
        public override void Post(SendOrPostCallback d, object? state) => Posted.Enqueue(d);
    }
    private sealed class Factory : IPlayerSpeechWorkerFactory, IAsyncDisposable
    {
        internal Worker? Worker;
        internal string? CaptureMessage;
        internal int Rate = 16000; internal bool FinishCapture;
        internal bool Microphone; internal SpeechLanguage Language; internal string? Error; internal bool WrongSequence;
        public async ValueTask DisposeAsync() { if (Worker is not null) { await Worker.DisposeAsync(); } }
        public ValueTask<IPlayerSpeechWorker> StartAsync(bool microphone, SpeechLanguage language, CancellationToken token)
        { token.ThrowIfCancellationRequested(); Microphone = microphone; Language = language; Worker = new(this); return ValueTask.FromResult<IPlayerSpeechWorker>(Worker); }
    }
    private sealed class Worker : IPlayerSpeechWorker
    {
        private readonly Factory factory;
        private readonly Channel<string> output = Channel.CreateUnbounded<string>();
        internal bool Disposed;
        internal int Receives;
        internal List<JsonElement> Requests = [];
        internal Worker(Factory factory) { this.factory = factory; output.Writer.TryWrite("{\"type\":\"ready\"}"); }
        public ValueTask SendAsync(string message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); using JsonDocument document = JsonDocument.Parse(message);
            JsonElement value = document.RootElement.Clone(); Requests.Add(value);
            switch (value.GetProperty("type").GetString())
            {
                case "start":
                    if (factory.CaptureMessage is not null) { output.Writer.TryWrite(factory.CaptureMessage); break; }
                    if (factory.Error is not null) { output.Writer.TryWrite(JsonSerializer.Serialize(new { type = "error", code = factory.Error })); break; }
                    byte[] bytes = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(bytes, 0.25f);
                    output.Writer.TryWrite(JsonSerializer.Serialize(new { type = "audio", sequence = 0, samples = 1, sampleRate = factory.Rate, pcm = Convert.ToBase64String(bytes) }));
                    if (factory.FinishCapture) { output.Writer.TryWrite("{\"type\":\"finished\"}"); }
                    break;
                case "audio":
                    output.Writer.TryWrite(JsonSerializer.Serialize(new { type = "event", sequence = factory.WrongSequence ? 2 : 0, kind = "partial", utterance = "1", text = "Do not" })); break;
                case "finish":
                    output.Writer.TryWrite("{\"type\":\"event\",\"sequence\":1,\"kind\":\"final\",\"utterance\":\"1\",\"text\":\"Do not take it\"}");
                    output.Writer.TryWrite("{\"type\":\"finished\"}"); break;
            }
            return ValueTask.CompletedTask;
        }
        public async ValueTask<string?> ReceiveAsync(CancellationToken token)
        { Interlocked.Increment(ref Receives); return await output.Reader.ReadAsync(token).ConfigureAwait(false); }
        public ValueTask DisposeAsync() { Disposed = true; output.Writer.TryComplete(); return ValueTask.CompletedTask; }
    }
}
