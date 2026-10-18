using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Retains one exact provider-selected setting and caller-composed evidence-decision model.</summary>
public sealed class CharacterDialogueMisquotationEvidenceModelBinding
{
    /// <summary>Creates an immutable provider-selected misquotation-evidence model binding.</summary>
    public CharacterDialogueMisquotationEvidenceModelBinding(
        CharacterModelSettings settings,
        ICharacterDialogueMisquotationEvidenceDecisionModel model)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(model);

        Settings = settings;
        Model = model;
    }

    /// <summary>Gets the exact immutable selected provider and model setting.</summary>
    public CharacterModelSettings Settings { get; }

    /// <summary>Gets the exact caller-composed misquotation-evidence decision model.</summary>
    public ICharacterDialogueMisquotationEvidenceDecisionModel Model { get; }
}
