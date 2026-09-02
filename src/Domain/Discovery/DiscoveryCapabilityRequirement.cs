using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Associates one world object with the character capability required to discover it.
/// </summary>
public sealed record DiscoveryCapabilityRequirement
{
    /// <summary>
    /// Creates an immutable discovery requirement for an existing object and canonical
    /// character-capability key.
    /// </summary>
    public DiscoveryCapabilityRequirement(ObjectId objectId, string requiredCapabilityKey)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        ObjectId = objectId;
        RequiredCapabilityKey = CanonicalIdentifierText.RequireCanonical(
            requiredCapabilityKey,
            nameof(requiredCapabilityKey));
    }

    /// <summary>
    /// Gets the identifier of the object governed by this discovery requirement.
    /// </summary>
    public ObjectId ObjectId { get; }

    /// <summary>
    /// Gets the canonical key of the character capability required for discovery.
    /// </summary>
    public string RequiredCapabilityKey { get; }
}
