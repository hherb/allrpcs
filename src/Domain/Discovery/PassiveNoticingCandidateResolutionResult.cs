using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Retains the perception and candidate snapshots resulting from one passive-noticing resolution
/// without performing that resolution.
/// </summary>
public sealed record PassiveNoticingCandidateResolutionResult
{
    /// <summary>
    /// Creates an immutable result from existing perception and candidate snapshots.
    /// </summary>
    public PassiveNoticingCandidateResolutionResult(
        CharacterPerceptionSnapshot perception,
        PassiveNoticingCandidateSnapshot candidates)
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
    /// Gets the retained passive-noticing candidate snapshot.
    /// </summary>
    public PassiveNoticingCandidateSnapshot Candidates { get; }
}
