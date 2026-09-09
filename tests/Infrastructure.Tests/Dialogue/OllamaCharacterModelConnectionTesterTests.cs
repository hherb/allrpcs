using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterModelConnectionTesterTests
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
    public async Task PostsTheExactConfiguredModelWithoutReadingTheResponseBody()
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ThrowingContent("must not read model metadata"),
        });
        using HttpClient client = new(handler);
        OllamaCharacterModelConnectionTester tester = new(client);
        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, "model/test:tag");

        CharacterModelConnectionTestResult result =
            await tester.TestConnectionAsync(settings, default);

        Assert.True(result.IsConnected);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("http://127.0.0.1:11434/api/show", handler.RequestUri?.AbsoluteUri);
        Assert.Null(handler.Authorization);
        Assert.Equal("application/json", handler.Accept);
        Assert.Equal("application/json", handler.ContentType);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal(["model"], body.RootElement.EnumerateObject().Select(value => value.Name));
        Assert.Equal("model/test:tag", body.RootElement.GetProperty("model").GetString());
        Assert.Equal(1, handler.SendCount);
    }

    [Fact]
    public async Task UsesAnExplicitValidatedLoopbackEndpoint()
    {
        CapturingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient client = new(handler);
        OllamaCharacterModelConnectionTester tester = new(
            client,
            new OllamaEndpoint(new Uri("http://[::1]:22334/", UriKind.Absolute)));

        CharacterModelConnectionTestResult result = await tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.Ollama, "model-x"),
            default);

        Assert.True(result.IsConnected);
        Assert.Equal("http://[::1]:22334/api/show", handler.RequestUri?.AbsoluteUri);
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
        OllamaCharacterModelConnectionTester tester = new(client);

        CharacterModelConnectionTestResult result = await tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.Ollama, "model-x"),
            default);

        Assert.False(result.IsConnected);
        Assert.Equal(expectedCode, result.Failure?.Code);
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task MapsTransportFailureAndPropagatesCallerCancellation()
    {
        CapturingHandler handler = new(_ => throw new HttpRequestException("secret network detail"));
        using HttpClient client = new(handler);
        OllamaCharacterModelConnectionTester tester = new(client);
        CharacterModelSettings settings = new(CharacterModelProvider.Ollama, "model-x");

        CharacterModelConnectionTestResult failed =
            await tester.TestConnectionAsync(settings, default);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.Equal(CharacterModelFailureCode.Transport, failed.Failure?.Code);
        Assert.DoesNotContain("secret", failed.ToString(), StringComparison.Ordinal);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            tester.TestConnectionAsync(settings, cancellation.Token).AsTask());
    }

    [Fact]
    public async Task RejectsWrongProviderNullInputsAndDependencies()
    {
        using HttpClient client = new(new CapturingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)));
        OllamaCharacterModelConnectionTester tester = new(client);

        await Assert.ThrowsAsync<ArgumentException>(() => tester.TestConnectionAsync(
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "model-x"),
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            tester.TestConnectionAsync(null!, default).AsTask());
        Assert.Throws<ArgumentNullException>(() =>
            new OllamaCharacterModelConnectionTester(null!));
        Assert.Throws<ArgumentNullException>(() =>
            new OllamaCharacterModelConnectionTester(client, null!));
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
