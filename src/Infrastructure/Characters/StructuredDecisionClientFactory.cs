using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Selects one provider client without network, credential or request I/O during construction.</summary>
public static class StructuredDecisionClientFactory
{
    /// <summary>Creates an exact caller-owned provider binding with optional endpoint overrides.</summary>
    public static IStructuredDecisionClient<TRequest, TDecision> Create<TRequest, TDecision>(
        HttpClient client, CharacterModelSettings settings, StructuredDecisionDescriptor<TRequest, TDecision> descriptor,
        IOpenAiCredentialSource? openAiCredentials = null, IAnthropicCredentialSource? anthropicCredentials = null,
        OpenAiEndpoint? openAiEndpoint = null, OllamaEndpoint? ollamaEndpoint = null,
        ProviderHttpTimeoutOptions? timeoutOptions = null)
        where TRequest : class where TDecision : class
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (settings.Provider == CharacterModelProvider.OpenAi)
        {
            ArgumentNullException.ThrowIfNull(openAiCredentials);
            return new OpenAiResponsesStructuredClient<TRequest, TDecision>(client, openAiCredentials, settings,
                openAiEndpoint ?? OpenAiEndpoint.Default, descriptor, timeoutOptions);
        }
        if (settings.Provider == CharacterModelProvider.Anthropic)
        {
            ArgumentNullException.ThrowIfNull(anthropicCredentials);
            return new AnthropicMessagesStructuredClient<TRequest, TDecision>(
                client, anthropicCredentials, settings, descriptor, timeoutOptions);
        }
        if (settings.Provider == CharacterModelProvider.Ollama)
        {
            return new OllamaChatStructuredClient<TRequest, TDecision>(
                client, settings, descriptor, ollamaEndpoint ?? OllamaEndpoint.Default, timeoutOptions);
        }
        throw new ArgumentOutOfRangeException(nameof(settings), "Unsupported character-model provider.");
    }
}
