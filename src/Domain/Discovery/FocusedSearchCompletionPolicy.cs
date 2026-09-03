namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search request may be completed under the recorded active searches.
/// </summary>
public static class FocusedSearchCompletionPolicy
{
    /// <summary>
    /// Determines whether an activity for the complete supplied request is currently recorded.
    /// </summary>
    public static bool CanCompleteSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(activities);

        return activities.Entries.Any(activity => activity.Request == request);
    }
}
