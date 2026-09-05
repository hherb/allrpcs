using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search outcome applies to one character perception snapshot
/// without changing perception.
/// </summary>
public static class FocusedSearchOutcomeApplicabilityPolicy
{
    /// <summary>
    /// Returns true when the snapshot represents the outcome's searcher and contains the target
    /// object.
    /// </summary>
    public static bool CanApply(
        FocusedSearchOutcome outcome,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(perception);

        return perception.CharacterId == outcome.Request.SearcherCharacterId
            && CharacterPerceptionLookup.FindByObject(
                outcome.Request.TargetObjectId,
                perception) is not null;
    }
}
