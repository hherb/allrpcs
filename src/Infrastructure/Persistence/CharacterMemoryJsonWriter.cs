using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes canonical provider-neutral character memory inside a versioned save document.</summary>
internal static class CharacterMemoryJsonWriter
{
    /// <summary>Writes every aggregate in stable supplied order.</summary>
    internal static void Write(
        Utf8JsonWriter writer,
        CharacterMemorySnapshot snapshot,
        bool includeDialogueDeliveryMode)
    {
        writer.WritePropertyName("characterMemory");
        writer.WriteStartObject();
        writer.WritePropertyName("characters");
        writer.WriteStartArray();
        foreach (CharacterMemoryAggregate aggregate in snapshot.Characters)
        {
            WriteAggregate(writer, aggregate, includeDialogueDeliveryMode);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteAggregate(
        Utf8JsonWriter writer,
        CharacterMemoryAggregate aggregate,
        bool includeDialogueDeliveryMode)
    {
        writer.WriteStartObject();
        writer.WriteString("characterId", aggregate.CharacterId.Value);
        WriteRecentConversation(
            writer,
            aggregate.RecentConversation,
            includeDialogueDeliveryMode);
        WriteConversationSummaries(writer, aggregate.ConversationSummaries);
        WriteEpisodicMemories(writer, aggregate.EpisodicMemories);
        WriteEpistemicMemories(writer, aggregate.EpistemicMemories);
        WriteBeliefAssessments(writer, aggregate.BeliefAssessments);
        WriteRelationships(writer, aggregate.Relationships);
        WriteCommitments(writer, aggregate.Commitments);
        WriteMemoryIndex(writer, aggregate.MemoryIndex);
        writer.WriteEndObject();
    }

    private static void WriteRecentConversation(
        Utf8JsonWriter writer,
        CharacterRecentConversationWindow window,
        bool includeDialogueDeliveryMode)
    {
        writer.WritePropertyName("recentConversation");
        writer.WriteStartArray();
        foreach (CharacterRecentConversationTurn turn in window.Turns)
        {
            writer.WriteStartObject();
            writer.WriteString("turnId", turn.TurnId.Value);
            WriteHeardDialogue(
                writer,
                turn.Activity.Request,
                includeDialogueDeliveryMode);
            WriteResponse(
                writer,
                turn.Activity.Response!,
                includeDialogueDeliveryMode);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteHeardDialogue(
        Utf8JsonWriter writer,
        CharacterTurnRequest request,
        bool includeDialogueDeliveryMode)
    {
        DialogueUtterance utterance = request.DialogueEvent.Utterance;
        writer.WritePropertyName("heard");
        writer.WriteStartObject();
        writer.WriteString("speakerCharacterId", utterance.SpeakerCharacterId.Value);
        if (includeDialogueDeliveryMode)
        {
            writer.WriteString("deliveryMode", request.DialogueEvent.Delivery.Mode.Value);
        }

        writer.WritePropertyName("listenerCharacterIds");
        writer.WriteStartArray();
        foreach (CharacterId listenerId in request.DialogueEvent.Audience.ListenerCharacterIds)
        {
            writer.WriteStringValue(listenerId.Value);
        }

        writer.WriteEndArray();
        writer.WriteString("text", utterance.Text);
        writer.WriteString("dialogueAct", request.DialogueAct.Type.Value);
        writer.WriteEndObject();
    }

    private static void WriteResponse(
        Utf8JsonWriter writer,
        CharacterTurnResponse response,
        bool includeDialogueDeliveryMode)
    {
        writer.WritePropertyName("response");
        writer.WriteStartObject();
        writer.WriteString("disposition", response.Disposition.Value);
        writer.WriteString("speech", response.Speech?.Text);
        if (includeDialogueDeliveryMode)
        {
            writer.WriteString("deliveryMode", response.Delivery?.Mode.Value);
        }

        writer.WritePropertyName("immediateIntention");
        WriteIntention(writer, response.ImmediateIntention);
        writer.WritePropertyName("commitments");
        writer.WriteStartArray();
        foreach (CharacterCommitment commitment in response.Commitments)
        {
            writer.WriteStartArray();
            foreach (CharacterIntention intention in commitment.ProposedIntentions)
            {
                WriteIntention(writer, intention);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
        writer.WritePropertyName("stateProposals");
        writer.WriteStartArray();
        foreach (CharacterStateProposal proposal in response.StateProposals)
        {
            writer.WriteStartObject();
            writer.WriteString("key", proposal.Key);
            writer.WriteString("value", proposal.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteIntention(Utf8JsonWriter writer, CharacterIntention intention)
    {
        writer.WriteStartObject();
        writer.WriteString("type", intention.Type.Value);
        writer.WriteString("targetObjectId", intention.TargetObjectId?.Value);
        writer.WriteEndObject();
    }

    private static void WriteConversationSummaries(
        Utf8JsonWriter writer,
        CharacterConversationSummarySnapshot snapshot)
    {
        writer.WritePropertyName("conversationSummaries");
        writer.WriteStartArray();
        foreach (CharacterConversationSummary summary in snapshot.Summaries)
        {
            writer.WriteStartObject();
            writer.WriteString("summaryId", summary.SummaryId.Value);
            WriteIdentifierValues(
                writer,
                "sourceTurnIds",
                summary.SourceTurnIds,
                identifier => identifier.Value);
            writer.WriteString("text", summary.Text);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteEpisodicMemories(
        Utf8JsonWriter writer,
        CharacterEpisodicMemorySnapshot snapshot)
    {
        writer.WritePropertyName("episodicMemories");
        writer.WriteStartArray();
        foreach (CharacterEpisodicMemory memory in snapshot.Memories)
        {
            writer.WriteStartObject();
            writer.WriteString("memoryId", memory.MemoryId.Value);
            WriteIdentifierValues(
                writer,
                "participantCharacterIds",
                memory.ParticipantCharacterIds,
                identifier => identifier.Value);
            writer.WriteString("locationId", memory.LocationId.Value);
            writer.WriteString("currentTimeEventId", memory.WorldTime.CurrentEventId?.Value);
            writer.WriteString("sourceTurnId", memory.SourceTurnId.Value);
            writer.WriteString("summary", memory.Summary);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteEpistemicMemories(
        Utf8JsonWriter writer,
        CharacterEpistemicMemorySnapshot snapshot)
    {
        writer.WritePropertyName("epistemicMemories");
        writer.WriteStartArray();
        foreach (CharacterEpistemicMemory memory in snapshot.Memories)
        {
            writer.WriteStartObject();
            writer.WriteString("memoryId", memory.MemoryId.Value);
            writer.WriteString("category", memory.Category.Value);
            writer.WriteString("sourceTurnId", memory.SourceTurnId.Value);
            writer.WriteString("statement", memory.Statement);
            writer.WriteString(
                "attributedSpeakerCharacterId",
                memory.AttributedSpeakerCharacterId?.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteBeliefAssessments(
        Utf8JsonWriter writer,
        CharacterBeliefAssessmentSnapshot snapshot)
    {
        writer.WritePropertyName("beliefAssessments");
        writer.WriteStartArray();
        foreach (CharacterBeliefAssessment assessment in snapshot.Assessments)
        {
            writer.WriteStartObject();
            writer.WriteString("beliefMemoryId", assessment.Belief.MemoryId.Value);
            writer.WriteNumber("confidenceBasisPoints", assessment.ConfidenceBasisPoints);
            WriteIdentifierValues(
                writer,
                "supportingEvidenceIds",
                assessment.SupportingEvidenceIds,
                identifier => identifier.Value);
            WriteIdentifierValues(
                writer,
                "contradictingEvidenceIds",
                assessment.ContradictingEvidenceIds,
                identifier => identifier.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteRelationships(
        Utf8JsonWriter writer,
        CharacterRelationshipMemorySnapshot snapshot)
    {
        writer.WritePropertyName("relationships");
        writer.WriteStartArray();
        foreach (CharacterRelationshipMemory relationship in snapshot.Relationships)
        {
            writer.WriteStartObject();
            writer.WriteString("relatedCharacterId", relationship.RelatedCharacterId.Value);
            writer.WritePropertyName("dimensions");
            writer.WriteStartArray();
            foreach (CharacterRelationshipDimensionValue dimension in relationship.Dimensions)
            {
                writer.WriteStartObject();
                writer.WriteString("dimension", dimension.Dimension.Value);
                writer.WriteNumber("value", dimension.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName("reasons");
            writer.WriteStartArray();
            foreach (CharacterRelationshipReason reason in relationship.Reasons)
            {
                writer.WriteStartObject();
                writer.WriteString("reasonId", reason.ReasonId.Value);
                writer.WriteString("dimension", reason.Dimension.Value);
                writer.WriteString("sourceTurnId", reason.SourceTurnId.Value);
                writer.WriteString("summary", reason.Summary);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteCommitments(
        Utf8JsonWriter writer,
        CharacterCommitmentMemorySnapshot snapshot)
    {
        writer.WritePropertyName("commitments");
        writer.WriteStartArray();
        foreach (CharacterCommitmentMemory commitment in snapshot.Commitments)
        {
            writer.WriteStartObject();
            writer.WriteString("commitmentId", commitment.CommitmentId.Value);
            writer.WriteString("kind", commitment.Kind.Value);
            writer.WriteString(
                "counterpartyCharacterId",
                commitment.CounterpartyCharacterId.Value);
            writer.WriteString("sourceTurnId", commitment.SourceTurnId.Value);
            writer.WriteString("description", commitment.Description);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteMemoryIndex(Utf8JsonWriter writer, CharacterMemoryIndex index)
    {
        writer.WritePropertyName("memoryIndex");
        writer.WriteStartArray();
        foreach (CharacterMemoryIndexEntry entry in index.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("category", entry.MemoryReference.Category.Value);
            writer.WriteString("memoryIdentity", GetMemoryIdentity(entry.MemoryReference));
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            foreach (CharacterMemoryTag tag in entry.Tags)
            {
                writer.WriteStartObject();
                writer.WriteString("kind", tag.Kind.Value);
                writer.WriteString("value", tag.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static string GetMemoryIdentity(CharacterMemoryReference reference) =>
        reference.Category switch
        {
            _ when ReferenceEquals(reference.Category, CharacterMemoryCategory.Episodic) =>
                reference.EpisodicMemory!.MemoryId.Value,
            _ when ReferenceEquals(reference.Category, CharacterMemoryCategory.Epistemic) =>
                reference.EpistemicMemory!.MemoryId.Value,
            _ when ReferenceEquals(reference.Category, CharacterMemoryCategory.Relationship) =>
                reference.RelationshipMemory!.RelatedCharacterId.Value,
            _ => reference.CommitmentMemory!.CommitmentId.Value,
        };

    private static void WriteIdentifierValues<T>(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<T> identifiers,
        Func<T, string> getValue)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (T identifier in identifiers)
        {
            writer.WriteStringValue(getValue(identifier));
        }

        writer.WriteEndArray();
    }
}
