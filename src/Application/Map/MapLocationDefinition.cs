using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Defines one safe player-facing opening-region map location.</summary>
public sealed class MapLocationDefinition
{
    /// <summary>Gets the maximum accepted display-label length in UTF-16 code units.</summary>
    public const int MaximumDisplayLabelLength = 80;

    /// <summary>Creates one immutable map-location definition.</summary>
    public MapLocationDefinition(LocationId id, string displayLabel)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(displayLabel);

        if (string.IsNullOrWhiteSpace(displayLabel))
        {
            throw new ArgumentException(
                "A map-location display label must contain non-whitespace text.",
                nameof(displayLabel));
        }

        if (displayLabel.Length > MaximumDisplayLabelLength)
        {
            throw new ArgumentOutOfRangeException(nameof(displayLabel));
        }

        Id = id;
        DisplayLabel = displayLabel;
    }

    /// <summary>Gets the exact canonical location identifier.</summary>
    public LocationId Id { get; }

    /// <summary>Gets the exact authored player-facing label.</summary>
    public string DisplayLabel { get; }
}
