namespace Allrpcs.Domain.Characters;

/// <summary>Projects one accepted exact selection into a truth-neutral presentation.</summary>
public static class CharacterDialogueMisquotationEvidencePresentationProjection
{
    /// <summary>Returns a presentation for current selected evidence, or null otherwise.</summary>
    public static CharacterDialogueMisquotationEvidencePresentationSnapshot? Project(
        CharacterDialogueMisquotationEvidenceViewSnapshot currentView,
        CharacterDialogueMisquotationEvidencePresentationSelection selection)
    {
        ArgumentNullException.ThrowIfNull(currentView);
        ArgumentNullException.ThrowIfNull(selection);

        if (!CharacterDialogueMisquotationEvidencePresentationSelectionPolicy.CanAccept(
                currentView,
                selection)
            || selection.SelectedEvidence is null)
        {
            return null;
        }

        return new CharacterDialogueMisquotationEvidencePresentationSnapshot(selection);
    }
}
