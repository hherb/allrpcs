using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI delay-recovery dependencies without I/O.</summary>
public static class OpenAiCharacterTargetSabotageDelayRecoveryModelBindingFactory
{
    /// <summary>Creates an OpenAI delay-recovery binding over exact dependencies.</summary>
    public static CharacterTargetSabotageDelayRecoveryModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource) =>
        Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI delay-recovery binding for one validated API base.</summary>
    public static CharacterTargetSabotageDelayRecoveryModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource,
        OpenAiEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(credentialSource);
        ArgumentNullException.ThrowIfNull(endpoint);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        return new CharacterTargetSabotageDelayRecoveryModelBinding(
            settings,
            new OpenAiCharacterTargetSabotageDelayRecoveryModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
