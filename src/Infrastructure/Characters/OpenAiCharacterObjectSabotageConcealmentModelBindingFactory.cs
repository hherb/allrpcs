using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI concealment dependencies without input/output.</summary>
public static class OpenAiCharacterObjectSabotageConcealmentModelBindingFactory
{
    /// <summary>Creates an OpenAI concealment binding over exact caller-owned dependencies.</summary>
    public static CharacterObjectSabotageConcealmentModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI concealment binding for one validated API base.</summary>
    public static CharacterObjectSabotageConcealmentModelBinding Create(
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

        return new CharacterObjectSabotageConcealmentModelBinding(
            settings,
            new OpenAiCharacterObjectSabotageConcealmentModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
