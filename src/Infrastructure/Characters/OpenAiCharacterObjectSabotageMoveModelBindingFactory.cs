using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI move dependencies without input/output.</summary>
public static class OpenAiCharacterObjectSabotageMoveModelBindingFactory
{
    /// <summary>Creates an OpenAI move binding over exact caller-owned dependencies.</summary>
    public static CharacterObjectSabotageMoveModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI move binding for one validated API base.</summary>
    public static CharacterObjectSabotageMoveModelBinding Create(
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

        return new CharacterObjectSabotageMoveModelBinding(
            settings,
            new OpenAiCharacterObjectSabotageMoveModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
