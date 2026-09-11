namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one retained older-conversation summary.</summary>
public sealed record CharacterConversationSummaryId
{
    /// <summary>Creates a summary identifier from canonical text.</summary>
    public CharacterConversationSummaryId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical identifier text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a summary identifier.</summary>
    public static CharacterConversationSummaryId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
