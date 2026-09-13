namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one bounded independent character task.</summary>
public sealed record CharacterIndependentTaskId
{
    /// <summary>Creates an independent-task identifier from canonical text.</summary>
    public CharacterIndependentTaskId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an independent-task identifier.</summary>
    public static CharacterIndependentTaskId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
