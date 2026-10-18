using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Characters;

public sealed class OpenAiResponsesStructuredClientTests
{
    private const string CompletedEvents =
        "data: {\"type\":\"response.created\"}\n\n" +
        "data: {\"type\":\"response.in_progress\"}\n\n" +
        "data: {\"type\":\"response.output_text.done\",\"text\":\"result\"}\n\n" +
        "data: {\"type\":\"response.completed\"}\n\n";

    [Fact]
    public async Task ReturnsAtTheTerminalFrameDisposesResponseAndReusesTheCallerClient()
    {
        List<TrackingStream> bodies = [];
        using HttpClient http = new(new Handler(_ =>
        {
            TrackingStream stream = new(CompletedEvents, throwOnEnd: true);
            bodies.Add(stream);
            return Response(HttpStatusCode.OK, "text/event-stream", stream);
        }));
        OpenAiResponsesStructuredClient<Request, Decision> client = Create(http);
        Request request = new();

        for (int index = 0; index < 2; index++)
        {
            Decision result = await client.DecideAsync(request, CancellationToken.None);
            Assert.Same(request, result.Request);
            Assert.Equal("result", result.Text);
        }

        Assert.Equal(2, bodies.Count);
        Assert.All(bodies, body =>
        {
            Assert.True(body.Disposed);
            Assert.False(body.ReadPastCompletion);
        });
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "text/event-stream", "private provider detail")]
    [InlineData(HttpStatusCode.OK, "application/json", "private provider detail")]
    [InlineData(HttpStatusCode.OK, "text/event-stream", "data: private provider detail\n\n")]
    public async Task DisposesRejectedResponsesWithoutLeakingProviderDetails(
        HttpStatusCode status, string mediaType, string payload)
    {
        TrackingStream body = new(payload);
        using HttpClient http = new(new Handler(_ => Response(status, mediaType, body)));
        OpenAiResponsesStructuredClient<Request, Decision> client = Create(http);

        Exception? failure = await Record.ExceptionAsync(async () =>
            await client.DecideAsync(new Request(), CancellationToken.None));

        Assert.NotNull(failure);
        if (status == HttpStatusCode.BadRequest)
            Assert.Equal(status, Assert.IsType<HttpRequestException>(failure).StatusCode);
        else
            Assert.IsType<InvalidDataException>(failure);
        Assert.DoesNotContain("private", failure.ToString(), StringComparison.Ordinal);
        Assert.True(body.Disposed);
    }

    [Fact]
    public async Task RedactsTransportFailuresWithoutDisposingTheCallerClient()
    {
        Handler handler = new(_ => throw new HttpRequestException("private network detail"));
        using HttpClient http = new(handler);
        OpenAiResponsesStructuredClient<Request, Decision> client = Create(http);
        IOException failure = await Assert.ThrowsAsync<IOException>(async () =>
            await client.DecideAsync(new Request(), CancellationToken.None));
        Assert.DoesNotContain("private", failure.ToString(), StringComparison.Ordinal);
        Assert.False(handler.Disposed);
    }

    private static OpenAiResponsesStructuredClient<Request, Decision> Create(HttpClient http) =>
        new(http, new Credential(), new CharacterModelSettings(CharacterModelProvider.OpenAi, "test"),
            OpenAiEndpoint.Default, (_, _) => "{}", (request, stream, token) =>
                new OpenAiStructuredDecisionAccumulator<Request, Decision>(request,
                    (actual, text) => new Decision(actual, text), 16).CompleteAsync(stream, token));

    private static HttpResponseMessage Response(HttpStatusCode status, string mediaType, Stream body)
    {
        HttpResponseMessage response = new(status) { Content = new StreamContent(body) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return response;
    }

    private sealed class Request;
    private sealed record Decision(Request Request, string Text);

    private sealed class Credential : IOpenAiCredentialSource
    {
        public ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("test-key"));
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> create) : HttpMessageHandler
    {
        internal bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(create(request));

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackingStream(string payload, bool throwOnEnd = false)
        : MemoryStream(Encoding.UTF8.GetBytes(payload))
    {
        internal bool Disposed { get; private set; }
        internal bool ReadPastCompletion { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (throwOnEnd && Position == Length)
            {
                ReadPastCompletion = true;
                throw new IOException("private trailing stream detail");
            }
            return base.ReadAsync(buffer[..Math.Min(1, buffer.Length)], cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
