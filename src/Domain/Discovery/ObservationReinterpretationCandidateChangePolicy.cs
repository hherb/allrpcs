using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one triggered observation-reinterpretation candidate would change existing
/// perception.
/// </summary>
public static class ObservationReinterpretationCandidateChangePolicy
{
    /// <summary>
    /// Returns true only when the candidate applies and its retained target is noticed.
    /// </summary>
    public static bool WouldChange(
        ObservationReinterpretationCandidate candidate,
        Character character,
        CharacterPerceptionSnapshot perception)
    {
        if (!ObservationReinterpretationCandidateApplicabilityPolicy.CanApply(
                candidate,
                character,
                perception))
        {
            return false;
        }

        WorldObjectVisibility target = CharacterPerceptionLookup.FindByObject(
            candidate.TargetObjectId,
            perception)!;

        return target.VisibilityState == VisibilityState.Noticed;
    }
}
