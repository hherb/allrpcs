using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class AnthropicCharacterModelTests
{
    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,\"deliveryMode\":null,"
        + "\"immediateIntention\":{\"type\":\"wait\",\"targetObjectId\":null},"
        + "\"commitments\":[],\"stateProposals\":[]}";

    public static TheoryData<HttpStatusCode, CharacterModelFailureCode> FailureStatuses => new()
    {
        { HttpStatusCode.Unauthorized, CharacterModelFailureCode.Authentication },
        { HttpStatusCode.Forbidden, CharacterModelFailureCode.Authentication },
        { HttpStatusCode.TooManyRequests, CharacterModelFailureCode.RateLimited },
        { HttpStatusCode.RequestTimeout, CharacterModelFailureCode.Unavailable },
        { HttpStatusCode.BadGateway, CharacterModelFailureCode.Unavailable },
        { HttpStatusCode.ServiceUnavailable, CharacterModelFailureCode.Unavailable },
        { HttpStatusCode.BadRequest, CharacterModelFailureCode.ProviderRejected },
    };

    [Fact]
    public async Task PostsOneAuthenticatedStatelessStreamingRequestAndCompletes()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        CharacterModelStreamUpdate[] updates = await CollectAsync(model, request);

        CharacterModelStreamTermination result =
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Same(request.TurnRequest, result.Response?.Request);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.anthropic.com/v1/messages", handler.RequestUri?.AbsoluteUri);
        Assert.Null(handler.Authorization);
        Assert.Equal("test-secret", handler.ApiKey);
        Assert.Equal("2023-06-01", handler.AnthropicVersion);
        Assert.Equal("text/event-stream", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("model-x", body.RootElement.GetProperty("model").GetString());
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.True(handler.CanSendAgain);
    }

    [Fact]
    public async Task RecordsBoundedRedactedCompletionDiagnostics()
    {
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        RecordingDiagnosticSink sink = new();
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings(),
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(325)),
            sink);

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        CharacterModelGenerationDiagnostic diagnostic = Assert.IsType<
            CharacterModelGenerationDiagnostic>(sink.Diagnostic);
        Assert.Same(CharacterModelProvider.Anthropic, diagnostic.Provider);
        Assert.Equal(TimeSpan.FromMilliseconds(325), diagnostic.Elapsed);
        Assert.Equal(11, diagnostic.InputTokenCount);
        Assert.Equal(7, diagnostic.OutputTokenCount);
        Assert.Equal(18, diagnostic.TotalTokenCount);
        Assert.DoesNotContain("secret", diagnostic.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DiagnosticSinkFailureDoesNotChangeCompletedGeneration()
    {
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings(),
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(5)),
            new ThrowingDiagnosticSink());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Null(result.Failure);
    }

    [Theory]
    [MemberData(nameof(FailureStatuses))]
    public async Task MapsHttpStatusWithoutReadingOrRetainingTheErrorBody(
        HttpStatusCode status,
        CharacterModelFailureCode expectedCode)
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(status)
        {
            Content = new ThrowingContent("secret provider response"),
        });
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(expectedCode, result.Failure?.Code);
        Assert.Null(result.Response);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" key with spaces ")]
    [InlineData("key\0value")]
    public async Task RejectsUnavailableOrInvalidCredentialsWithoutSending(string? apiKey)
    {
        CapturingHandler handler = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(client, new StubCredentialSource(apiKey), Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Authentication, result.Failure?.Code);
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task RejectsExcessiveCredentialWithoutSending()
    {
        string apiKey = new('k', AnthropicCharacterModel.MaximumApiKeyLength + 1);
        CapturingHandler handler = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(client, new StubCredentialSource(apiKey), Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Authentication, result.Failure?.Code);
        Assert.Equal(0, handler.SendCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    public async Task RequiresAnEventStreamSuccessContentType(string? mediaType)
    {
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(), mediaType));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
    }

    [Fact]
    public async Task MapsHttpTransportFailureWithoutRetainingDetails()
    {
        CapturingHandler handler = new(_ => throw new HttpRequestException("secret network detail"));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCancellationAndRejectsInvalidDependencies()
    {
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(model, CreateRequest(), cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() => CollectAsync(model, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new AnthropicCharacterModel(null!, new StubCredentialSource("key"), Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new AnthropicCharacterModel(client, null!, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new AnthropicCharacterModel(client, new StubCredentialSource("key"), null!));
        Assert.Throws<ArgumentException>(() => new AnthropicCharacterModel(
            client,
            new StubCredentialSource("key"),
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "model-x")));
        Assert.Throws<ArgumentNullException>(() => new AnthropicCharacterModel(
            client,
            new StubCredentialSource("key"),
            Settings(),
            null!,
            new RecordingDiagnosticSink()));
        Assert.Throws<ArgumentNullException>(() => new AnthropicCharacterModel(
            client,
            new StubCredentialSource("key"),
            Settings(),
            TimeProvider.System,
            null!));
    }

    private static CharacterModelSettings Settings() =>
        new(CharacterModelProvider.Anthropic, "model-x");

    private static async Task<CharacterModelStreamUpdate[]> CollectAsync(
        AnthropicCharacterModel model,
        CharacterModelGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        List<CharacterModelStreamUpdate> updates = [];
        await foreach (CharacterModelStreamUpdate update in
                       model.StreamResponseAsync(request, cancellationToken))
        {
            updates.Add(update);
        }

        return updates.ToArray();
    }

    private static HttpResponseMessage EventStreamResponse(
        string body,
        string? mediaType = "text/event-stream")
    {
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        response.Content.Headers.ContentType = mediaType is null
            ? null
            : new MediaTypeHeaderValue(mediaType);
        return response;
    }

    private static string CompletedEvents() =>
        Frame("message_start", new
        {
            type = "message_start",
            message = new
            {
                content = Array.Empty<object>(),
                stop_reason = (string?)null,
                usage = new { input_tokens = 11 },
            },
        })
        + Frame("content_block_start", new
        {
            type = "content_block_start",
            index = 0,
            content_block = new { type = "text", text = string.Empty },
        })
        + Frame("content_block_delta", new
        {
            type = "content_block_delta",
            index = 0,
            delta = new { type = "text_delta", text = ResponseDocument },
        })
        + Frame("content_block_stop", new { type = "content_block_stop", index = 0 })
        + Frame("message_delta", new
        {
            type = "message_delta",
            delta = new { stop_reason = "end_turn" },
            usage = new { output_tokens = 7 },
        })
        + Frame("message_stop", new { type = "message_stop" });

    private static string Frame(string eventName, object semanticEvent) =>
        $"event: {eventName}\ndata: {JsonSerializer.Serialize(semanticEvent)}\n\n";

    private static CharacterModelGenerationRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        string playerText = "Please inspect the herb.";
        DialogueUtterance utterance = new(seekerId, playerText);
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(seekerId, [healerId]));
        CharacterTurnRequest turnRequest = new(
            healerId,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
        return new CharacterModelGenerationRequest(
            turnRequest,
            new CharacterModelContextSnapshot(
                [
                    new CharacterModelContextEntry(
                        CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                        playerText,
                        DialogueDeliveryMode.OrdinarySpeech),
                ]));
    }

    private sealed class StubCredentialSource(string? apiKey) : IAnthropicCredentialSource
    {
        public ValueTask<AnthropicCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(apiKey is null
                ? AnthropicCredentialLoadResult.Missing()
                : AnthropicCredentialPolicy.IsValid(apiKey)
                    ? AnthropicCredentialLoadResult.Loaded(apiKey)
                    : AnthropicCredentialLoadResult.Failed());
        }
    }

    private sealed class RecordingDiagnosticSink : ICharacterModelGenerationDiagnosticSink
    {
        public CharacterModelGenerationDiagnostic? Diagnostic { get; private set; }

        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            Diagnostic = diagnostic;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingDiagnosticSink : ICharacterModelGenerationDiagnosticSink
    {
        public ValueTask RecordAsync(
            CharacterModelGenerationDiagnostic diagnostic,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("private sink detail");
    }

    private sealed class SequenceTimeProvider(params TimeSpan[] timestamps) : TimeProvider
    {
        private readonly Queue<long> values = new(
            timestamps.Select(timestamp => timestamp.Ticks));

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => values.Dequeue();
    }

    private sealed class CapturingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public AuthenticationHeaderValue? Authorization { get; private set; }

        public string? ApiKey { get; private set; }

        public string? AnthropicVersion { get; private set; }

        public string? Accept { get; private set; }

        public string? ContentType { get; private set; }

        public string? Body { get; private set; }

        public int SendCount { get; private set; }

        public bool CanSendAgain => !Disposed;

        private bool Disposed { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            ApiKey = GetSingleHeader(request, "x-api-key");
            AnthropicVersion = GetSingleHeader(request, "anthropic-version");
            Accept = request.Headers.Accept.SingleOrDefault()?.MediaType;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }

        private static string? GetSingleHeader(HttpRequestMessage request, string name) =>
            request.Headers.TryGetValues(name, out IEnumerable<string>? values)
                ? values.SingleOrDefault()
                : null;
    }

    private sealed class ThrowingContent(string message) : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context) =>
            Task.FromException(new IOException(message));

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
