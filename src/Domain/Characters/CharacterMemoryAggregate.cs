using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains all canonical provider-neutral local memory for one character.</summary>
public sealed class CharacterMemoryAggregate
{
    /// <summary>Creates one immutable per-character memory aggregate.</summary>
    public CharacterMemoryAggregate(
        CharacterId characterId,
        CharacterRecentConversationWindow recentConversation,
        CharacterConversationSummarySnapshot conversationSummaries,
        CharacterEpisodicMemorySnapshot episodicMemories,
        CharacterEpistemicMemorySnapshot epistemicMemories,
        CharacterBeliefAssessmentSnapshot beliefAssessments,
        CharacterRelationshipMemorySnapshot relationships,
        CharacterCommitmentMemorySnapshot commitments,
        CharacterMemoryIndex memoryIndex)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(recentConversation);
        ArgumentNullException.ThrowIfNull(conversationSummaries);
        ArgumentNullException.ThrowIfNull(episodicMemories);
        ArgumentNullException.ThrowIfNull(epistemicMemories);
        ArgumentNullException.ThrowIfNull(beliefAssessments);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(commitments);
        ArgumentNullException.ThrowIfNull(memoryIndex);

        if (recentConversation.CharacterId != characterId
            || conversationSummaries.CharacterId != characterId
            || episodicMemories.CharacterId != characterId
            || epistemicMemories.CharacterId != characterId
            || beliefAssessments.CharacterId != characterId
            || relationships.CharacterId != characterId
            || commitments.CharacterId != characterId
            || memoryIndex.CharacterId != characterId)
        {
            throw new ArgumentException(
                "Every memory constituent must belong to the aggregate character.",
                nameof(characterId));
        }

        foreach (CharacterMemoryIndexEntry entry in memoryIndex.Entries)
        {
            if (!RetainsExactMemory(
                    entry.MemoryReference,
                    episodicMemories,
                    epistemicMemories,
                    relationships,
                    commitments))
            {
                throw new ArgumentException(
                    "Every index reference must retain an exact memory from its category snapshot.",
                    nameof(memoryIndex));
            }
        }

        CharacterId = characterId;
        RecentConversation = recentConversation;
        ConversationSummaries = conversationSummaries;
        EpisodicMemories = episodicMemories;
        EpistemicMemories = epistemicMemories;
        BeliefAssessments = beliefAssessments;
        Relationships = relationships;
        Commitments = commitments;
        MemoryIndex = memoryIndex;
    }

    /// <summary>Gets the exact character identity supplied for this aggregate.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact recent local conversation window.</summary>
    public CharacterRecentConversationWindow RecentConversation { get; }

    /// <summary>Gets the exact older-conversation summary snapshot.</summary>
    public CharacterConversationSummarySnapshot ConversationSummaries { get; }

    /// <summary>Gets the exact episodic-memory snapshot.</summary>
    public CharacterEpisodicMemorySnapshot EpisodicMemories { get; }

    /// <summary>Gets the exact epistemic-memory snapshot.</summary>
    public CharacterEpistemicMemorySnapshot EpistemicMemories { get; }

    /// <summary>Gets the exact belief-assessment snapshot.</summary>
    public CharacterBeliefAssessmentSnapshot BeliefAssessments { get; }

    /// <summary>Gets the exact relationship-memory snapshot.</summary>
    public CharacterRelationshipMemorySnapshot Relationships { get; }

    /// <summary>Gets the exact unfinished-commitment snapshot.</summary>
    public CharacterCommitmentMemorySnapshot Commitments { get; }

    /// <summary>Gets the exact deterministic local memory index.</summary>
    public CharacterMemoryIndex MemoryIndex { get; }

    private static bool RetainsExactMemory(
        CharacterMemoryReference memoryReference,
        CharacterEpisodicMemorySnapshot episodicMemories,
        CharacterEpistemicMemorySnapshot epistemicMemories,
        CharacterRelationshipMemorySnapshot relationships,
        CharacterCommitmentMemorySnapshot commitments) => memoryReference.Category switch
        {
            _ when ReferenceEquals(memoryReference.Category, CharacterMemoryCategory.Episodic) =>
                episodicMemories.Memories.Any(memory =>
                    ReferenceEquals(memory, memoryReference.EpisodicMemory)),
            _ when ReferenceEquals(memoryReference.Category, CharacterMemoryCategory.Epistemic) =>
                epistemicMemories.Memories.Any(memory =>
                    ReferenceEquals(memory, memoryReference.EpistemicMemory)),
            _ when ReferenceEquals(memoryReference.Category, CharacterMemoryCategory.Relationship) =>
                relationships.Relationships.Any(memory =>
                    ReferenceEquals(memory, memoryReference.RelationshipMemory)),
            _ => commitments.Commitments.Any(memory =>
                ReferenceEquals(memory, memoryReference.CommitmentMemory)),
        };
}
