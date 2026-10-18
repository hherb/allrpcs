using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
    private static readonly string[] CaseValues4 = ["anthropic", "ollama"];
    private static readonly string[] CaseValues5 = ["invalid-json", "oversized-event", "oversized-text", "wrong-shape", "error", "wrong-role-or-index", "wrong-order"];

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

    public static IEnumerable<object[]> MalformedCases =>
        from provider in CaseValues4
        from defect in CaseValues5
        select new object[] { provider, defect };

    [Theory]
    [MemberData(nameof(MalformedCases))]
    public void RejectsMalformedEventsAndFencesEveryFailure(string provider, string defect)
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
            case "wrong-order":
                if (provider == "anthropic") documents.Reverse();
                else documents[0] = documents[0].Replace("false", "\"false\"", StringComparison.Ordinal);
                break;
        }
        var apply = Accumulator(provider, new("id"));
        var error = Assert.Throws<InvalidDataException>(() =>
        {
            foreach (string document in documents) apply(document);
        });
        Assert.DoesNotContain("private", error.ToString());
        Assert.Throws<InvalidOperationException>(() => apply("{}"));
    }

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
