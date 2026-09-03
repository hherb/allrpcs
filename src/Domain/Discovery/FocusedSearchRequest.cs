using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Identifies one character's request to focus a search on one existing world object without
/// starting or evaluating the search.
/// </summary>
public sealed record FocusedSearchRequest
{
    /// <summary>
    /// Creates an immutable focused-search request for an existing searcher and target object.
    /// </summary>
    public FocusedSearchRequest(CharacterId searcherCharacterId, ObjectId targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(searcherCharacterId);
        ArgumentNullException.ThrowIfNull(targetObjectId);

        SearcherCharacterId = searcherCharacterId;
        TargetObjectId = targetObjectId;
    }

    /// <summary>
    /// Gets the stable identifier of the character requesting the search.
    /// </summary>
    public CharacterId SearcherCharacterId { get; }

    /// <summary>
    /// Gets the stable identifier of the object targeted by the search.
    /// </summary>
    public ObjectId TargetObjectId { get; }
}
