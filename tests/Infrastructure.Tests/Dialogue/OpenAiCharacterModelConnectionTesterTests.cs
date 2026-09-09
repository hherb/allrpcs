using System.Net;
using System.Net.Http.Headers;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OpenAiCharacterModelConnectionTesterTests
{
    public static TheoryData<HttpStatusCode, CharacterModelFailureCode> FailureStatuses => new()
    {
        { HttpStatusCode.Unauthorized, CharacterModelFailureCode.Authentication },
        { HttpStatusCode.Forbidden, CharacterModelFailureCode.Authentication },
        { HttpStatusCode.TooManyRequests, CharacterModelFailureCode.RateLimited },
        { HttpStatusCode.RequestTimeout, CharacterModelFailureCode.Unavailable },
        { HttpStatusCode.BadGateway, CharacterModelFailureCode.Unavailable },
        { HttpStatusCode.NotFound, CharacterModelFailureCode.ProviderRejected },
    };

    [Fact]
    public async Task RetrievesTheExactConfiguredModelWithoutReadingTheResponseBody()
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ThrowingContent("must not read model metadata"),
        });
        using HttpClient client = new(handler);
        OpenAiCharacterModelConnectionTester tester = new(
            client,
            new StubCredentialSource(OpenAiCredentialLoadResult.Loaded("test-secret")));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt/test?model");

        CharacterModelConnectionTestResult result =
            await tester.TestConnectionAsync(settings, default);

        Assert.True(result.IsConnected);
        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal(
            "https://api.openai.com/v1/models/gpt%2Ftest%3Fmodel",
            handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.Authorization?.Scheme);
        Assert.Equal("test-secret", handler.Authorization?.Parameter);
        Assert.Equal("application/json", handler.Accept);
        Assert.Equal(1, handler.SendCount);
    }

    [Theory]
    [MemberData(nameof(FailureStatuses))]
    public async Task MapsHttpFailuresWithoutReadingOrRetainingProviderDetails(
        HttpStatusCode status,
        CharacterModelFailureCode expectedCode)
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(status)
        {
            Content = new ThrowingContent("secret provider details"),
        });
        using HttpClient client = new(handler);
        OpenAiCharacterModelConnectionTester tester = new(
            client,
            new StubCredentialSource(OpenAiCredentialLoadResult.Loaded("test-secret")));

        CharacterModelConnectionTestResult result = await tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "model-x"),
            default);

        Assert.False(result.IsConnected);
        Assert.Equal(expectedCode, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(OpenAiCredentialLoadStatus.Missing)]
    [InlineData(OpenAiCredentialLoadStatus.Failed)]
    public async Task RejectsUnavailableCredentialsWithoutSending(
        OpenAiCredentialLoadStatus credentialStatus)
    {
        OpenAiCredentialLoadResult credential = credentialStatus switch
        {
            OpenAiCredentialLoadStatus.Missing => OpenAiCredentialLoadResult.Missing(),
            OpenAiCredentialLoadStatus.Failed => OpenAiCredentialLoadResult.Failed(),
            _ => throw new ArgumentOutOfRangeException(nameof(credentialStatus)),
        };
        CapturingHandler handler = new(_ => throw new InvalidOperationException("must not send"));
        using HttpClient client = new(handler);
        OpenAiCharacterModelConnectionTester tester = new(
            client,
            new StubCredentialSource(credential));

        CharacterModelConnectionTestResult result = await tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "model-x"),
            default);

        Assert.Equal(CharacterModelFailureCode.Authentication, result.Failure?.Code);
        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task MapsTransportFailureAndPropagatesCallerCancellation()
    {
        CapturingHandler handler = new(_ => throw new HttpRequestException("secret network detail"));
        using HttpClient client = new(handler);
        OpenAiCharacterModelConnectionTester tester = new(
            client,
            new StubCredentialSource(OpenAiCredentialLoadResult.Loaded("test-secret")));
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "model-x");

        CharacterModelConnectionTestResult failed =
            await tester.TestConnectionAsync(settings, default);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.Equal(CharacterModelFailureCode.Transport, failed.Failure?.Code);
        Assert.DoesNotContain("secret", failed.ToString(), StringComparison.Ordinal);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => tester.TestConnectionAsync(settings, cancellation.Token).AsTask());
    }

    [Fact]
    public async Task RejectsWrongProviderNullInputsAndDependencies()
    {
        using HttpClient client = new(new CapturingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)));
        OpenAiCharacterModelConnectionTester tester = new(
            client,
            new StubCredentialSource(OpenAiCredentialLoadResult.Loaded("key")));

        await Assert.ThrowsAsync<ArgumentException>(() => tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.Anthropic, "model-x"),
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => tester.TestConnectionAsync(null!, default).AsTask());
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterModelConnectionTester(null!, new StubCredentialSource(
                OpenAiCredentialLoadResult.Loaded("key"))));
        Assert.Throws<ArgumentNullException>(() =>
            new OpenAiCharacterModelConnectionTester(client, null!));
    }

    private sealed class StubCredentialSource(
        OpenAiCredentialLoadResult result) : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(result);
        }
    }

    private sealed class CapturingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public AuthenticationHeaderValue? Authorization { get; private set; }

        public string? Accept { get; private set; }

        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            Accept = request.Headers.Accept.SingleOrDefault()?.MediaType;
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class ThrowingContent(string message) : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context) => throw new InvalidOperationException(message);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }
}
