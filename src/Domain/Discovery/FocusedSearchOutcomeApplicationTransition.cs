using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies one eligible, state-changing focused-search outcome to a perception snapshot.
/// </summary>
public static class FocusedSearchOutcomeApplicationTransition
{
    /// <summary>
    /// Returns a new snapshot with only the outcome target's visibility state replaced.
    /// </summary>
    public static CharacterPerceptionSnapshot Apply(
        FocusedSearchOutcome outcome,
        CharacterPerceptionSnapshot perception)
    {
        if (!FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception))
        {
            throw new InvalidOperationException(
                "Only an applicable focused-search outcome that changes perception may be applied.");
        }

        return new CharacterPerceptionSnapshot(
            perception.CharacterId,
            perception.Entries.Select(entry =>
                entry.ObjectId == outcome.Request.TargetObjectId
                    ? new WorldObjectVisibility(
                        entry.ObjectId,
                        outcome.ResultingVisibilityState)
                    : entry));
    }
}
