using System.Buffers.Binary;
using System.Text.Json;
using System.Threading.Channels;
using Allrpcs.Application.Speech;
using Allrpcs.Infrastructure.Speech;

namespace Allrpcs.Infrastructure.Tests.Speech;

public sealed class PocketSpeechSynthesizerTests
{
    [Fact]
    public async Task StreamsExactTextAndReusesWorkerAcrossLanguages()
    {
        FakeFactory factory = new(Normal);
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        const string text = "  Bleib bei mir.\nEs gibt noch Hoffnung!  ";
        List<SpeechAudioChunk> first = await Collect(synthesizer, text, SpeechLanguage.German);
        await Collect(synthesizer, "Stay close.", SpeechLanguage.English);
        Assert.Single(first);
        Assert.Equal(0.25f, first[0].Samples[0]);
        Assert.Single(factory.Workers);
        Assert.Equal(text, factory.Workers[0].Requests[0].GetProperty("text").GetString());
        Assert.Equal("de", factory.Workers[0].Requests[0].GetProperty("language").GetString());
        Assert.Equal("en", factory.Workers[0].Requests[1].GetProperty("language").GetString());
    }

    [Fact]
    public async Task RequestErrorIsTerminalAndReusesSameWarmWorkerWithoutCancellation()
    {
        FakeFactory factory = new((worker, request) => worker.Output.Writer.TryWrite(
            JsonSerializer.Serialize(new { type = "error", id = request.GetProperty("id").GetString(), code = "synthesis_failed" })));
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await Assert.ThrowsAsync<SpeechSynthesisException>(() => Collect(synthesizer, "First."));
        FakeWorker original = Assert.Single(factory.Workers);
        Assert.Equal(0, original.Cancellations);
        Assert.False(original.Disposed);
        factory.Handler = Normal;
        Assert.Single(await Collect(synthesizer, "Next."));
        Assert.Same(original, Assert.Single(factory.Workers));
    }

    [Fact]
    public async Task EarlyConsumerExitRetiresBufferedAudioBeforeNextRequest()
    {
        FakeFactory factory = new((worker, request) =>
        {
            Normal(worker, request);
        });
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await using (IAsyncEnumerator<SpeechAudioChunk> stream = synthesizer.SynthesizeAsync(
            Request(), default).GetAsyncEnumerator())
        {
            Assert.True(await stream.MoveNextAsync());
        }
        Assert.Equal(1, factory.Workers[0].Cancellations);
        Assert.Single(await Collect(synthesizer, "Next."));
        Assert.Single(factory.Workers);
    }

    [Fact]
    public async Task CallerCancellationStopsCurrentStreamAndRecovers()
    {
        FakeFactory factory = new((worker, request) =>
        {
            worker.Output.Writer.TryWrite(Audio(request.GetProperty("id").GetString()!));
        });
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        using CancellationTokenSource cancellation = new();
        await using IAsyncEnumerator<SpeechAudioChunk> stream = synthesizer.SynthesizeAsync(
            Request(), cancellation.Token).GetAsyncEnumerator();
        Assert.True(await stream.MoveNextAsync());
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.MoveNextAsync().AsTask());
        Assert.Equal(1, factory.Workers[0].Cancellations);
        factory.Handler = Normal;
        Assert.Single(await Collect(synthesizer, "Next."));
    }

    [Theory]
    [InlineData("error-missing-code")]
    [InlineData("error-wrong-id")]
    [InlineData("transport")]
    [InlineData("missing-id")]
    [InlineData("stale-id")]
    [InlineData("bad-pcm")]
    [InlineData("nonfinite")]
    [InlineData("wrong-sequence")]
    [InlineData("incompatible-ready")]
    [InlineData("closed-output")]
    public async Task RejectsInvalidWorkerDataWithoutReturningPcm(string fault)
    {
        FakeFactory factory = new((worker, request) =>
        {
            string id = request.GetProperty("id").GetString()!;
            if (fault == "transport") { throw new IOException("synthetic broken pipe"); }
            worker.Output.Writer.TryWrite(fault switch
            {
                "error-missing-code" => JsonSerializer.Serialize(new { type = "error", id }),
                "error-wrong-id" => JsonSerializer.Serialize(new { type = "error", id = "stale", code = "synthesis_failed" }),
                "missing-id" => "{\"type\":\"audio\"}",
                "stale-id" => Audio("obsolete"),
                "bad-pcm" => JsonSerializer.Serialize(new
                {
                    type = "audio",
                    id,
                    sequence = 0,
                    sampleRate = 24000,
                    samples = 2,
                    pcm = "!"
                }),
                "nonfinite" => Audio(id, sample: float.NaN),
                "wrong-sequence" => Audio(id, sequence: 1),
                _ => Audio(id),
            });
            worker.Finish(id);
        })
        { Ready = fault == "incompatible-ready" ? "{\"type\":\"ready\",\"protocol\":99}" : Ready };
        if (fault == "closed-output")
        {
            factory.Handler = (worker, _) => worker.Output.Writer.TryComplete();
        }
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await Assert.ThrowsAsync<SpeechSynthesisException>(() => Collect(synthesizer, "Do not take the herb."));
        Assert.True(factory.Workers[0].Disposed);
    }

    [Fact]
    public async Task PreCancelledRequestStartsNoProcess()
    {
        FakeFactory factory = new(Normal);
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Collect(synthesizer, "Hello", token: cancellation.Token));
        Assert.Empty(factory.Workers);
    }

    [Fact]
    public async Task CancellingQueuedRequestLeavesCurrentUtteranceAndWorkerIntact()
    {
        FakeFactory factory = new(Normal);
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await using IAsyncEnumerator<SpeechAudioChunk> current = synthesizer.SynthesizeAsync(Request(), default)
            .GetAsyncEnumerator();
        Assert.True(await current.MoveNextAsync());
        using CancellationTokenSource queuedCancellation = new();
        Task<List<SpeechAudioChunk>> queued = Collect(synthesizer, "Obsolete queued reply.", token: queuedCancellation.Token);
        Assert.False(queued.IsCompleted);
        queuedCancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
        Assert.False(await current.MoveNextAsync());
        Assert.Single(factory.Workers[0].Requests);
        Assert.Equal(0, factory.Workers[0].Cancellations);
        Assert.Single(await Collect(synthesizer, "Next reply."));
    }

    [Fact]
    public async Task UnacknowledgedCancellationReplacesWorkerBeforeNextUtterance()
    {
        FakeFactory factory = new((worker, request) =>
            worker.Output.Writer.TryWrite(Audio(request.GetProperty("id").GetString()!)))
        {
            AcknowledgeCancellation = false,
        };
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await using (IAsyncEnumerator<SpeechAudioChunk> current = synthesizer.SynthesizeAsync(Request(), default)
            .GetAsyncEnumerator())
        {
            Assert.True(await current.MoveNextAsync());
        }
        Assert.True(factory.Workers[0].Disposed);
        factory.Handler = Normal;
        Assert.Single(await Collect(synthesizer, "Recovered reply."));
        Assert.Equal(2, factory.Workers.Count);
    }

    [Theory]
    [InlineData("error")]
    [InlineData("finished")]
    public async Task MalformedCancellationAcknowledgementDiscardsWorker(string kind)
    {
        FakeFactory factory = new((worker, request) =>
            worker.Output.Writer.TryWrite(Audio(request.GetProperty("id").GetString()!)))
        { CancellationReply = id => JsonSerializer.Serialize(new { type = kind, id }) };
        await using PocketSpeechSynthesizer synthesizer = new(factory);
        await using (IAsyncEnumerator<SpeechAudioChunk> current = synthesizer.SynthesizeAsync(Request(), default).GetAsyncEnumerator())
        { Assert.True(await current.MoveNextAsync()); }
        Assert.True(factory.Workers[0].Disposed);
    }

    [Fact]
    public async Task MissingRuntimeReportsUnavailableWithoutExposingLocalPaths()
    {
        string missing = Path.Combine(Path.GetTempPath(), "missing-pocket-runtime-" + Guid.NewGuid().ToString("N"));
        PocketSpeechWorkerProcessFactory factory = new(new(missing, missing + ".py", missing + "-assets", missing + ".json"));
        SpeechSynthesisException exception = await Assert.ThrowsAsync<SpeechSynthesisException>(
            () => factory.StartAsync(default).AsTask());
        Assert.DoesNotContain(missing, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DisposingServiceCancelsBlockedReceiveAndClosesWorker()
    {
        FakeFactory factory = new((worker, request) =>
            worker.Output.Writer.TryWrite(Audio(request.GetProperty("id").GetString()!)));
        PocketSpeechSynthesizer synthesizer = new(factory);
        await using IAsyncEnumerator<SpeechAudioChunk> stream = synthesizer.SynthesizeAsync(Request(), default)
            .GetAsyncEnumerator();
        Assert.True(await stream.MoveNextAsync());
        Task<bool> waiting = stream.MoveNextAsync().AsTask();
        Task disposal = synthesizer.DisposeAsync().AsTask();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        await disposal;
        Assert.True(factory.Workers[0].Disposed);
    }

    private const string Ready = "{\"type\":\"ready\",\"protocol\":1}";
    private static SpeechSynthesisRequest Request() => new("Stay close.", SpeechLanguage.English, "default");

    private static async Task<List<SpeechAudioChunk>> Collect(
        PocketSpeechSynthesizer synthesizer, string text, SpeechLanguage language = SpeechLanguage.English,
        CancellationToken token = default)
    {
        List<SpeechAudioChunk> chunks = [];
        await foreach (SpeechAudioChunk chunk in synthesizer.SynthesizeAsync(new(text, language, "default"), token))
        {
            chunks.Add(chunk);
        }
        return chunks;
    }

    private static void Normal(FakeWorker worker, JsonElement request)
    {
        string id = request.GetProperty("id").GetString()!;
        worker.Output.Writer.TryWrite(Audio(id));
        worker.Finish(id);
    }

    private static string Audio(string id, int sequence = 0, float sample = 0.25f)
    {
        byte[] pcm = new byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(pcm, sample);
        return JsonSerializer.Serialize(new
        {
            type = "audio",
            id,
            sequence,
            sampleRate = 24000,
            samples = 1,
            pcm = Convert.ToBase64String(pcm)
        });
    }

    private static string Finished(string id) => JsonSerializer.Serialize(new { type = "finished", id, status = "completed" });

    private sealed class FakeFactory(Action<FakeWorker, JsonElement> handler) : IPocketSpeechWorkerFactory
    {
        public List<FakeWorker> Workers { get; } = [];
        public Action<FakeWorker, JsonElement> Handler { get; set; } = handler;
        public string Ready { get; set; } = PocketSpeechSynthesizerTests.Ready;
        public Func<string, string>? CancellationReply { get; init; }
        public bool AcknowledgeCancellation { get; init; } = true;

        public ValueTask<IPocketSpeechWorker> StartAsync(CancellationToken cancellationToken)
        {
            FakeWorker worker = new(this);
            worker.Output.Writer.TryWrite(Ready);
            Workers.Add(worker);
            return ValueTask.FromResult<IPocketSpeechWorker>(worker);
        }
    }

    private sealed class FakeWorker(FakeFactory factory) : IPocketSpeechWorker
    {
        public Channel<string> Output { get; } = Channel.CreateUnbounded<string>();
        public List<JsonElement> Requests { get; } = [];
        public int Cancellations { get; private set; }
        public bool Disposed { get; private set; }
        private bool _finished;

        public void Finish(string id)
        {
            _finished = true;
            Output.Writer.TryWrite(Finished(id));
        }

        public ValueTask SendAsync(string message, CancellationToken cancellationToken)
        {
            using JsonDocument document = JsonDocument.Parse(message);
            JsonElement root = document.RootElement;
            if (root.GetProperty("type").GetString() == "cancel")
            {
                Cancellations++;
                if (!_finished && factory.AcknowledgeCancellation)
                {
                    string id = root.GetProperty("id").GetString()!;
                    if (factory.CancellationReply is { } reply) { Output.Writer.TryWrite(reply(id)); }
                    else { Finish(id); }
                }
            }
            else
            {
                Requests.Add(root.Clone());
                _finished = false;
                factory.Handler(this, root);
            }
            return ValueTask.CompletedTask;
        }

        public async ValueTask<string?> ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Output.Reader.ReadAsync(cancellationToken);
            }
            catch (ChannelClosedException)
            {
                return null;
            }
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            Output.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }
}
