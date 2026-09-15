namespace Allrpcs.Domain.Characters;

/// <summary>Applies an exact accepted substitution selection or preserves abstention.</summary>
public static class CharacterObjectSabotageSubstitutionConsiderationTransition
{
    /// <summary>Returns fresh substitution and evidence state for a selection, or null for abstention.</summary>
    public static CharacterObjectSabotageSubstitutionResult? Apply(
        CharacterObjectSabotageSubstitutionConsiderationResult considerationResult)
    {
        ArgumentNullException.ThrowIfNull(considerationResult);

        CharacterObjectSabotageSubstitutionCandidate? selectedCandidate =
            considerationResult.SelectedCandidate;
        if (selectedCandidate is null)
        {
            return null;
        }

        CharacterObjectSabotageSubstitutionConsiderationRequest request = considerationResult.Request;
        return CharacterObjectSabotageSubstitutionTransition.Apply(
            request.Character,
            request.PhysicalAccess,
            request.WorldObjects,
            request.Substitutions,
            request.Evidence,
            selectedCandidate);
    }
}
