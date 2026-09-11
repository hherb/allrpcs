using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one bounded canonical deterministic memory-retrieval tag.</summary>
public sealed record CharacterMemoryTag
{
    /// <summary>Gets the maximum canonical tag-value length in UTF-16 code units.</summary>
    public const int MaximumValueLength = CanonicalIdentifierText.MaximumLength;

    /// <summary>Creates one immutable typed retrieval tag.</summary>
    public CharacterMemoryTag(CharacterMemoryTagKind kind, string value)
    {
        ArgumentNullException.ThrowIfNull(kind);
        Kind = kind;
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the exact closed tag kind.</summary>
    public CharacterMemoryTagKind Kind { get; }

    /// <summary>Gets the canonical tag value.</summary>
    public string Value { get; }
}
