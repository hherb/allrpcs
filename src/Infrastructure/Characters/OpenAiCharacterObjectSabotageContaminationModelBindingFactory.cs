using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI contamination dependencies without input/output.</summary>
public static class OpenAiCharacterObjectSabotageContaminationModelBindingFactory
{
    /// <summary>Creates an OpenAI contamination binding over exact caller-owned dependencies.</summary>
    public static CharacterObjectSabotageContaminationModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI contamination binding for one validated API base.</summary>
    public static CharacterObjectSabotageContaminationModelBinding Create(
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

        return new CharacterObjectSabotageContaminationModelBinding(
            settings,
            new OpenAiCharacterObjectSabotageContaminationModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
