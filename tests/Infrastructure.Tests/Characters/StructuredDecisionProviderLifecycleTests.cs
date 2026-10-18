using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Tests.Dialogue;
using Decision = Allrpcs.Infrastructure.Tests.Characters.StructuredDecisionProvidersTests.Decision;
using Request = Allrpcs.Infrastructure.Tests.Characters.StructuredDecisionProvidersTests.Request;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class StructuredDecisionProviderLifecycleTests
{
    private static readonly string[] CaseValues1 = ["anthropic", "ollama"];
    private static readonly bool[] CaseValues2 = [false, true];
    private static readonly bool[] CaseValues3 = [false, true];

    public static IEnumerable<object[]> DeadlineCases =>
        from provider in CaseValues1
        from headers in CaseValues2
        from cancel in CaseValues3
        select new object[] { provider, headers, cancel };

    [Theory]
    [MemberData(nameof(DeadlineCases))]
    public async Task BoundsHeadersAndBodyDisposesResponsesAndPreservesCallerCancellation(
        string provider, bool headers, bool cancel)
    {
        using ProviderDeadlineTestTransport transport = new()
        {
            StallHeaders = headers,
            MediaType = provider == "ollama" ? "application/x-ndjson" : "text/event-stream",
        };
        using HttpClient http = new(transport);
        using CancellationTokenSource caller = new();
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Parse(provider), "model-test"), StructuredDecisionProvidersTests.Descriptor,
            anthropicCredentials: new StructuredDecisionProvidersTests.Credentials(), timeoutOptions: transport.Options);
        Task<Decision> pending = client.DecideAsync(new("id"), caller.Token).AsTask();
        await transport.ExpireAsync(cancel, caller);
        Exception? failure = await Record.ExceptionAsync(async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)));
        if (cancel) Assert.IsAssignableFrom<OperationCanceledException>(failure);
        else Assert.IsType<IOException>(failure);
        if (!headers) Assert.True(transport.Body.Disposed);
        Assert.False(transport.Disposed);
    }

    [Theory]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public async Task CompletesBeforeUnreadTailDisposesResponseAndLeavesClientUsable(string provider)
    {
        List<TrackingStream> bodies = [];
        using HttpClient http = new(new Handler(() =>
        {
            var body = new TrackingStream(StructuredDecisionProvidersTests.Events(provider));
            bodies.Add(body);
            return Response(body, provider);
        }));
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Parse(provider), "model-test"), StructuredDecisionProvidersTests.Descriptor,
            anthropicCredentials: new StructuredDecisionProvidersTests.Credentials());
        Request request = new("id");
        Assert.Same(request, (await client.DecideAsync(request, default)).Request);
        Assert.Same(request, (await client.DecideAsync(request, default)).Request);
        Assert.All(bodies, body => { Assert.True(body.Disposed); Assert.False(body.ReadPastEnd); });
    }

    [Theory]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public async Task RejectsIncompleteStreamsAndSanitizesReadErrors(string provider)
    {
        var documents = Documents(provider);
        string incomplete = Encode(provider, documents[..^1]);
        using var direct = new TrackingStream(incomplete, failAtEnd: false);
        var complete = Completion(provider, new("id"));
        await Assert.ThrowsAsync<InvalidDataException>(() => complete(direct));
        Assert.False(direct.Disposed);

        using var body = new TrackingStream(incomplete);
        using HttpClient http = new(new Handler(() => Response(body, provider)));
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Parse(provider), "model-test"), StructuredDecisionProvidersTests.Descriptor,
            anthropicCredentials: new StructuredDecisionProvidersTests.Credentials());
        var error = await Assert.ThrowsAsync<IOException>(async () => await client.DecideAsync(new("id"), default));
        Assert.DoesNotContain("private", error.ToString());
        Assert.True(body.Disposed);
    }

    // Each defect names the zero-based event that must fail; the fixture events are, for Anthropic,
    // message_start, content_block_start, delta, content_block_stop, message_delta, message_stop and,
    // for Ollama, one content record and the done record.
    public static TheoryData<string, string, int> MalformedCases => new()
    {
        { "anthropic", "invalid-json", 0 }, { "ollama", "invalid-json", 0 },
        { "anthropic", "oversized-event", 0 }, { "ollama", "oversized-event", 0 },
        { "anthropic", "oversized-text", 2 }, { "ollama", "oversized-text", 0 },
        { "anthropic", "wrong-shape", 0 }, { "ollama", "wrong-shape", 0 },
        { "anthropic", "error", 0 }, { "ollama", "error", 0 },
        { "anthropic", "wrong-role-or-index", 1 }, { "ollama", "wrong-role-or-index", 0 },
        { "anthropic", "wrong-type", 1 }, { "ollama", "wrong-type", 0 },
        { "anthropic", "missing-stop", 4 }, { "ollama", "missing-stop", 1 },
        { "anthropic", "wrong-order", 0 },
        { "anthropic", "thinking-after-text", 4 },
        { "anthropic", "second-text-block", 4 },
    };

    [Theory]
    [MemberData(nameof(MalformedCases))]
    public void RejectsMalformedEventsAndFencesEveryFailure(string provider, string defect, int failingEvent)
    {
        var documents = Documents(provider).ToList();
        switch (defect)
        {
            case "invalid-json": documents[0] = "private invalid json"; break;
            case "oversized-event": documents[0] = new string('x', 64_001); break;
            case "oversized-text":
                documents = documents.Select(value => value.Replace("{}", new string('x', 17), StringComparison.Ordinal)).ToList();
                break;
            case "wrong-shape": documents[0] = "[]"; break;
            case "error": documents.Insert(0, "{\"type\":\"error\",\"error\":\"private\"}"); break;
            case "wrong-role-or-index":
                documents = documents.Select(value => provider == "anthropic"
                    ? value.Replace("\"index\":0", "\"index\":1", StringComparison.Ordinal)
                    : value.Replace("assistant", "user", StringComparison.Ordinal)).ToList();
                break;
            case "wrong-type":
                documents = documents.Select(value => provider == "anthropic"
                    ? value.Replace("\"index\":0", "\"index\":\"0\"", StringComparison.Ordinal)
                    : value.Replace("\"done\":false", "\"done\":\"false\"", StringComparison.Ordinal)).ToList();
                break;
            case "missing-stop":
                if (provider == "anthropic") documents.RemoveAt(4);
                else documents[1] = documents[1].Replace(",\"done_reason\":\"stop\"", "", StringComparison.Ordinal);
                break;
            case "wrong-order": documents.Reverse(); break;
            case "thinking-after-text":
                documents.Insert(4, "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"thinking\",\"thinking\":\"\"}}");
                break;
            case "second-text-block":
                documents.Insert(4, "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}");
                break;
        }
        var apply = Accumulator(provider, new("id"));
        int index = 0;
        var error = Assert.Throws<InvalidDataException>(() =>
        {
            for (; index < documents.Count; index++) apply(documents[index]);
        });
        Assert.Equal(failingEvent, index);
        Assert.DoesNotContain("private", error.ToString());
        Assert.Throws<InvalidOperationException>(() => apply("{}"));
    }

    [Theory]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public void JoinsTextSplitAcrossEventsAndBoundsTheJoinedLength(string provider)
    {
        Request request = new("split");
        var apply = Accumulator(provider, request);
        Decision? decision = null;
        foreach (string document in SplitDocuments(provider, "{", "", "}")) decision = apply(document);
        Assert.Same(request, decision!.Request);

        // Every fragment is within the 16-character bound, but their join is not.
        var bounded = Accumulator(provider, request);
        string[] oversized = SplitDocuments(provider, new string('x', 9), new string('x', 9));
        Assert.Throws<InvalidDataException>(() =>
        {
            foreach (string document in oversized) bounded(document);
        });
    }

    private static string[] SplitDocuments(string provider, params string[] fragments) => provider == "anthropic"
        ? StructuredDecisionProvidersTests.RecordedAnthropicEvents(fragments).Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(frame => frame[(frame.IndexOf("data: ", StringComparison.Ordinal) + 6)..]).ToArray()
        : fragments.Select(fragment => JsonSerializer.Serialize(new { message = new { role = "assistant", content = fragment }, done = false }))
            .Append(JsonSerializer.Serialize(new { message = new { role = "assistant", content = "" }, done = true, done_reason = "stop" }))
            .ToArray();

    [Theory]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public void PublishesOnlyAtTerminalAndRejectsSubsequentEvents(string provider)
    {
        Request request = new("exact");
        var apply = Accumulator(provider, request);
        var documents = Documents(provider);
        foreach (string document in documents[..^1]) Assert.Null(apply(document));
        Assert.Same(request, apply(documents[^1])!.Request);
        Assert.Throws<InvalidOperationException>(() => apply(documents[^1]));
    }

    private static string[] Documents(string provider) => provider == "anthropic"
        ? StructuredDecisionProvidersTests.Events(provider).Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(frame => frame[6..]).ToArray()
        : StructuredDecisionProvidersTests.Events(provider).Split('\n', StringSplitOptions.RemoveEmptyEntries);

    private static string Encode(string provider, IEnumerable<string> documents) => provider == "anthropic"
        ? string.Concat(documents.Select(document => "data: " + document + "\n\n"))
        : string.Concat(documents.Select(document => document + "\n"));

    private static Func<string, Decision?> Accumulator(string provider, Request request) => provider == "anthropic"
        ? new AnthropicStructuredDecisionAccumulator<Request, Decision>(request, StructuredDecisionProvidersTests.Descriptor).Apply
        : new OllamaStructuredDecisionAccumulator<Request, Decision>(request, StructuredDecisionProvidersTests.Descriptor).Apply;

    private static Func<Stream, Task<Decision>> Completion(string provider, Request request) => provider == "anthropic"
        ? stream => new AnthropicStructuredDecisionAccumulator<Request, Decision>(request, StructuredDecisionProvidersTests.Descriptor).CompleteAsync(stream)
        : stream => new OllamaStructuredDecisionAccumulator<Request, Decision>(request, StructuredDecisionProvidersTests.Descriptor).CompleteAsync(stream);

    private static HttpResponseMessage Response(Stream body, string provider)
    {
        HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new StreamContent(body) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(provider == "ollama" ? "application/x-ndjson" : "text/event-stream");
        return response;
    }

    private sealed class Handler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond());
    }

    private sealed class TrackingStream(string payload, bool failAtEnd = true) : MemoryStream(Encoding.UTF8.GetBytes(payload))
    {
        internal bool Disposed { get; private set; }
        internal bool ReadPastEnd { get; private set; }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (Position == Length && failAtEnd)
            {
                ReadPastEnd = true;
                throw new IOException("private read failure");
            }
            return base.ReadAsync(buffer, cancellationToken);
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
