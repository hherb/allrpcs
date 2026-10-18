using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class StructuredDecisionProvidersTests
{
    internal sealed record Request(string Id);
    internal sealed record Decision(Request Request);

    internal static readonly StructuredDecisionDescriptor<Request, Decision> Descriptor = new(
        "test_decision", "trusted instructions", request => request.Id,
        (writer, _) => { writer.WriteStartObject(); writer.WriteString("type", "object"); writer.WriteEndObject(); },
        (request, text) => text == "{}" ? new(request) : throw new ArgumentException("private decision"),
        128, 16, requireLifecycle: true, allowMetadataAfterTextDone: true);

    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public async Task ReturnsExactRequestOnlyAfterSuccessAndReusesCallerClient(string providerName)
    {
        CharacterModelProvider provider = CharacterModelProvider.Parse(providerName);
        Handler handler = new(Events(providerName));
        using HttpClient http = new(handler);
        IStructuredDecisionClient<Request, Decision> client = StructuredDecisionClientFactory.Create(
            http, new(provider, "model-test"), Descriptor, new Credentials(), new Credentials());
        Request request = new("trusted-context");

        Assert.Same(request, (await client.DecideAsync(request, default)).Request);
        Assert.Same(request, (await client.DecideAsync(request, default)).Request);
        Assert.Equal(2, handler.Count);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal("model-test", body.RootElement.GetProperty("model").GetString());
        Assert.Equal(providerName == "openai" ? "/v1/responses" : providerName == "anthropic"
            ? "/v1/messages" : "/api/chat", handler.Uri!.AbsolutePath);
        Assert.Equal(providerName == "openai" ? "Bearer secret-key" : null, handler.Authorization);
        Assert.Equal(providerName == "anthropic" ? "secret-key" : null, handler.ApiKey);
        Assert.Equal(providerName == "anthropic" ? "2023-06-01" : null, handler.Version);
    }

    [Theory]
    [InlineData("anthropic", "max_tokens")]
    [InlineData("anthropic", "refusal")]
    [InlineData("ollama", "length")]
    public async Task RejectsTruncationAndRefusalEvenWithValidDecisionText(string provider, string reason)
    {
        string events = Events(provider).Replace(provider == "ollama" ? "\"stop\"" : "\"end_turn\"",
            JsonSerializer.Serialize(reason), StringComparison.Ordinal);
        using HttpClient http = new(new Handler(events));
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Parse(provider), "model-test"), Descriptor,
            new Credentials(), new Credentials());
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await client.DecideAsync(new("id"), default));
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public async Task RedactsMalformedInputAndHonorsPreCancellation(string provider)
    {
        Handler handler = new("{\"secret\":\"private model data\"}\n\n");
        using HttpClient http = new(handler);
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Parse(provider), "model-test"), Descriptor,
            new Credentials(), new Credentials());
        var error = await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await client.DecideAsync(new("id"), default));
        Assert.DoesNotContain("private", error.ToString());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await client.DecideAsync(new("id"), cancellation.Token));
        Assert.Equal(1, handler.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectsAnUnavailableAnthropicCredentialBeforeAnyRequest(bool missing)
    {
        Handler handler = new(Events("anthropic"));
        using HttpClient http = new(handler);
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Anthropic, "model-test"), Descriptor,
            anthropicCredentials: new UnavailableAnthropicCredentials(missing));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await client.DecideAsync(new("id"), default));
        Assert.Equal(0, handler.Count);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("ollama")]
    public async Task KeepsUnsupportedMediaTypeDistinctFromInvalidStreams(string provider)
    {
        using HttpClient wrongType = new(new Handler(Events(provider), "text/html"));
        using HttpClient invalidStream = new(new Handler("{\"secret\":\"private\"}\n\n"));
        CharacterModelSettings settings = new(CharacterModelProvider.Parse(provider), "model-test");
        var mediaError = await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await StructuredDecisionClientFactory.Create(wrongType, settings, Descriptor, new Credentials(), new Credentials())
                .DecideAsync(new("id"), default));
        var streamError = await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await StructuredDecisionClientFactory.Create(invalidStream, settings, Descriptor, new Credentials(), new Credentials())
                .DecideAsync(new("id"), default));
        Assert.Contains("media type", mediaError.Message, StringComparison.Ordinal);
        Assert.NotEqual(mediaError.Message, streamError.Message);
    }

    [Fact]
    public async Task AcceptsRecordedAnthropicFramingWithLeadingThinkingAndSplitText()
    {
        using HttpClient http = new(new Handler(RecordedAnthropicEvents("{", "", "}")));
        var client = StructuredDecisionClientFactory.Create(http,
            new(CharacterModelProvider.Anthropic, "model-test"), Descriptor, anthropicCredentials: new Credentials());
        Request request = new("id");
        Assert.Same(request, (await client.DecideAsync(request, default)).Request);
    }

    /// <summary>Mirrors the live Messages stream: event lines, ping, usage, a thinking block and text deltas.</summary>
    internal static string RecordedAnthropicEvents(params string[] fragments) =>
        Named("message_start", new
        {
            type = "message_start",
            message = new
            {
                id = "msg_test",
                type = "message",
                role = "assistant",
                model = "model-test",
                content = Array.Empty<object>(),
                stop_reason = (string?)null,
                stop_sequence = (string?)null,
                usage = new { input_tokens = 12, output_tokens = 1 },
            },
        })
        + Named("ping", new { type = "ping" })
        + Named("content_block_start", new { type = "content_block_start", index = 0, content_block = new { type = "thinking", thinking = "", signature = "" } })
        + Named("content_block_delta", new { type = "content_block_delta", index = 0, delta = new { type = "thinking_delta", thinking = "" } })
        + Named("content_block_delta", new { type = "content_block_delta", index = 0, delta = new { type = "signature_delta", signature = "sig" } })
        + Named("content_block_stop", new { type = "content_block_stop", index = 0 })
        + Named("content_block_start", new { type = "content_block_start", index = 1, content_block = new { type = "text", text = "" } })
        + string.Concat(fragments.Select(fragment => Named("content_block_delta",
            new { type = "content_block_delta", index = 1, delta = new { type = "text_delta", text = fragment } })))
        + Named("content_block_stop", new { type = "content_block_stop", index = 1 })
        + Named("message_delta", new { type = "message_delta", delta = new { stop_reason = "end_turn", stop_sequence = (string?)null }, usage = new { output_tokens = 9 } })
        + Named("message_stop", new { type = "message_stop" });

    private static string Named(string name, object value) => "event: " + name + "\n" + Frame(value);

    internal static string Events(string provider) => provider switch
    {
        "openai" => Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.done", text = "{}" })
            + Frame(new { type = "response.completed" }),
        "anthropic" => Frame(new { type = "message_start", message = new { content = Array.Empty<object>(), stop_reason = (string?)null } })
            + Frame(new { type = "content_block_start", index = 0, content_block = new { type = "text", text = "" } })
            + Frame(new { type = "content_block_delta", index = 0, delta = new { type = "text_delta", text = "{}" } })
            + Frame(new { type = "content_block_stop", index = 0 })
            + Frame(new { type = "message_delta", delta = new { stop_reason = "end_turn" } })
            + Frame(new { type = "message_stop" }),
        _ => JsonSerializer.Serialize(new { message = new { role = "assistant", content = "{}" }, done = false }) + "\n"
            + JsonSerializer.Serialize(new { message = new { role = "assistant", content = "" }, done = true, done_reason = "stop" }) + "\n",
    };

    private static string Frame(object value) => "data: " + JsonSerializer.Serialize(value) + "\n\n";

    internal sealed class Credentials : IOpenAiCredentialSource, IAnthropicCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken token) =>
            ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("secret-key"));
        ValueTask<AnthropicCredentialLoadResult> IAnthropicCredentialSource.LoadApiKeyAsync(CancellationToken token) =>
            ValueTask.FromResult(AnthropicCredentialLoadResult.Loaded("secret-key"));
    }

    private sealed class UnavailableAnthropicCredentials(bool missing) : IAnthropicCredentialSource
    {
        public ValueTask<AnthropicCredentialLoadResult> LoadApiKeyAsync(CancellationToken token) =>
            ValueTask.FromResult(missing ? AnthropicCredentialLoadResult.Missing() : AnthropicCredentialLoadResult.Failed());
    }

    private sealed class Handler(string events, string? mediaType = null) : HttpMessageHandler
    {
        internal int Count { get; private set; }
        internal string? Body { get; private set; }
        internal Uri? Uri { get; private set; }
        internal string? Authorization { get; private set; }
        internal string? ApiKey { get; private set; }
        internal string? Version { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Count++;
            Uri = request.RequestUri;
            Authorization = request.Headers.Authorization?.ToString();
            ApiKey = request.Headers.TryGetValues("x-api-key", out var keys) ? keys.Single() : null;
            Version = request.Headers.TryGetValues("anthropic-version", out var versions) ? versions.Single() : null;
            Body = await request.Content!.ReadAsStringAsync(token);
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(events, Encoding.UTF8,
                mediaType ?? (Uri!.AbsolutePath == "/api/chat" ? "application/x-ndjson" : "text/event-stream"))
            };
        }
    }
}
