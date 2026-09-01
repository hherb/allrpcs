namespace Allrpcs.Domain.Locations;

/// <summary>
/// Composes the complete immutable authored location model without defining runtime behavior.
/// </summary>
public sealed class Location
{
    /// <summary>
    /// Creates an immutable location from its existing validated components.
    /// </summary>
    public Location(LocationIdentity identity, LocationDescription description)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(description);

        Identity = identity;
        Description = description;
    }

    /// <summary>
    /// Gets the location's stable identity.
    /// </summary>
    public LocationIdentity Identity { get; }

    /// <summary>
    /// Gets the location's authoritative authored description.
    /// </summary>
    public LocationDescription Description { get; }
}
