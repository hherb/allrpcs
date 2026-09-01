using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Contains a character's ordered, duplicate-free references to knowledge.
/// </summary>
public sealed class CharacterKnowledge
{
    /// <summary>
    /// Creates character knowledge. An empty sequence explicitly means the character references no knowledge.
    /// </summary>
    public CharacterKnowledge(IEnumerable<KnowledgeId> knowledgeReferences)
    {
        ArgumentNullException.ThrowIfNull(knowledgeReferences);

        List<KnowledgeId> orderedReferences = new();
        HashSet<KnowledgeId> identifiers = new();

        foreach (KnowledgeId knowledgeReference in knowledgeReferences)
        {
            if (knowledgeReference is null)
            {
                throw new ArgumentException(
                    "Knowledge references cannot contain null values.",
                    nameof(knowledgeReferences));
            }

            if (!identifiers.Add(knowledgeReference))
            {
                throw new ArgumentException(
                    "Knowledge references cannot contain duplicate identifiers.",
                    nameof(knowledgeReferences));
            }

            orderedReferences.Add(knowledgeReference);
        }

        KnowledgeReferences = orderedReferences.AsReadOnly();
    }

    /// <summary>
    /// Gets stable knowledge references in their authored order.
    /// </summary>
    public IReadOnlyList<KnowledgeId> KnowledgeReferences { get; }
}
