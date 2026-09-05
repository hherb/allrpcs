using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Retains the perception and outcome snapshots resulting from one focused-search outcome
/// resolution without performing that resolution.
/// </summary>
public sealed record FocusedSearchOutcomeResolutionResult
{
    /// <summary>
    /// Creates an immutable result from existing perception and outcome snapshots.
    /// </summary>
    public FocusedSearchOutcomeResolutionResult(
        CharacterPerceptionSnapshot perception,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(outcomes);

        Perception = perception;
        Outcomes = outcomes;
    }

    /// <summary>
    /// Gets the retained character-perception snapshot.
    /// </summary>
    public CharacterPerceptionSnapshot Perception { get; }

    /// <summary>
    /// Gets the retained focused-search outcome snapshot.
    /// </summary>
    public FocusedSearchOutcomeSnapshot Outcomes { get; }
}
