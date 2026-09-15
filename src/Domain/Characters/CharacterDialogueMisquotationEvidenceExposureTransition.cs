namespace Allrpcs.Domain.Characters;

/// <summary>Appends one exact accepted misquotation evidence selection or preserves abstention.</summary>
public static class CharacterDialogueMisquotationEvidenceExposureTransition
{
    /// <summary>Returns fresh exposure state for a selection, or the exact current state for abstention.</summary>
    public static CharacterDialogueMisquotationEvidenceExposureSnapshot Expose(
        CharacterDialogueMisquotationEvidenceExposureSnapshot current,
        CharacterDialogueMisquotationEvidenceConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterDialogueMisquotationEvidence? selectedEvidence = considerationResult.SelectedEvidence;
        if (selectedEvidence is null)
        {
            return current;
        }

        if (current.Entries.Count >= CharacterDialogueMisquotationEvidenceExposureSnapshot.MaximumEvidenceCount)
        {
            throw new InvalidOperationException("Exposure state has no capacity for more evidence.");
        }

        if (current.Entries.Any(entry => ReferenceEquals(
                entry.Occurrence.DeliveredEvent,
                selectedEvidence.Occurrence.DeliveredEvent)))
        {
            throw new InvalidOperationException("The exact delivered event is already exposed.");
        }

        return new CharacterDialogueMisquotationEvidenceExposureSnapshot(
            current.Entries.Append(selectedEvidence));
    }
}
