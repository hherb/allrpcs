using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Locations;

/// <summary>
/// Identifies a location by a stable typed identifier and an authored display name.
/// </summary>
public sealed record LocationIdentity
{
    /// <summary>
    /// Gets the maximum supported authored display-name length.
    /// </summary>
    public const int MaximumDisplayNameLength = 80;

    /// <summary>
    /// Creates an immutable location identity.
    /// </summary>
    public LocationIdentity(LocationId id, string displayName)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        DisplayName = AuthoredLocationText.RequireNonBlank(
            displayName,
            MaximumDisplayNameLength,
            nameof(displayName));
    }

    /// <summary>
    /// Gets the location's stable identifier.
    /// </summary>
    public LocationId Id { get; }

    /// <summary>
    /// Gets the authored display name exactly as supplied.
    /// </summary>
    public string DisplayName { get; }
}
