namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Completes one eligible focused search by returning a new activity snapshot.
/// </summary>
public static class FocusedSearchCompletionTransition
{
    /// <summary>
    /// Removes the activity for the supplied request without changing the input snapshot.
    /// </summary>
    public static FocusedSearchActivitySnapshot CompleteSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        if (!FocusedSearchCompletionPolicy.CanCompleteSearch(request, activities))
        {
            throw new InvalidOperationException(
                "Only an active focused search may be completed.");
        }

        return new FocusedSearchActivitySnapshot(
            activities.Entries.Where(activity => activity.Request != request));
    }
}
