using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Characters;

/// <summary>Composes exact caller-owned OpenAI misquotation-evidence dependencies without I/O.</summary>
public static class OpenAiCharacterDialogueMisquotationEvidenceModelBindingFactory
{
    /// <summary>Creates an OpenAI misquotation-evidence binding over exact dependencies.</summary>
    public static CharacterDialogueMisquotationEvidenceModelBinding Create(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource credentialSource) =>
        Create(settings, client, credentialSource, OpenAiEndpoint.Default);

    /// <summary>Creates an OpenAI misquotation-evidence binding for one validated API base.</summary>
    public static CharacterDialogueMisquotationEvidenceModelBinding Create(
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

        return new CharacterDialogueMisquotationEvidenceModelBinding(
            settings,
            new OpenAiCharacterDialogueMisquotationEvidenceModel(
                client,
                credentialSource,
                settings,
                endpoint));
    }
}
