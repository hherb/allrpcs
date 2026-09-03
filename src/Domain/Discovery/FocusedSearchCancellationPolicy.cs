namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search request may be cancelled under the recorded active searches.
/// </summary>
public static class FocusedSearchCancellationPolicy
{
    /// <summary>
    /// Determines whether an activity for the complete supplied request is currently recorded.
    /// </summary>
    public static bool CanCancelSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(activities);

        return activities.Entries.Any(activity => activity.Request == request);
    }
}
