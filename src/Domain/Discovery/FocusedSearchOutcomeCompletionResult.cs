namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Retains the activity and outcome snapshots resulting from one focused-search completion with
/// an outcome without performing that completion.
/// </summary>
public sealed record FocusedSearchOutcomeCompletionResult
{
    /// <summary>
    /// Creates an immutable result from existing activity and outcome snapshots.
    /// </summary>
    public FocusedSearchOutcomeCompletionResult(
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(outcomes);

        Activities = activities;
        Outcomes = outcomes;
    }

    /// <summary>
    /// Gets the retained focused-search activity snapshot.
    /// </summary>
    public FocusedSearchActivitySnapshot Activities { get; }

    /// <summary>
    /// Gets the retained focused-search outcome snapshot.
    /// </summary>
    public FocusedSearchOutcomeSnapshot Outcomes { get; }
}
