using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Projects active focused searches for one character without changing the recorded snapshot.
/// </summary>
public static class FocusedSearchActivityProjection
{
    /// <summary>
    /// Returns a new snapshot containing activities for the supplied character in stable order.
    /// </summary>
    public static FocusedSearchActivitySnapshot ForCharacter(
        CharacterId searcherCharacterId,
        FocusedSearchActivitySnapshot activities)
    {
        ArgumentNullException.ThrowIfNull(searcherCharacterId);
        ArgumentNullException.ThrowIfNull(activities);

        return new FocusedSearchActivitySnapshot(
            activities.Entries.Where(
                activity => activity.Request.SearcherCharacterId == searcherCharacterId));
    }
}
