namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact witness view and exact member evidence or explicit abstention.</summary>
public sealed class CharacterDialogueMisquotationEvidencePresentationSelection
{
    /// <summary>Creates one immutable presentation selection from an exact witness view.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSelection(
        CharacterDialogueMisquotationEvidenceViewSnapshot witnessView,
        CharacterDialogueMisquotationEvidence? selectedEvidence)
    {
        ArgumentNullException.ThrowIfNull(witnessView);

        if (selectedEvidence is not null
            && !witnessView.Entries.Any(evidence => ReferenceEquals(evidence, selectedEvidence)))
        {
            throw new ArgumentException(
                "Selected misquotation evidence must be an exact member of the witness view.",
                nameof(selectedEvidence));
        }

        WitnessView = witnessView;
        SelectedEvidence = selectedEvidence;
    }

    /// <summary>Gets the exact witness view from which the selection was made.</summary>
    public CharacterDialogueMisquotationEvidenceViewSnapshot WitnessView { get; }

    /// <summary>Gets the exact selected member evidence, or null to abstain.</summary>
    public CharacterDialogueMisquotationEvidence? SelectedEvidence { get; }
}
