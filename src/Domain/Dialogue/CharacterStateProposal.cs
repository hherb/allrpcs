using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Declares bounded character-owned state text for later validation without applying it.
/// </summary>
public sealed record CharacterStateProposal
{
    /// <summary>Gets the maximum supported proposed value length.</summary>
    public const int MaximumValueLength = 500;

    /// <summary>Creates an immutable proposal from a canonical key and exact bounded value.</summary>
    public CharacterStateProposal(string key, string value)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Value = AuthoredCharacterText.RequireNonBlank(
            value,
            MaximumValueLength,
            nameof(value));
    }

    /// <summary>Gets the stable canonical proposal key.</summary>
    public string Key { get; }

    /// <summary>Gets the proposed value exactly as supplied.</summary>
    public string Value { get; }
}
