using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Associates another character with an authored relationship summary.
/// </summary>
public sealed record CharacterRelationship
{
    /// <summary>
    /// Gets the maximum supported authored summary length.
    /// </summary>
    public const int MaximumSummaryLength = 500;

    /// <summary>
    /// Creates an immutable relationship to another character.
    /// </summary>
    public CharacterRelationship(CharacterId relatedCharacterId, string summary)
    {
        ArgumentNullException.ThrowIfNull(relatedCharacterId);

        RelatedCharacterId = relatedCharacterId;
        Summary = AuthoredCharacterText.RequireNonBlank(
            summary,
            MaximumSummaryLength,
            nameof(summary));
    }

    /// <summary>
    /// Gets the stable identifier of the related character.
    /// </summary>
    public CharacterId RelatedCharacterId { get; }

    /// <summary>
    /// Gets the authored relationship summary exactly as supplied.
    /// </summary>
    public string Summary { get; }
}
