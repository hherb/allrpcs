namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies one completed local character turn without attaching mutable character state.
/// </summary>
public sealed record CharacterTurnId
{
    /// <summary>Creates a character-turn identifier from canonical text.</summary>
    public CharacterTurnId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a character-turn identifier.</summary>
    public static CharacterTurnId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
