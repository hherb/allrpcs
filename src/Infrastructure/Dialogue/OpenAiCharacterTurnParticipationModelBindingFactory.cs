using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Composes exact caller-owned OpenAI participation dependencies without performing input/output.
/// </summary>
public static class OpenAiCharacterTurnParticipationModelBindingFactory
{
    /// <summary>Creates an OpenAI participation binding over exact caller-owned dependencies.</summary>
    public static CharacterTurnParticipationModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource)
        => Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates a participation binding over exact dependencies and one API base.</summary>
    public static CharacterTurnParticipationModelBinding Create(
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

        return new CharacterTurnParticipationModelBinding(
            CharacterModelProvider.OpenAi,
            settings,
            new OpenAiCharacterTurnParticipationModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
