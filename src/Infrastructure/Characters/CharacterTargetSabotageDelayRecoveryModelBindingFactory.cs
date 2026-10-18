using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes the existing typed decision binding for any supported provider without I/O.</summary>
public static class CharacterTargetSabotageDelayRecoveryModelBindingFactory
{
    /// <summary>Retains exact settings, caller-owned transport and provider-specific dependencies.</summary>
    public static CharacterTargetSabotageDelayRecoveryModelBinding Create(
        CharacterModelSettings settings, HttpClient client, IOpenAiCredentialSource? credentialSource = null,
        OpenAiEndpoint? endpoint = null, IAnthropicCredentialSource? anthropicCredentials = null,
        OllamaEndpoint? ollamaEndpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(settings, new CharacterTargetSabotageDelayRecoveryModel(StructuredDecisionClientFactory.Create(
            client, settings, CharacterStructuredDecisions.TargetSabotageDelayRecovery, credentialSource, anthropicCredentials, endpoint, ollamaEndpoint, timeoutOptions)));
}
