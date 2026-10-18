using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterObjectSabotageContaminationModelTests
{
    [Fact]
    public async Task PostsOneAuthenticatedStatelessRequestAndReturnsTheExactDecision()
    {
        CharacterObjectSabotageContaminationConsiderationRequest request =
            OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request)));
        using HttpClient client = new(handler);
        OpenAiCharacterObjectSabotageContaminationModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());
        using CancellationTokenSource cancellation = new();

        CharacterObjectSabotageContaminationDecision decision =
            await model.DecideAsync(request, cancellation.Token);

        Assert.Same(request.Character.Identity.Id, decision.CharacterId);
        Assert.Same(request.Candidates[0].Evidence.EvidenceId, decision.SelectedEvidenceId);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.openai.com/v1/responses", handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("test-secret", handler.Authorization?.Parameter);
        Assert.Equal("text/event-stream", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        Assert.True(handler.CancellationToken.CanBeCanceled);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("model-x", body.RootElement.GetProperty("model").GetString());
        Assert.False(body.RootElement.GetProperty("store").GetBoolean());
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.True(handler.CanSendAgain);
    }

    [Fact]
    public async Task UsesTheExactValidatedCallerOwnedResponsesEndpoint()
    {
        CharacterObjectSabotageContaminationConsiderationRequest request =
            OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request)));
        using HttpClient client = new(handler);
        OpenAiCharacterObjectSabotageContaminationModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings(),
            new OpenAiEndpoint(new Uri("http://127.0.0.1:8000/v1")));

        await model.DecideAsync(request, CancellationToken.None);

        Assert.Equal("http://127.0.0.1:8000/v1/responses", handler.RequestUri?.AbsoluteUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" key with spaces ")]
    public async Task RejectsUnavailableOrInvalidCredentialsWithoutSending(string? apiKey)
    {
        CapturingHandler handler = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient client = new(handler);
        OpenAiCharacterObjectSabotageContaminationModel model = new(
            client,
            new StubCredentialSource(apiKey),
            Settings());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await model.DecideAsync(
                OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(),
                CancellationToken.None));

        Assert.Equal(0, handler.SendCount);
        if (!string.IsNullOrEmpty(apiKey))
        {
            Assert.DoesNotContain(apiKey, exception.ToString(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task RejectsHttpFailureWithoutReadingOrRetainingTheProviderBody()
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new ThrowingContent("secret provider response"),
        });
        using HttpClient client = new(handler);
        OpenAiCharacterObjectSabotageContaminationModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            async () => await model.DecideAsync(
                OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(),
                CancellationToken.None));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.DoesNotContain("secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    public async Task RequiresAnEventStreamSuccessContentType(string? mediaType)
    {
        CharacterObjectSabotageContaminationConsiderationRequest request =
            OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request), mediaType));
        using HttpClient client = new(handler);
        OpenAiCharacterObjectSabotageContaminationModel model = new(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await model.DecideAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RedactsTransportAndSemanticFailuresAndPropagatesCallerCancellation()
    {
        CapturingHandler transport = new(_ =>
            throw new HttpRequestException("secret network detail"));
        using HttpClient transportClient = new(transport);
        OpenAiCharacterObjectSabotageContaminationModel transportModel = new(
            transportClient,
            new StubCredentialSource("test-secret"),
            Settings());
        IOException transportException = await Assert.ThrowsAsync<IOException>(async () =>
            await transportModel.DecideAsync(
                OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(),
                CancellationToken.None));
        Assert.DoesNotContain("secret", transportException.ToString(), StringComparison.Ordinal);

        CapturingHandler semantic = new(_ => EventStreamResponse(
            Frame(new { type = "response.failed", detail = "secret provider detail" })));
        using HttpClient semanticClient = new(semantic);
        OpenAiCharacterObjectSabotageContaminationModel semanticModel = new(
            semanticClient,
            new StubCredentialSource("test-secret"),
            Settings());
        InvalidDataException semanticException = await Assert.ThrowsAsync<InvalidDataException>(
            async () => await semanticModel.DecideAsync(
                OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(),
                CancellationToken.None));
        Assert.DoesNotContain("secret", semanticException.ToString(), StringComparison.Ordinal);

        CapturingHandler unused = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient unusedClient = new(unused);
        OpenAiCharacterObjectSabotageContaminationModel cancellingModel = new(
            unusedClient,
            new StubCredentialSource("test-secret"),
            Settings());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cancellingModel.DecideAsync(
                OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest(),
                cancellation.Token));
        Assert.Equal(0, unused.SendCount);
    }

    [Fact]
    public async Task RejectsNullsWrongProviderAndExposesOnlyTheDecisionInterface()
    {
        CharacterObjectSabotageContaminationConsiderationRequest request =
            OpenAiCharacterObjectSabotageContaminationRequestSerializerTests.CreateRequest();
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request)));
        using HttpClient client = new(handler);
        StubCredentialSource credentials = new("test-secret");
        OpenAiCharacterObjectSabotageContaminationModel model = new(client, credentials, Settings());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await model.DecideAsync(null!, CancellationToken.None));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageContaminationModel(null!, credentials, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageContaminationModel(client, null!, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageContaminationModel(client, credentials, null!));
        Assert.Throws<ArgumentException>(() =>
            new OpenAiCharacterObjectSabotageContaminationModel(
                client,
                credentials,
                new CharacterModelSettings(CharacterModelProvider.Anthropic, "model-x")));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterObjectSabotageContaminationModel(
                client,
                credentials,
                Settings(),
                null!));

        Assert.Contains(
            typeof(ICharacterObjectSabotageContaminationDecisionModel),
            model.GetType().GetInterfaces());
        System.Reflection.MethodInfo method = Assert.Single(model.GetType().GetMethods().Where(
            candidate => candidate.DeclaringType == model.GetType()));
        Assert.Equal(nameof(ICharacterObjectSabotageContaminationDecisionModel.DecideAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterObjectSabotageContaminationDecision>), method.ReturnType);
        Assert.Empty(model.GetType().GetProperties());
        Assert.Empty(model.GetType().GetEvents());
        Assert.True(model.GetType().IsSealed);
    }

    private static CharacterModelSettings Settings() =>
        new(CharacterModelProvider.OpenAi, "model-x");

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

    private static string CompletedEvents(
        CharacterObjectSabotageContaminationConsiderationRequest request)
    {
        string document =
            OpenAiCharacterObjectSabotageContaminationStreamAccumulatorTests.DecisionDocument(
                request.Character.Identity.Id.Value,
                request.Candidates[0].Evidence.EvidenceId.Value);
        return Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.delta", delta = document })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
    }

    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

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
        internal HttpMethod? Method { get; private set; }
        internal Uri? RequestUri { get; private set; }
        internal AuthenticationHeaderValue? Authorization { get; private set; }
        internal string? Accept { get; private set; }
        internal string? ContentType { get; private set; }
        internal string? Body { get; private set; }
        internal CancellationToken CancellationToken { get; private set; }
        internal int SendCount { get; private set; }
        internal bool CanSendAgain { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            Accept = request.Headers.Accept.SingleOrDefault()?.MediaType;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            CancellationToken = cancellationToken;
            HttpResponseMessage response = responseFactory(request);
            CanSendAgain = request.Content is not null
                && request.Content.ReadAsStringAsync(cancellationToken).IsCompletedSuccessfully;
            return response;
        }
    }

    private sealed class ThrowingContent(string message) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            Task.FromException(new InvalidOperationException(message));

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
