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
        128, 16);

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

    private sealed class Handler(string events) : HttpMessageHandler
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
                Uri!.AbsolutePath == "/api/chat" ? "application/x-ndjson" : "text/event-stream")
            };
        }
    }
}
