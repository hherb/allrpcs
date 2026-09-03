using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Discovery;

/// <summary>
/// Decides whether one character has focused-search activity to indicate.
/// </summary>
public static class FocusedSearchActivityIndicatorPolicy
{
    /// <summary>
    /// Returns whether the per-character focused-search projection contains any activity.
    /// </summary>
    public static bool ShouldShow(
        CharacterId searcherCharacterId,
        FocusedSearchActivitySnapshot activities) =>
        FocusedSearchActivityProjection
            .ForCharacter(searcherCharacterId, activities)
            .Entries.Count > 0;
}
