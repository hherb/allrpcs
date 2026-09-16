using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact selected misquotation-evidence event pair without judging truth.</summary>
public sealed class CharacterDialogueMisquotationEvidencePresentationSnapshot
{
    /// <summary>Creates one immutable presentation from a non-abstaining exact selection.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSnapshot(
        CharacterDialogueMisquotationEvidencePresentationSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        CharacterDialogueMisquotationEvidence selectedEvidence =
            selection.SelectedEvidence
            ?? throw new ArgumentException(
                "A misquotation-evidence presentation requires selected evidence.",
                nameof(selection));

        Selection = selection;
        WitnessCharacterId = selectedEvidence.WitnessCharacterId;
        SourceEvent = selectedEvidence.Occurrence.Proposal.SourceEvent;
        DeliveredEvent = selectedEvidence.Occurrence.DeliveredEvent;
    }

    /// <summary>Gets the exact accepted presentation selection.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSelection Selection { get; }

    /// <summary>Gets the exact witness identifier retained by the selected evidence.</summary>
    public CharacterId WitnessCharacterId { get; }

    /// <summary>Gets the exact source dialogue event without declaring it truthful.</summary>
    public DialogueEvent SourceEvent { get; }

    /// <summary>Gets the exact delivered misquotation event without declaring it false.</summary>
    public DialogueEvent DeliveredEvent { get; }
}
