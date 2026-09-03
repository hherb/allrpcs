namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Starts one eligible focused search by returning a new activity snapshot.
/// </summary>
public static class FocusedSearchStartTransition
{
    /// <summary>
    /// Appends one new activity for the supplied request without changing the input snapshot.
    /// </summary>
    public static FocusedSearchActivitySnapshot StartSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities)
    {
        if (!FocusedSearchStartPolicy.CanStartSearch(request, activities))
        {
            throw new InvalidOperationException(
                "A character cannot start a second active focused search.");
        }

        return new FocusedSearchActivitySnapshot(
            activities.Entries.Append(new FocusedSearchActivity(request)));
    }
}
