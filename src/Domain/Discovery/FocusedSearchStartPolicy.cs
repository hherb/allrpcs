namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search request may start under the recorded active searches.
/// </summary>
public static class FocusedSearchStartPolicy
{
    /// <summary>
    /// Determines whether the requesting character has no existing focused-search activity.
    /// </summary>
    public static bool CanStartSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(activities);

        return !activities.Entries.Any(
            activity => activity.Request.SearcherCharacterId == request.SearcherCharacterId);
    }
}
