using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Associates one world object with the tool object required to discover it.
/// </summary>
public sealed record DiscoveryToolRequirement
{
    /// <summary>
    /// Creates an immutable discovery requirement for an existing target object and required
    /// tool object.
    /// </summary>
    public DiscoveryToolRequirement(ObjectId objectId, ObjectId requiredToolObjectId)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(requiredToolObjectId);

        ObjectId = objectId;
        RequiredToolObjectId = requiredToolObjectId;
    }

    /// <summary>
    /// Gets the identifier of the object governed by this discovery requirement.
    /// </summary>
    public ObjectId ObjectId { get; }

    /// <summary>
    /// Gets the identifier of the tool object required for discovery.
    /// </summary>
    public ObjectId RequiredToolObjectId { get; }
}
