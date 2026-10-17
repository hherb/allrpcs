using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class ProviderModelDeadlineTests
{
    private static readonly bool[] Flags = [false, true];
    private static readonly CharacterModelProvider[] Providers =
        [CharacterModelProvider.OpenAi, CharacterModelProvider.Anthropic, CharacterModelProvider.Ollama];

    public static IEnumerable<object[]> Cases =>
        from provider in Providers
        from headers in Flags
        from cancel in Flags
        select new object[] { provider, headers, cancel };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TurnConsumersBoundHeadersAndBodyAndPreserveCallerCancellation(
        CharacterModelProvider provider, bool headers, bool cancel)
    {
        using ProviderDeadlineTestTransport transport = new()
        {
            StallHeaders = headers,
            MediaType = provider == CharacterModelProvider.Ollama ? "application/x-ndjson" : "text/event-stream"
        };
        using HttpClient client = new(transport);
        using CancellationTokenSource caller = new();
        CharacterModelSettings settings = new(provider, "model");
        Credential credential = new();
        ICharacterModel model = provider.Value switch
        {
            "openai" => new OpenAiCharacterModel(
                client, credential, settings, OpenAiEndpoint.Default, transport.Options),
            "anthropic" => new AnthropicCharacterModel(client, credential, settings, transport.Options),
            "ollama" => new OllamaCharacterModel(client, settings, OllamaEndpoint.Default, transport.Options),
            _ => throw new ArgumentOutOfRangeException(nameof(provider)),
        };
        Task<CharacterModelStreamTermination> task = ReadAsync(model, caller.Token);
        await transport.ExpireAsync(cancel, caller);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        else Assert.Equal(CharacterModelFailureCode.Transport, (await task).Failure?.Code);
        Assert.Equal(cancel, caller.IsCancellationRequested);
        Assert.False(transport.Disposed);
        if (!headers) Assert.True(transport.Body.Disposed);
        Assert.Equal(0, transport.Clock.ActiveTimerCount);
    }

    public static IEnumerable<object[]> ConnectionCases =>
        from provider in Providers
        from cancel in Flags
        select new object[] { provider, cancel };

    [Theory]
    [MemberData(nameof(ConnectionCases))]
    public async Task ConnectionTestersBoundHeadersAndPreserveCallerCancellation(
        CharacterModelProvider provider, bool cancel)
    {
        using ProviderDeadlineTestTransport transport = new() { StallHeaders = true };
        using HttpClient client = new(transport);
        using CancellationTokenSource caller = new();
        Credential credential = new();
        ICharacterModelConnectionTester tester = provider.Value switch
        {
            "openai" => new OpenAiCharacterModelConnectionTester(
                client, credential, OpenAiEndpoint.Default, transport.Options),
            "anthropic" => new AnthropicCharacterModelConnectionTester(client, credential, transport.Options),
            "ollama" => new OllamaCharacterModelConnectionTester(client, OllamaEndpoint.Default, transport.Options),
            _ => throw new ArgumentOutOfRangeException(nameof(provider)),
        };
        Task<CharacterModelConnectionTestResult> task = tester.TestConnectionAsync(
            new CharacterModelSettings(provider, "model"), caller.Token).AsTask();
        await transport.ExpireAsync(cancel, caller);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        else Assert.Equal(CharacterModelFailureCode.Transport, (await task).Failure?.Code);
        Assert.False(transport.Disposed);
        Assert.Equal(0, transport.Clock.ActiveTimerCount);
    }

    [Fact]
    public async Task AnthropicRejectsCrossHostRedirectWithoutASecondCredentialedRequest()
    {
        using RedirectHandler handler = new();
        using HttpClient client = new(handler);
        AnthropicCharacterModel model = new(client, new Credential(),
            new CharacterModelSettings(CharacterModelProvider.Anthropic, "model"));
        CharacterModelStreamTermination result = await ReadAsync(model, default);
        Assert.Equal(CharacterModelFailureCode.ProviderRejected, result.Failure?.Code);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal("api.anthropic.com", handler.RequestHost);
        Assert.Equal("test-secret", handler.ApiKey);
        using HttpClientHandler productionHandler = ProviderHttpTransport.CreateHandler();
        Assert.False(productionHandler.AllowAutoRedirect);
    }

    private sealed class RedirectHandler : HttpMessageHandler
    {
        internal int CallCount { get; private set; }
        internal string? RequestHost { get; private set; }
        internal string? ApiKey { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            RequestHost = request.RequestUri?.Host;
            ApiKey = request.Headers.GetValues("x-api-key").Single();
            HttpResponseMessage response = new(System.Net.HttpStatusCode.TemporaryRedirect);
            response.Headers.Location = new Uri("https://other.invalid/messages");
            return Task.FromResult(response);
        }
    }

    private static async Task<CharacterModelStreamTermination> ReadAsync(
        ICharacterModel model, CancellationToken cancellationToken)
    {
        CharacterId speaker = new("speaker");
        CharacterId listener = new("listener");
        DialogueUtterance utterance = new(speaker, "Wait");
        CharacterTurnRequest turn = new(listener, new DialogueEvent(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speaker, [listener])), new DialogueAct(utterance, DialogueActType.Request));
        CharacterModelGenerationRequest request = new(turn, new CharacterModelContextSnapshot([new CharacterModelContextEntry(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue, utterance.Text, DialogueDeliveryMode.OrdinarySpeech)]));
        await foreach (CharacterModelStreamUpdate update in model.StreamResponseAsync(request, cancellationToken))
        {
            if (update is CharacterModelStreamTermination terminal) return terminal;
        }

        throw new InvalidOperationException();
    }

    private sealed class Credential : IOpenAiCredentialSource, IAnthropicCredentialSource
    {
        ValueTask<OpenAiCredentialLoadResult> IOpenAiCredentialSource.LoadApiKeyAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult(OpenAiCredentialLoadResult.Loaded("test-secret"));
        ValueTask<AnthropicCredentialLoadResult> IAnthropicCredentialSource.LoadApiKeyAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult(AnthropicCredentialLoadResult.Loaded("test-secret"));
    }
}
