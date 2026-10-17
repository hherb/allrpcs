using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class OllamaCharacterTurnParticipationTests
{
    [Fact]
    public void BoundsParticipationOutputTokens()
    {
        using JsonDocument document = JsonDocument.Parse(
            OllamaCharacterTurnParticipationRequestSerializer.Serialize(Request(), "model"));
        Assert.Equal(128, document.RootElement.GetProperty("options").GetProperty("num_predict").GetInt32());
    }

    [Fact]
    public void SerializesStatelessStructuredDecisionWithSeparatedTrustedAndPlayerContext()
    {
        CharacterTurnParticipationConsiderationRequest request = Request();
        using JsonDocument document = JsonDocument.Parse(
            OllamaCharacterTurnParticipationRequestSerializer.Serialize(request, "gemma4:e4b"));
        JsonElement root = document.RootElement;
        Assert.Equal("gemma4:e4b", root.GetProperty("model").GetString());
        Assert.True(root.GetProperty("stream").GetBoolean());
        Assert.False(root.GetProperty("think").GetBoolean());
        JsonElement.ArrayEnumerator messages = root.GetProperty("messages").EnumerateArray();
        JsonElement[] rows = messages.ToArray();
        Assert.Equal(["system", "system", "user"], rows.Select(r => r.GetProperty("role").GetString()));
        Assert.Equal(request.HeardDialogueEvent.Utterance.Text, rows[2].GetProperty("content").GetString());
        Assert.Contains("scheduledCharacterId", rows[1].GetProperty("content").GetString());
        Assert.False(root.GetProperty("format").GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(["kind", "motiveKey"],
            root.GetProperty("format").GetProperty("required").EnumerateArray().Select(x => x.GetString()));
        Assert.Equal(0, root.GetProperty("options").GetProperty("temperature").GetInt32());
        Assert.False(root.TryGetProperty("tools", out _));
    }

    [Theory]
    [InlineData(true, "respond", null)]
    [InlineData(false, "interject", "compare-evidence")]
    [InlineData(false, "remain-silent", "compare-evidence")]
    public async Task ReturnsOnlyTheExactRequestBoundDecision(bool addressed, string kind, string? motive)
    {
        CharacterTurnParticipationConsiderationRequest request = Request(addressed);
        string decision = JsonSerializer.Serialize(new { kind, motiveKey = motive });
        using Handler handler = new(_ => Response(Chunk(decision[..8]) + Chunk(decision[8..], true)));
        using HttpClient client = new(handler);
        OllamaCharacterTurnParticipationModel model = new(client, Settings());
        CharacterTurnParticipationDecision result = await model.DecideAsync(request, CancellationToken.None);
        Assert.Same(request.ScheduledCharacter.Identity.Id, result.ScheduledCharacterId);
        Assert.Same(CharacterTurnParticipationKind.Parse(kind), result.Kind);
        if (motive is not null) { Assert.Same(request.ScheduledCharacter.Goals.Entries.Single(), result.Motive); }
        Assert.Equal(new Uri("http://127.0.0.1:11434/api/chat"), handler.Uri);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Null(handler.Authorization);
        Assert.Contains("gemma4:e4b", handler.Body);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"kind\":\"respond\",\"motiveKey\":\"compare-evidence\"}")]
    [InlineData("{\"kind\":\"remain-silent\",\"motiveKey\":\"invented\"}")]
    [InlineData("{\"kind\":\"interject\",\"motiveKey\":\"compare-evidence\"}")]
    [InlineData("{\"kind\":\"respond\",\"kind\":\"respond\",\"motiveKey\":null}")]
    public async Task RejectsMalformedOrPolicyInvalidDecision(string decision)
    {
        using Handler handler = new(_ => Response(Chunk(decision, true)));
        using HttpClient client = new(handler);
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new OllamaCharacterTurnParticipationModel(client, Settings()).DecideAsync(Request(), CancellationToken.None));
    }

    [Theory]
    [InlineData("{}")] // missing native fields
    [InlineData("{\"error\":\"private provider text\"}\n")]
    [InlineData("{\"message\":{\"role\":\"user\",\"content\":\"x\"},\"done\":false}\n")]
    [InlineData("{\"message\":{\"role\":\"assistant\",\"content\":\"\"},\"done\":true,\"done_reason\":\"length\"}\n")]
    public async Task RejectsInvalidNativeStreamWithoutLeakingProviderContent(string body)
    {
        using Handler handler = new(_ => Response(body));
        using HttpClient client = new(handler);
        InvalidDataException failure = await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new OllamaCharacterTurnParticipationModel(client, Settings()).DecideAsync(Request(), CancellationToken.None));
        Assert.DoesNotContain("private provider text", failure.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequiresCompletionAndReturnsBeforeTrailingDocuments(bool trailing)
    {
        string body = Chunk("{\"kind\":\"respond\",\"motiveKey\":null}", trailing);
        if (trailing) { body += Chunk(""); }
        using Handler handler = new(_ => Response(body));
        using HttpClient client = new(handler);
        if (trailing)
        {
            Assert.NotNull(await new OllamaCharacterTurnParticipationModel(client, Settings())
                .DecideAsync(Request(), CancellationToken.None));
            return;
        }
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new OllamaCharacterTurnParticipationModel(client, Settings()).DecideAsync(Request(), CancellationToken.None));
    }

    [Fact]
    public async Task BoundsAccumulatedDecisionAndNativeLine()
    {
        foreach (string body in new[] {
            Chunk(new string('x', CharacterTurnParticipationDecisionJsonDeserializer.MaximumDocumentLength)) + Chunk("x", true),
            new string('x', OllamaNewlineDelimitedJsonReader.MaximumLineLength + 1) + "\n" })
        {
            using Handler handler = new(_ => Response(body));
            using HttpClient client = new(handler);
            await Assert.ThrowsAsync<InvalidDataException>(async () =>
                await new OllamaCharacterTurnParticipationModel(client, Settings()).DecideAsync(Request(), CancellationToken.None));
        }
    }

    [Fact]
    public async Task RejectsHttpFailureAndWrongMediaType()
    {
        foreach (HttpResponseMessage response in new[] {
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("private error") } })
        {
            using Handler handler = new(_ => response);
            using HttpClient client = new(handler);
            Exception? failure = await Record.ExceptionAsync(async () =>
                await new OllamaCharacterTurnParticipationModel(client, Settings()).DecideAsync(Request(), CancellationToken.None));
            Assert.NotNull(failure);
            Assert.DoesNotContain("private error", failure.ToString());
        }
    }

    [Fact]
    public async Task PropagatesCallerCancellationAndRedactsTransportFailure()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        using Handler handler = new(_ => throw new HttpRequestException("private transport detail"));
        using HttpClient client = new(handler);
        OllamaCharacterTurnParticipationModel model = new(client, Settings());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await model.DecideAsync(Request(), cancellation.Token));
        IOException failure = await Assert.ThrowsAsync<IOException>(async () => await model.DecideAsync(Request(), CancellationToken.None));
        Assert.DoesNotContain("private transport detail", failure.ToString());
    }

    [Fact]
    public async Task CancelsAnActiveParticipationRequest()
    {
        using CancellationTokenSource cancellation = new();
        using CancellableHandler handler = new();
        using HttpClient client = new(handler);
        Task<CharacterTurnParticipationDecision> pending = new OllamaCharacterTurnParticipationModel(client, Settings())
            .DecideAsync(Request(), cancellation.Token).AsTask();
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Fact]
    public void BindingRetainsExactSettingsAndRejectsOtherProviders()
    {
        using HttpClient client = new();
        CharacterModelSettings settings = Settings();
        CharacterTurnParticipationModelBinding binding = OllamaCharacterTurnParticipationModelBindingFactory.Create(
            settings, client, OllamaEndpoint.Default);
        Assert.Same(settings, binding.Settings);
        Assert.IsType<OllamaCharacterTurnParticipationModel>(binding.Model);
        Assert.Throws<ArgumentException>(() => OllamaCharacterTurnParticipationModelBindingFactory.Create(
            new(CharacterModelProvider.OpenAi, "test"), client, OllamaEndpoint.Default));
    }

    private static CharacterTurnParticipationConsiderationRequest Request(bool addressed = true) =>
        OpenAiCharacterTurnParticipationStreamAccumulatorTests.CreateRequest(addressed);

    private static CharacterModelSettings Settings() => new(CharacterModelProvider.Ollama, "gemma4:e4b");

    private static string Chunk(string content, bool done = false) => JsonSerializer.Serialize(new
    {
        message = new { role = "assistant", content },
        done,
        done_reason = done ? "stop" : null,
    }).Replace(",\"done_reason\":null", "", StringComparison.Ordinal) + "\n";

    private static HttpResponseMessage Response(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/x-ndjson"),
    };

    private sealed class CancellableHandler : HttpMessageHandler
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Unexpected request completion.");
        }
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Authorization { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Uri = request.RequestUri; Method = request.Method;
            Authorization = request.Headers.Authorization?.ToString();
            Body = await request.Content!.ReadAsStringAsync(token);
            return respond(request);
        }
    }
}
