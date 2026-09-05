namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Selects a focused-search outcome by its complete request without changing recorded outcomes.
/// </summary>
public static class FocusedSearchOutcomeLookup
{
    /// <summary>
    /// Returns the exact outcome for a value-equal request, or null when no outcome matches.
    /// </summary>
    public static FocusedSearchOutcome? FindByRequest(
        FocusedSearchRequest request,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(outcomes);

        return outcomes.Entries.FirstOrDefault(outcome => outcome.Request == request);
    }
}
