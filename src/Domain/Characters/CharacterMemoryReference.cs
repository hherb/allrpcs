using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains exactly one category-safe reference to an existing local character memory.</summary>
public sealed record CharacterMemoryReference
{
    /// <summary>Creates one immutable discriminated memory reference.</summary>
    public CharacterMemoryReference(
        CharacterMemoryCategory category,
        CharacterEpisodicMemory? episodicMemory,
        CharacterEpistemicMemory? epistemicMemory,
        CharacterRelationshipMemory? relationshipMemory,
        CharacterCommitmentMemory? commitmentMemory)
    {
        ArgumentNullException.ThrowIfNull(category);
        int suppliedCount = CountSupplied(
            episodicMemory,
            epistemicMemory,
            relationshipMemory,
            commitmentMemory);
        bool categoryMatches =
            (ReferenceEquals(category, CharacterMemoryCategory.Episodic) && episodicMemory is not null)
            || (ReferenceEquals(category, CharacterMemoryCategory.Epistemic) && epistemicMemory is not null)
            || (ReferenceEquals(category, CharacterMemoryCategory.Relationship) && relationshipMemory is not null)
            || (ReferenceEquals(category, CharacterMemoryCategory.Commitment) && commitmentMemory is not null);
        if (suppliedCount != 1 || !categoryMatches)
        {
            throw new ArgumentException(
                "A memory reference requires exactly one memory matching its category.",
                nameof(category));
        }

        Category = category;
        EpisodicMemory = episodicMemory;
        EpistemicMemory = epistemicMemory;
        RelationshipMemory = relationshipMemory;
        CommitmentMemory = commitmentMemory;
        CharacterId = GetCharacterId(
            episodicMemory,
            epistemicMemory,
            relationshipMemory,
            commitmentMemory);
    }

    /// <summary>Gets the exact closed referenced-memory category.</summary>
    public CharacterMemoryCategory Category { get; }

    /// <summary>Gets the exact owner identity of the referenced memory.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact episodic memory only for an episodic reference.</summary>
    public CharacterEpisodicMemory? EpisodicMemory { get; }

    /// <summary>Gets the exact epistemic memory only for an epistemic reference.</summary>
    public CharacterEpistemicMemory? EpistemicMemory { get; }

    /// <summary>Gets the exact relationship memory only for a relationship reference.</summary>
    public CharacterRelationshipMemory? RelationshipMemory { get; }

    /// <summary>Gets the exact commitment memory only for a commitment reference.</summary>
    public CharacterCommitmentMemory? CommitmentMemory { get; }

    internal string IdentityValue => Category switch
    {
        _ when ReferenceEquals(Category, CharacterMemoryCategory.Episodic) =>
            EpisodicMemory!.MemoryId.Value,
        _ when ReferenceEquals(Category, CharacterMemoryCategory.Epistemic) =>
            EpistemicMemory!.MemoryId.Value,
        _ when ReferenceEquals(Category, CharacterMemoryCategory.Relationship) =>
            RelationshipMemory!.RelatedCharacterId.Value,
        _ => CommitmentMemory!.CommitmentId.Value,
    };

    private static int CountSupplied(
        CharacterEpisodicMemory? episodicMemory,
        CharacterEpistemicMemory? epistemicMemory,
        CharacterRelationshipMemory? relationshipMemory,
        CharacterCommitmentMemory? commitmentMemory) =>
        (episodicMemory is null ? 0 : 1)
        + (epistemicMemory is null ? 0 : 1)
        + (relationshipMemory is null ? 0 : 1)
        + (commitmentMemory is null ? 0 : 1);

    private static CharacterId GetCharacterId(
        CharacterEpisodicMemory? episodicMemory,
        CharacterEpistemicMemory? epistemicMemory,
        CharacterRelationshipMemory? relationshipMemory,
        CharacterCommitmentMemory? commitmentMemory) =>
        episodicMemory?.CharacterId
        ?? epistemicMemory?.CharacterId
        ?? relationshipMemory?.CharacterId
        ?? commitmentMemory!.CharacterId;
}
