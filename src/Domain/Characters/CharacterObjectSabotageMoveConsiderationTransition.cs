namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted move selection or preserves abstention.</summary>
public static class CharacterObjectSabotageMoveConsiderationTransition
{
    /// <summary>Returns fresh placement and evidence state for a selection, or null for abstention.</summary>
    public static CharacterObjectSabotageMoveResult? Apply(
        CharacterObjectSabotageMoveConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageMoveCandidate? selectedCandidate = considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterObjectSabotageMoveConsiderationRequest request = considerationResult.Request;
        return CharacterObjectSabotageMoveTransition.Apply(
            request.Character,
            request.PhysicalAccess,
            request.WorldObjects,
            request.Placements,
            request.Evidence,
            selectedCandidate);
    }
}
