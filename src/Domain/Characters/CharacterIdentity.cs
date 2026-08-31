using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Identifies a character by a stable typed identifier and an authored display name.
/// </summary>
public sealed record CharacterIdentity
{
    /// <summary>
    /// Gets the maximum supported authored display-name length.
    /// </summary>
    public const int MaximumDisplayNameLength = 80;

    /// <summary>
    /// Creates an immutable character identity.
    /// </summary>
    public CharacterIdentity(CharacterId id, string displayName)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        DisplayName = AuthoredCharacterText.RequireNonBlank(
            displayName,
            MaximumDisplayNameLength,
            nameof(displayName));
    }

    /// <summary>
    /// Gets the character's stable identifier.
    /// </summary>
    public CharacterId Id { get; }

    /// <summary>
    /// Gets the authored display name exactly as supplied.
    /// </summary>
    public string DisplayName { get; }
}
