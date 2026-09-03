namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Cancels one eligible focused search by returning a new activity snapshot.
/// </summary>
public static class FocusedSearchCancellationTransition
{
    /// <summary>
    /// Removes the activity for the supplied request without changing the input snapshot.
    /// </summary>
    public static FocusedSearchActivitySnapshot CancelSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        if (!FocusedSearchCancellationPolicy.CanCancelSearch(request, activities))
        {
            throw new InvalidOperationException(
                "Only an active focused search may be cancelled.");
        }

        return new FocusedSearchActivitySnapshot(
            activities.Entries.Where(activity => activity.Request != request));
    }
}
