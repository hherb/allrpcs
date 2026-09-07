using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects the next resolvable observation-reinterpretation candidate for one character.
/// </summary>
public static class ObservationReinterpretationCandidateResolutionSelection
{
    /// <summary>
    /// Returns the exact first resolvable retained candidate in stable order, or null.
    /// </summary>
    public static ObservationReinterpretationCandidate? SelectNext(
        Character character,
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates.Entries.FirstOrDefault(
            candidate => ObservationReinterpretationCandidateResolutionPolicy.CanResolve(
                candidate,
                character,
                perception,
                candidates));
    }
}
