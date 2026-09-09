using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelTests
{
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
    public async Task PostsOneAuthenticatedStatelessStreamingRequestAndCompletes()
    {
        CharacterModelGenerationRequest request = CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        OpenAiCharacterModel model = new(client, new StubCredentialSource("test-secret"), Settings());

        CharacterModelStreamUpdate[] updates = await CollectAsync(model, request);

        CharacterModelStreamTermination result =
            Assert.IsType<CharacterModelStreamTermination>(Assert.Single(updates));
        Assert.Equal(CharacterModelStreamTerminationStatus.Completed, result.Status);
        Assert.Same(request.TurnRequest, result.Response?.Request);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.openai.com/v1/responses", handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("test-secret", handler.Authorization?.Parameter);
        Assert.Equal("text/event-stream", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("model-x", body.RootElement.GetProperty("model").GetString());
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.True(handler.CanSendAgain);
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
        OpenAiCharacterModel model = new(client, new StubCredentialSource("test-secret"), Settings());

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
        OpenAiCharacterModel model = new(client, new StubCredentialSource(apiKey), Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Authentication, result.Failure?.Code);
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task RejectsExcessiveCredentialWithoutSending()
    {
        string apiKey = new('k', OpenAiCharacterModel.MaximumApiKeyLength + 1);
        CapturingHandler handler = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient client = new(handler);
        OpenAiCharacterModel model = new(client, new StubCredentialSource(apiKey), Settings());

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
        OpenAiCharacterModel model = new(client, new StubCredentialSource("test-secret"), Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.InvalidResponse, result.Failure?.Code);
    }

    [Fact]
    public async Task MapsHttpTransportFailureWithoutRetainingDetails()
    {
        CapturingHandler handler = new(_ => throw new HttpRequestException("secret network detail"));
        using HttpClient client = new(handler);
        OpenAiCharacterModel model = new(client, new StubCredentialSource("test-secret"), Settings());

        CharacterModelStreamTermination result = Assert.IsType<CharacterModelStreamTermination>(
            Assert.Single(await CollectAsync(model, CreateRequest())));

        Assert.Equal(CharacterModelFailureCode.Transport, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task PropagatesCancellationAndRejectsNullDependenciesAndRequests()
    {
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents()));
        using HttpClient client = new(handler);
        OpenAiCharacterModel model = new(client, new StubCredentialSource("test-secret"), Settings());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CollectAsync(model, CreateRequest(), cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CollectAsync(model, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterModel(null!, new StubCredentialSource("key"), Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterModel(client, null!, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterModel(client, new StubCredentialSource("key"), null!));
        Assert.Throws<ArgumentException>(() => new OpenAiCharacterModel(
            client,
            new StubCredentialSource("key"),
            new CharacterModelSettings(CharacterModelProvider.Anthropic, "model-x")));
    }

    private static CharacterModelSettings Settings() =>
        new(CharacterModelProvider.OpenAi, "model-x");

    private static async Task<CharacterModelStreamUpdate[]> CollectAsync(
        OpenAiCharacterModel model,
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
        Frame(new { type = "response.output_text.delta", delta = ResponseDocument })
        + Frame(new { type = "response.output_text.done", text = ResponseDocument })
        + Frame(new { type = "response.completed" });

    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private static CharacterModelGenerationRequest CreateRequest()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        string playerText = "Please inspect the herb.";
        DialogueUtterance utterance = new(seekerId, playerText);
        DialogueEvent dialogueEvent = new(
            utterance,
            new DialogueAudienceSnapshot(seekerId, [healerId]));
        CharacterTurnRequest turnRequest = new(
            healerId,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterModelContextSnapshot context = new(
            [
                new CharacterModelContextEntry(
                    CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                    playerText),
            ]);
        return new CharacterModelGenerationRequest(turnRequest, context);
    }

    private sealed class StubCredentialSource(string? apiKey) : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(apiKey is null
                ? OpenAiCredentialLoadResult.Missing()
                : OpenAiCredentialPolicy.IsValid(apiKey)
                    ? OpenAiCredentialLoadResult.Loaded(apiKey)
                    : OpenAiCredentialLoadResult.Failed());
        }
    }

    private sealed class CapturingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public AuthenticationHeaderValue? Authorization { get; private set; }

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
