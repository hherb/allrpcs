using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI substitution dependencies without input/output.</summary>
public static class OpenAiCharacterObjectSabotageSubstitutionModelBindingFactory
{
    /// <summary>Creates an OpenAI substitution binding over exact caller-owned dependencies.</summary>
    public static CharacterObjectSabotageSubstitutionModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI substitution binding for one validated API base.</summary>
    public static CharacterObjectSabotageSubstitutionModelBinding Create(
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

        return new CharacterObjectSabotageSubstitutionModelBinding(
            settings,
            new OpenAiCharacterObjectSabotageSubstitutionModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
