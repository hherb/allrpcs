using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one observation-reinterpretation candidate can change perception and then be
/// consumed without changing any input.
/// </summary>
public static class ObservationReinterpretationCandidateResolutionPolicy
{
    /// <summary>
    /// Returns true when the triggered candidate would change perception and is retained for
    /// consumption.
    /// </summary>
    public static bool CanResolve(
        ObservationReinterpretationCandidate candidate,
        Character character,
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates) =>
        ObservationReinterpretationCandidateChangePolicy.WouldChange(
            candidate,
            character,
            perception)
        && ObservationReinterpretationCandidateConsumptionPolicy.CanConsume(
            candidate,
            candidates);
}
