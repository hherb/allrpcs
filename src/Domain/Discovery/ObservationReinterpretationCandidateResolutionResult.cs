using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Retains the perception and candidate snapshots resulting from one observation reinterpretation
/// without performing that resolution.
/// </summary>
public sealed record ObservationReinterpretationCandidateResolutionResult
{
    /// <summary>
    /// Creates an immutable result from existing perception and candidate snapshots.
    /// </summary>
    public ObservationReinterpretationCandidateResolutionResult(
        CharacterPerceptionSnapshot perception,
        ObservationReinterpretationCandidateSnapshot candidates)
    {
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(candidates);

        Perception = perception;
        Candidates = candidates;
    }

    /// <summary>
    /// Gets the retained character-perception snapshot.
    /// </summary>
    public CharacterPerceptionSnapshot Perception { get; }

    /// <summary>
    /// Gets the retained observation-reinterpretation candidate snapshot.
    /// </summary>
    public ObservationReinterpretationCandidateSnapshot Candidates { get; }
}
