using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one triggered observation-reinterpretation candidate applies to an existing
/// character perception target without changing any input.
/// </summary>
public static class ObservationReinterpretationCandidateApplicabilityPolicy
{
    /// <summary>
    /// Returns true when the trigger is satisfied and matching observer perception retains the target.
    /// </summary>
    public static bool CanApply(
        ObservationReinterpretationCandidate candidate,
        Character character,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(perception);

        return ObservationReinterpretationCandidateTriggerPolicy.CanTrigger(candidate, character)
            && perception.CharacterId == candidate.ObserverCharacterId
            && CharacterPerceptionLookup.FindByObject(
                candidate.TargetObjectId,
                perception) is not null;
    }
}
