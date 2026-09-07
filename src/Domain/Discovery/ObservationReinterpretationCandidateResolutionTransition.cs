using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies and consumes one eligible observation-reinterpretation candidate as a single pure
/// resolution.
/// </summary>
public static class ObservationReinterpretationCandidateResolutionTransition
{
    /// <summary>
    /// Returns the perception and candidate snapshots produced by resolving the supplied candidate.
    /// </summary>
    public static ObservationReinterpretationCandidateResolutionResult Resolve(
        ObservationReinterpretationCandidate candidate,
        Character character,
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        if (!ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate,
                character,
                perception,
                candidates))
        {
            throw new InvalidOperationException(
                "Only a triggered, perception-changing retained observation-reinterpretation "
                + "candidate may be resolved.");
        }

        CharacterPerceptionSnapshot resolvedPerception =
            ObservationReinterpretationCandidateApplicationTransition.Apply(
                candidate,
                character,
                perception);
        ObservationReinterpretationCandidateSnapshot remainingCandidates =
            ObservationReinterpretationCandidateConsumptionTransition.Consume(
                candidate,
                candidates);

        return new ObservationReinterpretationCandidateResolutionResult(
            resolvedPerception,
            remainingCandidates);
    }
}
