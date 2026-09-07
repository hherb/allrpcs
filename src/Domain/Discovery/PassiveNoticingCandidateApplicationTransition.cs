using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies one eligible passive-noticing candidate to a perception snapshot.
/// </summary>
public static class PassiveNoticingCandidateApplicationTransition
{
    /// <summary>
    /// Returns a new snapshot with only the candidate target changed from unnoticed to noticed.
    /// </summary>
    public static CharacterPerceptionSnapshot Apply(
        PassiveNoticingCandidate candidate,
        CharacterPerceptionSnapshot perception)
    {
        if (!PassiveNoticingCandidateChangePolicy.WouldChange(candidate, perception))
        {
            throw new InvalidOperationException(
                "Only an applicable passive-noticing candidate for an unnoticed target may be applied.");
        }

        return new CharacterPerceptionSnapshot(
            perception.CharacterId,
            perception.Entries.Select(entry =>
                entry.ObjectId == candidate.TargetObjectId
                    ? new WorldObjectVisibility(entry.ObjectId, VisibilityState.Noticed)
                    : entry));
    }
}
