using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes the existing typed decision binding for any supported provider without I/O.</summary>
public static class CharacterObjectSabotageContaminationModelBindingFactory
{
    /// <summary>Retains exact settings, caller-owned transport and provider-specific dependencies.</summary>
    public static CharacterObjectSabotageContaminationModelBinding Create(
        CharacterModelSettings settings, HttpClient client, IOpenAiCredentialSource? credentialSource = null,
        OpenAiEndpoint? endpoint = null, IAnthropicCredentialSource? anthropicCredentials = null,
        OllamaEndpoint? ollamaEndpoint = null, ProviderHttpTimeoutOptions? timeoutOptions = null) =>
        new(settings, new CharacterObjectSabotageContaminationModel(StructuredDecisionClientFactory.Create(
            client, settings, CharacterStructuredDecisions.ObjectSabotageContamination, credentialSource, anthropicCredentials, endpoint, ollamaEndpoint, timeoutOptions)));
}
