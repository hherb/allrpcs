using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI target-delay dependencies without I/O.</summary>
public static class OpenAiCharacterTargetSabotageDelayModelBindingFactory
{
    /// <summary>Creates an OpenAI target-delay binding over exact dependencies.</summary>
    public static CharacterTargetSabotageDelayModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource) =>
        Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI target-delay binding for one validated API base.</summary>
    public static CharacterTargetSabotageDelayModelBinding Create(
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

        return new CharacterTargetSabotageDelayModelBinding(
            settings,
            new OpenAiCharacterTargetSabotageDelayModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
