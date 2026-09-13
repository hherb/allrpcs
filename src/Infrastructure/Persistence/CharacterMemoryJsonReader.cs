using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly materializes canonical provider-neutral version-2 character memory.</summary>
internal static class CharacterMemoryJsonReader
{
    /// <summary>Reads every exact character-memory aggregate from its closed document shape.</summary>
    internal static CharacterMemorySnapshot Read(
        JsonElement element,
        bool includesDialogueDeliveryMode)
    {
        SaveSnapshotJsonDeserializer.RequireObject(element, "characters");
        JsonElement charactersElement = element.GetProperty("characters");
        SaveSnapshotJsonDeserializer.RequireArray(charactersElement);
        List<CharacterMemoryAggregate> characters = new();
        foreach (JsonElement characterElement in charactersElement.EnumerateArray())
        {
            characters.Add(ReadAggregate(characterElement, includesDialogueDeliveryMode));
        }

        return new CharacterMemorySnapshot(characters);
    }

    private static CharacterMemoryAggregate ReadAggregate(
        JsonElement element,
        bool includesDialogueDeliveryMode)
    {
        SaveSnapshotJsonDeserializer.RequireObject(
            element,
            "characterId",
            "recentConversation",
            "conversationSummaries",
            "episodicMemories",
            "epistemicMemories",
            "beliefAssessments",
            "relationships",
            "commitments",
            "memoryIndex");
        CharacterId characterId = new(ReadString(element, "characterId"));
        Allrpcs.Domain.Dialogue.CharacterRecentConversationWindow recentConversation =
            CharacterRecentConversationJsonReader.Read(
                element.GetProperty("recentConversation"),
                characterId,
                includesDialogueDeliveryMode);
        CharacterConversationSummarySnapshot summaries = ReadConversationSummaries(
            element.GetProperty("conversationSummaries"),
            characterId);
        CharacterEpisodicMemorySnapshot episodic = ReadEpisodicMemories(
            element.GetProperty("episodicMemories"),
            characterId);
        CharacterEpistemicMemorySnapshot epistemic = ReadEpistemicMemories(
            element.GetProperty("epistemicMemories"),
            characterId);
        CharacterBeliefAssessmentSnapshot assessments = ReadBeliefAssessments(
            element.GetProperty("beliefAssessments"),
            characterId,
            epistemic);
        CharacterRelationshipMemorySnapshot relationships = ReadRelationships(
            element.GetProperty("relationships"),
            characterId);
        CharacterCommitmentMemorySnapshot commitments = ReadCommitments(
            element.GetProperty("commitments"),
            characterId);
        CharacterMemoryIndex index = ReadMemoryIndex(
            element.GetProperty("memoryIndex"),
            characterId,
            episodic,
            epistemic,
            relationships,
            commitments);
        return new CharacterMemoryAggregate(
            characterId,
            recentConversation,
            summaries,
            episodic,
            epistemic,
            assessments,
            relationships,
            commitments,
            index);
    }

    private static CharacterConversationSummarySnapshot ReadConversationSummaries(
        JsonElement element,
        CharacterId characterId)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterConversationSummary> summaries = new();
        foreach (JsonElement summaryElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                summaryElement,
                "summaryId",
                "sourceTurnIds",
                "text");
            summaries.Add(
                new CharacterConversationSummary(
                    new CharacterConversationSummaryId(ReadString(summaryElement, "summaryId")),
                    characterId,
                    ReadIdentifiers(
                        summaryElement.GetProperty("sourceTurnIds"),
                        value => new CharacterTurnId(value)),
                    ReadString(summaryElement, "text")));
        }

        return new CharacterConversationSummarySnapshot(characterId, summaries);
    }

    private static CharacterEpisodicMemorySnapshot ReadEpisodicMemories(
        JsonElement element,
        CharacterId characterId)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterEpisodicMemory> memories = new();
        foreach (JsonElement memoryElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                memoryElement,
                "memoryId",
                "participantCharacterIds",
                "locationId",
                "currentTimeEventId",
                "sourceTurnId",
                "summary");
            string? timeEventId = ReadNullableString(memoryElement.GetProperty("currentTimeEventId"));
            memories.Add(
                new CharacterEpisodicMemory(
                    new EpisodicMemoryId(ReadString(memoryElement, "memoryId")),
                    characterId,
                    ReadIdentifiers(
                        memoryElement.GetProperty("participantCharacterIds"),
                        value => new CharacterId(value)),
                    new LocationId(ReadString(memoryElement, "locationId")),
                    new WorldTimeSnapshot(
                        timeEventId is null ? null : new WorldEventId(timeEventId)),
                    new CharacterTurnId(ReadString(memoryElement, "sourceTurnId")),
                    ReadString(memoryElement, "summary")));
        }

        return new CharacterEpisodicMemorySnapshot(characterId, memories);
    }

    private static CharacterEpistemicMemorySnapshot ReadEpistemicMemories(
        JsonElement element,
        CharacterId characterId)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterEpistemicMemory> memories = new();
        foreach (JsonElement memoryElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                memoryElement,
                "memoryId",
                "category",
                "sourceTurnId",
                "statement",
                "attributedSpeakerCharacterId");
            string? speakerId = ReadNullableString(
                memoryElement.GetProperty("attributedSpeakerCharacterId"));
            memories.Add(
                new CharacterEpistemicMemory(
                    new EpistemicMemoryId(ReadString(memoryElement, "memoryId")),
                    characterId,
                    CharacterEpistemicMemoryCategory.Parse(ReadString(memoryElement, "category")),
                    new CharacterTurnId(ReadString(memoryElement, "sourceTurnId")),
                    ReadString(memoryElement, "statement"),
                    speakerId is null ? null : new CharacterId(speakerId)));
        }

        return new CharacterEpistemicMemorySnapshot(characterId, memories);
    }

    private static CharacterBeliefAssessmentSnapshot ReadBeliefAssessments(
        JsonElement element,
        CharacterId characterId,
        CharacterEpistemicMemorySnapshot epistemic)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterBeliefAssessment> assessments = new();
        foreach (JsonElement assessmentElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                assessmentElement,
                "beliefMemoryId",
                "confidenceBasisPoints",
                "supportingEvidenceIds",
                "contradictingEvidenceIds");
            string beliefMemoryId = ReadString(assessmentElement, "beliefMemoryId");
            CharacterEpistemicMemory belief = epistemic.Memories.SingleOrDefault(
                memory => string.Equals(
                    memory.MemoryId.Value,
                    beliefMemoryId,
                    StringComparison.Ordinal)) ?? throw SaveSnapshotJsonDeserializer.InvalidDocument();
            assessments.Add(
                new CharacterBeliefAssessment(
                    belief,
                    SaveSnapshotJsonDeserializer.ReadInt32(
                        assessmentElement.GetProperty("confidenceBasisPoints")),
                    ReadIdentifiers(
                        assessmentElement.GetProperty("supportingEvidenceIds"),
                        value => new EvidenceId(value)),
                    ReadIdentifiers(
                        assessmentElement.GetProperty("contradictingEvidenceIds"),
                        value => new EvidenceId(value))));
        }

        return new CharacterBeliefAssessmentSnapshot(characterId, assessments);
    }

    private static CharacterRelationshipMemorySnapshot ReadRelationships(
        JsonElement element,
        CharacterId characterId)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterRelationshipMemory> relationships = new();
        foreach (JsonElement relationshipElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                relationshipElement,
                "relatedCharacterId",
                "dimensions",
                "reasons");
            relationships.Add(
                new CharacterRelationshipMemory(
                    characterId,
                    new CharacterId(ReadString(relationshipElement, "relatedCharacterId")),
                    ReadDimensions(relationshipElement.GetProperty("dimensions")),
                    ReadReasons(relationshipElement.GetProperty("reasons"))));
        }

        return new CharacterRelationshipMemorySnapshot(characterId, relationships);
    }

    private static List<CharacterRelationshipDimensionValue> ReadDimensions(
        JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterRelationshipDimensionValue> dimensions = new();
        foreach (JsonElement dimensionElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(dimensionElement, "dimension", "value");
            dimensions.Add(
                new CharacterRelationshipDimensionValue(
                    CharacterRelationshipDimension.Parse(ReadString(dimensionElement, "dimension")),
                    SaveSnapshotJsonDeserializer.ReadInt32(dimensionElement.GetProperty("value"))));
        }

        return dimensions;
    }

    private static List<CharacterRelationshipReason> ReadReasons(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterRelationshipReason> reasons = new();
        foreach (JsonElement reasonElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                reasonElement,
                "reasonId",
                "dimension",
                "sourceTurnId",
                "summary");
            reasons.Add(
                new CharacterRelationshipReason(
                    new RelationshipReasonId(ReadString(reasonElement, "reasonId")),
                    CharacterRelationshipDimension.Parse(ReadString(reasonElement, "dimension")),
                    new CharacterTurnId(ReadString(reasonElement, "sourceTurnId")),
                    ReadString(reasonElement, "summary")));
        }

        return reasons;
    }

    private static CharacterCommitmentMemorySnapshot ReadCommitments(
        JsonElement element,
        CharacterId characterId)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterCommitmentMemory> commitments = new();
        foreach (JsonElement commitmentElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                commitmentElement,
                "commitmentId",
                "kind",
                "counterpartyCharacterId",
                "sourceTurnId",
                "description");
            commitments.Add(
                new CharacterCommitmentMemory(
                    new CharacterCommitmentId(ReadString(commitmentElement, "commitmentId")),
                    characterId,
                    CharacterCommitmentKind.Parse(ReadString(commitmentElement, "kind")),
                    new CharacterId(ReadString(commitmentElement, "counterpartyCharacterId")),
                    new CharacterTurnId(ReadString(commitmentElement, "sourceTurnId")),
                    ReadString(commitmentElement, "description")));
        }

        return new CharacterCommitmentMemorySnapshot(characterId, commitments);
    }

    private static CharacterMemoryIndex ReadMemoryIndex(
        JsonElement element,
        CharacterId characterId,
        CharacterEpisodicMemorySnapshot episodic,
        CharacterEpistemicMemorySnapshot epistemic,
        CharacterRelationshipMemorySnapshot relationships,
        CharacterCommitmentMemorySnapshot commitments)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterMemoryIndexEntry> entries = new();
        foreach (JsonElement entryElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                entryElement,
                "category",
                "memoryIdentity",
                "tags");
            CharacterMemoryCategory category = CharacterMemoryCategory.Parse(
                ReadString(entryElement, "category"));
            string identity = ReadString(entryElement, "memoryIdentity");
            CharacterMemoryReference reference = ResolveReference(
                category,
                identity,
                episodic,
                epistemic,
                relationships,
                commitments);
            JsonElement tagsElement = entryElement.GetProperty("tags");
            SaveSnapshotJsonDeserializer.RequireArray(tagsElement);
            List<CharacterMemoryTag> tags = new();
            foreach (JsonElement tagElement in tagsElement.EnumerateArray())
            {
                SaveSnapshotJsonDeserializer.RequireObject(tagElement, "kind", "value");
                tags.Add(
                    new CharacterMemoryTag(
                        CharacterMemoryTagKind.Parse(ReadString(tagElement, "kind")),
                        ReadString(tagElement, "value")));
            }

            entries.Add(new CharacterMemoryIndexEntry(reference, tags));
        }

        return new CharacterMemoryIndex(characterId, entries);
    }

    private static CharacterMemoryReference ResolveReference(
        CharacterMemoryCategory category,
        string identity,
        CharacterEpisodicMemorySnapshot episodic,
        CharacterEpistemicMemorySnapshot epistemic,
        CharacterRelationshipMemorySnapshot relationships,
        CharacterCommitmentMemorySnapshot commitments)
    {
        CharacterEpisodicMemory? episodicMemory = ReferenceEquals(
            category,
            CharacterMemoryCategory.Episodic)
            ? episodic.Memories.SingleOrDefault(memory => memory.MemoryId.Value == identity)
            : null;
        CharacterEpistemicMemory? epistemicMemory = ReferenceEquals(
            category,
            CharacterMemoryCategory.Epistemic)
            ? epistemic.Memories.SingleOrDefault(memory => memory.MemoryId.Value == identity)
            : null;
        CharacterRelationshipMemory? relationshipMemory = ReferenceEquals(
            category,
            CharacterMemoryCategory.Relationship)
            ? relationships.Relationships.SingleOrDefault(
                memory => memory.RelatedCharacterId.Value == identity)
            : null;
        CharacterCommitmentMemory? commitmentMemory = ReferenceEquals(
            category,
            CharacterMemoryCategory.Commitment)
            ? commitments.Commitments.SingleOrDefault(
                memory => memory.CommitmentId.Value == identity)
            : null;
        if (episodicMemory is null
            && epistemicMemory is null
            && relationshipMemory is null
            && commitmentMemory is null)
        {
            throw SaveSnapshotJsonDeserializer.InvalidDocument();
        }

        return new CharacterMemoryReference(
            category,
            episodicMemory,
            epistemicMemory,
            relationshipMemory,
            commitmentMemory);
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        SaveSnapshotJsonDeserializer.ReadString(element.GetProperty(propertyName));

    private static string? ReadNullableString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => SaveSnapshotJsonDeserializer.ReadString(element),
        _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
    };

    private static List<T> ReadIdentifiers<T>(
        JsonElement element,
        Func<string, T> create)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<T> identifiers = new();
        foreach (JsonElement identifierElement in element.EnumerateArray())
        {
            identifiers.Add(create(SaveSnapshotJsonDeserializer.ReadString(identifierElement)));
        }

        return identifiers;
    }
}
