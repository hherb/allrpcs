using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;
using static Allrpcs.Infrastructure.Tests.StructuredDecisionTestModels;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiCharacterTargetSabotageDelayModelTests
{
    [Fact]
    public async Task PostsOneAuthenticatedStatelessRequestAndReturnsTheExactDecision()
    {
        CharacterTargetSabotageDelayConsiderationRequest request =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request;
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request)));
        using HttpClient client = new(handler);
        CharacterTargetSabotageDelayModel model = CreateCharacterTargetSabotageDelay(
            client,
            new StubCredentialSource("test-secret"),
            Settings());

        CharacterTargetSabotageDelayDecision decision =
            await model.DecideAsync(request, CancellationToken.None);

        Assert.Same(request.Actor.Identity.Id, decision.ActorId);
        Assert.Same(request.Candidates[0], decision.SelectedCandidate);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://api.openai.com/v1/responses", handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("test-secret", handler.Authorization?.Parameter);
        Assert.Equal("text/event-stream", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.False(body.RootElement.GetProperty("store").GetBoolean());
        Assert.True(body.RootElement.GetProperty("stream").GetBoolean());
    }

    [Fact]
    public async Task UsesTheExactValidatedCallerOwnedResponsesEndpoint()
    {
        CharacterTargetSabotageDelayConsiderationRequest request =
            CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request;
        CapturingHandler handler = new(_ => EventStreamResponse(CompletedEvents(request)));
        using HttpClient client = new(handler);
        CharacterTargetSabotageDelayModel model = CreateCharacterTargetSabotageDelay(
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
        CharacterTargetSabotageDelayModel model = CreateCharacterTargetSabotageDelay(
            client,
            new StubCredentialSource(apiKey),
            Settings());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await model.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                CancellationToken.None));

        Assert.Equal(0, handler.SendCount);
        if (!string.IsNullOrEmpty(apiKey))
        {
            Assert.DoesNotContain(apiKey, exception.ToString(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task RejectsHttpAndContentTypeFailuresWithoutProviderDetails()
    {
        using HttpClient rejectedClient = new(new CapturingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("secret provider response"),
            }));
        CharacterTargetSabotageDelayModel rejected = CreateCharacterTargetSabotageDelay(
            rejectedClient,
            new StubCredentialSource("test-secret"),
            Settings());
        HttpRequestException rejection = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await rejected.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, rejection.StatusCode);
        Assert.DoesNotContain("secret", rejection.ToString(), StringComparison.Ordinal);

        using HttpClient jsonClient = new(new CapturingHandler(_ =>
            EventStreamResponse("{}", "application/json")));
        CharacterTargetSabotageDelayModel json = CreateCharacterTargetSabotageDelay(
            jsonClient,
            new StubCredentialSource("test-secret"),
            Settings());
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await json.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                CancellationToken.None));
    }

    [Fact]
    public async Task RedactsTransportAndSemanticFailuresAndPropagatesCallerCancellation()
    {
        using HttpClient transportClient = new(new CapturingHandler(_ =>
            throw new HttpRequestException("secret network detail")));
        CharacterTargetSabotageDelayModel transport = CreateCharacterTargetSabotageDelay(
            transportClient,
            new StubCredentialSource("test-secret"),
            Settings());
        IOException transportFailure = await Assert.ThrowsAsync<IOException>(async () =>
            await transport.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                CancellationToken.None));
        Assert.DoesNotContain("secret", transportFailure.ToString(), StringComparison.Ordinal);

        using HttpClient semanticClient = new(new CapturingHandler(_ => EventStreamResponse(
            Frame(new { type = "response.failed", detail = "secret provider detail" }))));
        CharacterTargetSabotageDelayModel semantic = CreateCharacterTargetSabotageDelay(
            semanticClient,
            new StubCredentialSource("test-secret"),
            Settings());
        InvalidDataException semanticFailure = await Assert.ThrowsAsync<InvalidDataException>(
            async () => await semantic.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                CancellationToken.None));
        Assert.DoesNotContain("secret", semanticFailure.ToString(), StringComparison.Ordinal);

        CapturingHandler unused = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient unusedClient = new(unused);
        CharacterTargetSabotageDelayModel cancelling = CreateCharacterTargetSabotageDelay(
            unusedClient,
            new StubCredentialSource("test-secret"),
            Settings());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await cancelling.DecideAsync(
                CharacterTargetSabotageDelayDecisionJsonDeserializerTests.CreateInput().Request,
                cancellation.Token));
        Assert.Equal(0, unused.SendCount);
    }

    [Fact]
    public void RejectsNullsWrongProviderAndExposesOnlyTheDecisionInterface()
    {
        using HttpClient client = new(new CapturingHandler(_ => throw new NotSupportedException()));
        StubCredentialSource credentials = new("test-secret");
        CharacterTargetSabotageDelayModel model = CreateCharacterTargetSabotageDelay(client, credentials, Settings());

        Assert.Throws<ArgumentNullException>(() =>
            CreateCharacterTargetSabotageDelay(null!, credentials, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            CreateCharacterTargetSabotageDelay(client, null!, Settings()));
        Assert.Throws<ArgumentNullException>(() =>
            CreateCharacterTargetSabotageDelay(client, credentials, null!));
        Assert.Throws<ArgumentNullException>(() => CreateCharacterTargetSabotageDelay(
            client,
            credentials,
            new CharacterModelSettings(CharacterModelProvider.Anthropic, "model-x")));
        Assert.Throws<ArgumentNullException>(() => new CharacterTargetSabotageDelayModel(null!));
        Assert.Contains(typeof(ICharacterTargetSabotageDelayDecisionModel), model.GetType().GetInterfaces());
        System.Reflection.MethodInfo method = Assert.Single(model.GetType().GetMethods().Where(
            candidate => candidate.DeclaringType == model.GetType()));
        Assert.Equal(nameof(ICharacterTargetSabotageDelayDecisionModel.DecideAsync), method.Name);
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

    private static string CompletedEvents(CharacterTargetSabotageDelayConsiderationRequest request)
    {
        string document = OpenAiCharacterTargetSabotageDelayStreamAccumulatorTests
            .DecisionDocument(request.Actor.Identity.Id.Value, 0);
        return Frame(new { type = "response.created" })
            + Frame(new { type = "response.in_progress" })
            + Frame(new { type = "response.output_text.done", text = document })
            + Frame(new { type = "response.completed" });
    }

    private static string Frame(object semanticEvent) =>
        "data: " + JsonSerializer.Serialize(semanticEvent) + "\n\n";

    private sealed class StubCredentialSource(string? apiKey) : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken cancellationToken)
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
        internal int SendCount { get; private set; }

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
            return responseFactory(request);
        }
    }
}
