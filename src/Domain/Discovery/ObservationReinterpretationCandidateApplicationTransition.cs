using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies one eligible observation-reinterpretation candidate to a perception snapshot.
/// </summary>
public static class ObservationReinterpretationCandidateApplicationTransition
{
    /// <summary>
    /// Returns a new snapshot with only the candidate target changed from noticed to identified.
    /// </summary>
    public static CharacterPerceptionSnapshot Apply(
        ObservationReinterpretationCandidate candidate,
        Character character,
        CharacterPerceptionSnapshot perception)
    {
        if (!ObservationReinterpretationCandidateChangePolicy.WouldChange(
                candidate,
                character,
                perception))
        {
            throw new InvalidOperationException(
                "Only a triggered reinterpretation candidate for a noticed target may be applied.");
        }

        return new CharacterPerceptionSnapshot(
            perception.CharacterId,
            perception.Entries.Select(entry =>
                entry.ObjectId == candidate.TargetObjectId
                    ? new WorldObjectVisibility(entry.ObjectId, VisibilityState.Identified)
                    : entry));
    }
}
