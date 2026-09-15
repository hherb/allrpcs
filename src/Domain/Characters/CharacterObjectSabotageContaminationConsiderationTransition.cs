namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted contamination selection or preserves abstention.</summary>
public static class CharacterObjectSabotageContaminationConsiderationTransition
{
    /// <summary>Returns fresh contamination and evidence state for a selection, or null for abstention.</summary>
    public static CharacterObjectSabotageContaminationResult? Apply(
        CharacterObjectSabotageContaminationConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageEvidence? selectedCandidate = considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterObjectSabotageContaminationConsiderationRequest request = considerationResult.Request;
        return CharacterObjectSabotageContaminationTransition.Apply(
            request.Character,
            request.PhysicalAccess,
            request.WorldObjects,
            request.Contamination,
            request.Evidence,
            selectedCandidate);
    }
}
