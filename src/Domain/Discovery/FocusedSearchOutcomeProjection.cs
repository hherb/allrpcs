using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Projects focused-search outcomes for one character without changing the recorded snapshot.
/// </summary>
public static class FocusedSearchOutcomeProjection
{
    /// <summary>
    /// Returns a new snapshot containing outcomes for the supplied character in stable order.
    /// </summary>
    public static FocusedSearchOutcomeSnapshot ForCharacter(
        CharacterId searcherCharacterId,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(searcherCharacterId);
        ArgumentNullException.ThrowIfNull(outcomes);

        return new FocusedSearchOutcomeSnapshot(
            outcomes.Entries.Where(
                outcome => outcome.Request.SearcherCharacterId == searcherCharacterId));
    }
}
