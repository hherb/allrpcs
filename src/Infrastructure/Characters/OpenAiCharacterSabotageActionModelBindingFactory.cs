using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI sabotage-action dependencies without I/O.</summary>
public static class OpenAiCharacterSabotageActionModelBindingFactory
{
    /// <summary>Creates an OpenAI sabotage-action binding over exact dependencies.</summary>
    public static CharacterSabotageActionModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource) =>
        Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI sabotage-action binding for one validated API base.</summary>
    public static CharacterSabotageActionModelBinding Create(
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

        return new CharacterSabotageActionModelBinding(
            settings,
            new OpenAiCharacterSabotageActionModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
