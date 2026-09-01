using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Perception;

/// <summary>
/// Associates a world object with one visibility state without assigning the association to a
/// character or defining perception mechanics.
/// </summary>
public sealed record WorldObjectVisibility
{
    /// <summary>
    /// Creates an immutable world-object visibility association.
    /// </summary>
    public WorldObjectVisibility(ObjectId objectId, VisibilityState visibilityState)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(visibilityState);

        ObjectId = objectId;
        VisibilityState = visibilityState;
    }

    /// <summary>
    /// Gets the stable identifier of the associated world object.
    /// </summary>
    public ObjectId ObjectId { get; }

    /// <summary>
    /// Gets the visibility state associated with the world object.
    /// </summary>
    public VisibilityState VisibilityState { get; }
}
