using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Knowledge;

/// <summary>
/// Associates one knowledge identifier with exact authored content without adding belief semantics.
/// </summary>
public sealed record KnowledgeEntry
{
    /// <summary>Gets the maximum supported authored knowledge-text length.</summary>
    public const int MaximumTextLength = 4_000;

    /// <summary>Creates one immutable authored knowledge entry.</summary>
    public KnowledgeEntry(KnowledgeId id, string text)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text) || text.Length > MaximumTextLength)
        {
            throw new ArgumentException(
                $"Knowledge text must be nonblank and at most {MaximumTextLength} UTF-16 code units.",
                nameof(text));
        }

        Id = id;
        Text = text;
    }

    /// <summary>Gets the exact stable identifier.</summary>
    public KnowledgeId Id { get; }

    /// <summary>Gets the exact authored knowledge text.</summary>
    public string Text { get; }
}
