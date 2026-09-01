using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Identifies a world object by a stable typed identifier and an authored display name.
/// </summary>
public sealed record WorldObjectIdentity
{
    /// <summary>
    /// Gets the maximum supported authored display-name length.
    /// </summary>
    public const int MaximumDisplayNameLength = 80;

    /// <summary>
    /// Creates an immutable world-object identity.
    /// </summary>
    public WorldObjectIdentity(ObjectId id, string displayName)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        DisplayName = AuthoredWorldObjectText.RequireNonBlank(
            displayName,
            MaximumDisplayNameLength,
            nameof(displayName));
    }

    /// <summary>
    /// Gets the world object's stable identifier.
    /// </summary>
    public ObjectId Id { get; }

    /// <summary>
    /// Gets the authored display name exactly as supplied.
    /// </summary>
    public string DisplayName { get; }
}
