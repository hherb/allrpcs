namespace Allrpcs.Domain.Characters;

/// <summary>Accepts presentation selection only for its exact current witness view.</summary>
public static class CharacterDialogueMisquotationEvidencePresentationSelectionPolicy
{
    /// <summary>Returns whether the selection retains the supplied exact current witness view.</summary>
    public static bool CanAccept(
        CharacterDialogueMisquotationEvidenceViewSnapshot currentView,
        CharacterDialogueMisquotationEvidencePresentationSelection selection)
    {
        ArgumentNullException.ThrowIfNull(currentView);
        ArgumentNullException.ThrowIfNull(selection);

        return ReferenceEquals(currentView, selection.WitnessView);
    }
}
