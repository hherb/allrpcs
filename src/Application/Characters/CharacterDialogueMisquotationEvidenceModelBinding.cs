using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact OpenAI setting and caller-composed evidence-decision model.</summary>
public sealed class CharacterDialogueMisquotationEvidenceModelBinding
{
    /// <summary>Creates an immutable OpenAI misquotation-evidence model binding.</summary>
    public CharacterDialogueMisquotationEvidenceModelBinding(
        CharacterModelSettings settings,
        ICharacterDialogueMisquotationEvidenceDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);
        if (settings.Provider != CharacterModelProvider.OpenAi)
        {
            throw new ArgumentException("Settings must select the OpenAI provider.", nameof(settings));
        }

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable OpenAI provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed misquotation-evidence decision model.</summary>
    public ICharacterDialogueMisquotationEvidenceDecisionModel Model { get; }
}
