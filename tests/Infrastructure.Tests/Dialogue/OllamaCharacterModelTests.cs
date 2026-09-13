using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private const string ResponseDocument =
        "{\"disposition\":\"accept\",\"speech\":null,"
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
    public async Task PostsOneUnauthenticatedStatelessStreamingRequestAndCompletes()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments()));
        using HttpClient client = new(handler);
        OllamaCharacterModel model = new(client, Settings());

        CharacterModelStreamUpdate[] updates = await CollectAsync(model, request);

        CharacterModelStreamTermination result =
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Same(request.TurnRequest, result.Response?.Request);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("http://127.0.0.1:11434/api/chat", handler.RequestUri?.AbsoluteUri);
        Assert.Null(handler.Authorization);
        Assert.Empty(handler.OtherHeaders);
        Assert.Equal("application/x-ndjson", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("model-x", body.RootElement.GetProperty("model").GetString());
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.True(handler.CanSendAgain);
    }

    [Fact]
    public async Task UsesAnExplicitValidatedLoopbackEndpoint()
    {
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments()));
        using HttpClient client = new(handler);
        OllamaEndpoint endpoint = new(new Uri("http://[::1]:22334/", UriKind.Absolute));
        OllamaCharacterModel model = new(client, Settings(), endpoint);

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Equal("http://[::1]:22334/api/chat", handler.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task RecordsBoundedRedactedCompletionDiagnostics()
    {
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments()));
        using HttpClient client = new(handler);
        RecordingDiagnosticSink sink = new();
        OllamaCharacterModel model = new(
            client,
            Settings(),
            OllamaEndpoint.Default,
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(225)),
            sink);

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        CharacterModelGenerationDiagnostic diagnostic = Assert.IsType<
            CharacterModelGenerationDiagnostic>(sink.Diagnostic);
        Assert.Same(CharacterModelProvider.Ollama, diagnostic.Provider);
        Assert.Equal(TimeSpan.FromMilliseconds(225), diagnostic.Elapsed);
        Assert.Equal(11, diagnostic.InputTokenCount);
        Assert.Equal(7, diagnostic.OutputTokenCount);
        Assert.Equal(18, diagnostic.TotalTokenCount);
        Assert.DoesNotContain("model-x", diagnostic.ToString(), StringComparison.Ordinal);
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
        OllamaCharacterModel model = new(client, Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(expectedCode, result.Failure?.Code);
        Assert.Null(result.Response);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    public async Task RequiresNativeNdjsonSuccessContentType(string? mediaType)
    {
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments(), mediaType));
        using HttpClient client = new(handler);
        OllamaCharacterModel model = new(client, Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
    }

    [Fact]
    public async Task MapsHttpTransportFailureWithoutRetainingDetails()
    {
        CapturingHandler handler = new(_ => throw new HttpRequestException("secret network detail"));
        using HttpClient client = new(handler);
        OllamaCharacterModel model = new(client, Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DiagnosticSinkFailureDoesNotChangeCompletedGeneration()
    {
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments()));
        using HttpClient client = new(handler);
        OllamaCharacterModel model = new(
            client,
            Settings(),
            OllamaEndpoint.Default,
            new SequenceTimeProvider(TimeSpan.Zero, TimeSpan.FromMilliseconds(5)),
            new ThrowingDiagnosticSink());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task PropagatesCancellationAndRejectsInvalidDependencies()
    {
        CapturingHandler handler = new(_ => NdjsonResponse(CompletedDocuments()));
        using HttpClient client = new(handler);
        OllamaCharacterModel model = new(client, Settings());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CollectAsync(model, CreateRequest(), cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() => CollectAsync(model, null!));
        Assert.Throws<ArgumentNullException>(() => new OllamaCharacterModel(null!, Settings()));
        Assert.Throws<ArgumentNullException>(() => new OllamaCharacterModel(client, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new OllamaCharacterModel(client, Settings(), null!));
        Assert.Throws<ArgumentException>(() => new OllamaCharacterModel(
            client,
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "model-x")));
        Assert.Throws<ArgumentNullException>(() => new OllamaCharacterModel(
            client,
            Settings(),
            OllamaEndpoint.Default,
            null!,
            new RecordingDiagnosticSink()));
        Assert.Throws<ArgumentNullException>(() => new OllamaCharacterModel(
            client,
            Settings(),
            OllamaEndpoint.Default,
            TimeProvider.System,
            null!));
    }

    private static CharacterModelSettings Settings() =>
        new(CharacterModelProvider.Ollama, "model-x");

    private static async Task<CharacterModelStreamUpdate[]> CollectAsync(
        OllamaCharacterModel model,
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

    private static HttpResponseMessage NdjsonResponse(
        string body,
        string? mediaType = "application/x-ndjson")
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

    private static string CompletedDocuments() =>
        Line(ResponseDocument, done: false)
        + Line(string.Empty, done: true, "stop", 11, 7);

    private static string Line(
        string content,
        bool done,
        string? doneReason = null,
        int? promptCount = null,
        int? outputCount = null) => JsonSerializer.Serialize(new
        {
            model = "model-x",
            created_at = "2026-09-10T00:00:00Z",
            message = new { role = "assistant", content },
            done,
            done_reason = doneReason,
            prompt_eval_count = promptCount,
            eval_count = outputCount,
        }, SerializerOptions) + "\n";

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
                        playerText),
                ]));
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

        public IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> OtherHeaders
        { get; private set; } = [];

        public string? Accept { get; private set; }

        public string? ContentType { get; private set; }

        public string? Body { get; private set; }

        public bool CanSendAgain => !Disposed;

        private bool Disposed { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            OtherHeaders = request.Headers
                .Where(header => header.Key != "Accept")
                .ToArray();
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
