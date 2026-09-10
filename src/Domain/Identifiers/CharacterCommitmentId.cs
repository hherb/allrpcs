namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one unfinished character commitment.</summary>
public sealed record CharacterCommitmentId
{
    /// <summary>Creates a character-commitment identifier from canonical text.</summary>
    public CharacterCommitmentId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a character-commitment identifier.</summary>
    public static CharacterCommitmentId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
