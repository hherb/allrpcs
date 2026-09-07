using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects and resolves the next eligible observation-reinterpretation candidate for one
/// character.
/// </summary>
public static class ObservationReinterpretationCandidateResolutionSequence
{
    /// <summary>
    /// Returns the atomic resolution of the exact first eligible retained candidate.
    /// </summary>
    public static ObservationReinterpretationCandidateResolutionResult ResolveNext(
        Character character,
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ObservationReinterpretationCandidate? candidate =
            ObservationReinterpretationCandidateResolutionSelection.SelectNext(
                character,
                perception,
                candidates);

        if (candidate is null)
        {
            throw new InvalidOperationException(
                "No observation-reinterpretation candidate can be resolved for the supplied character.");
        }

        return ObservationReinterpretationCandidateResolutionTransition.Resolve(
            candidate,
            character,
            perception,
            candidates);
    }
}
