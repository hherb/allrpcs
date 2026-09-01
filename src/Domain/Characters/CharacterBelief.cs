using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Describes an authored character belief without classifying it as authoritative knowledge.
/// </summary>
public sealed record CharacterBelief
{
    /// <summary>
    /// Gets the maximum supported authored statement length.
    /// </summary>
    public const int MaximumStatementLength = 500;

    /// <summary>
    /// Creates an immutable belief from a canonical key and authored statement.
    /// </summary>
    public CharacterBelief(string key, string statement)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Statement = AuthoredCharacterText.RequireNonBlank(
            statement,
            MaximumStatementLength,
            nameof(statement));
    }

    /// <summary>
    /// Gets the stable canonical belief key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored belief statement exactly as supplied.
    /// </summary>
    public string Statement { get; }
}
