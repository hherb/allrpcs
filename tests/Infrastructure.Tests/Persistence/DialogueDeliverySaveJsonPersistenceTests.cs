using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class DialogueDeliverySaveJsonPersistenceTests
{
    [Fact]
    public void SerializesVersionFourWithExactDialogueDeliveryModesWithoutChangingInputs()
    {
        SaveSnapshot snapshot = CreateSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            DialogueDeliveryMode.Whispering);
        DialogueEvent dialogueEvent = RecentDialogueEvent(snapshot);

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);
        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement heard = document.RootElement.GetProperty("characterMemory")
            .GetProperty("characters")[0]
            .GetProperty("recentConversation")[0]
            .GetProperty("heard");
        JsonElement response = document.RootElement.GetProperty("characterMemory")
            .GetProperty("characters")[0]
            .GetProperty("recentConversation")[0]
            .GetProperty("response");

        Assert.Equal(4, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("whispering", heard.GetProperty("deliveryMode").GetString());
        Assert.Equal("whispering", response.GetProperty("deliveryMode").GetString());
        Assert.Same(DialogueDeliveryMode.Whispering, dialogueEvent.Delivery.Mode);
        Assert.Same(DialogueDeliveryMode.Whispering, ResponseDelivery(snapshot).Mode);
    }

    [Fact]
    public void DeserializesAndCanonicallyRoundTripsExactVersionFourDeliveryMode()
    {
        SaveSnapshot source = CreateSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            DialogueDeliveryMode.Whispering);
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.Same(SaveSchemaVersion.DialogueDelivery, result.SchemaVersion);
        Assert.Same(
            DialogueDeliveryMode.Whispering,
            RecentDialogueEvent(result).Delivery.Mode);
        Assert.Same(DialogueDeliveryMode.Whispering, ResponseDelivery(result).Mode);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Fact]
    public void DeserializeCurrentMigratesVersionThreeWithHistoricallyImplicitOrdinarySpeech()
    {
        SaveSnapshot source = CreateSnapshot(
            SaveSchemaVersion.FirstPatient,
            DialogueDeliveryMode.Whispering);
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        Assert.DoesNotContain("deliveryMode", serialized, StringComparison.Ordinal);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            serialized,
            EmptyFirstPatient());

        Assert.Same(SaveSchemaVersion.FirstPatientRecovery, result.SchemaVersion);
        Assert.Same(FirstPatientRecoveryStage.NotHeard, result.FirstPatientRecovery);
        Assert.Same(
            DialogueDeliveryMode.OrdinarySpeech,
            RecentDialogueEvent(result).Delivery.Mode);
        Assert.Same(DialogueDeliveryMode.OrdinarySpeech, ResponseDelivery(result).Mode);
        Assert.Contains(
            "\"deliveryMode\":\"ordinary-speech\"",
            SaveSnapshotJsonSerializer.Serialize(result),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DeserializeCurrentMigratesAnAlreadyValidVersionFourDocument()
    {
        SaveSnapshot source = CreateSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            DialogueDeliveryMode.Whispering);
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            serialized,
            EmptyFirstPatient());

        Assert.Same(SaveSchemaVersion.FirstPatientRecovery, result.SchemaVersion);
        Assert.Same(FirstPatientRecoveryStage.NotHeard, result.FirstPatientRecovery);
        Assert.Same(
            DialogueDeliveryMode.Whispering,
            RecentDialogueEvent(result).Delivery.Mode);
        Assert.Same(DialogueDeliveryMode.Whispering, ResponseDelivery(result).Mode);
        Assert.NotNull(result.Journal);
        Assert.Empty(result.Journal.Observations);
        Assert.NotNull(result.Map);
        Assert.Empty(result.Map.ReportedLocationIds);
        Assert.NotEqual(serialized, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Fact]
    public void RejectsMissingUnknownDuplicateInvalidOrMismatchedVersionFourDeliveryMode()
    {
        string serialized = SaveSnapshotJsonSerializer.Serialize(CreateSnapshot(
            SaveSchemaVersion.DialogueDelivery,
            DialogueDeliveryMode.Whispering));
        string missing = serialized.Replace(
            "\"deliveryMode\":\"whispering\",",
            string.Empty,
            StringComparison.Ordinal);
        string unknown = serialized.Replace(
            "\"deliveryMode\":\"whispering\",",
            "\"deliveryMode\":\"whispering\",\"unknown\":true,",
            StringComparison.Ordinal);
        string duplicate = serialized.Replace(
            "\"deliveryMode\":\"whispering\",",
            "\"deliveryMode\":\"whispering\",\"deliveryMode\":\"whispering\",",
            StringComparison.Ordinal);
        string invalid = serialized.Replace(
            "\"deliveryMode\":\"whispering\"",
            "\"deliveryMode\":\"shouting\"",
            StringComparison.Ordinal);
        string missingResponseMode = serialized.Replace(
            "\"speech\":\"I understand.\",\"deliveryMode\":\"whispering\",",
            "\"speech\":\"I understand.\",",
            StringComparison.Ordinal);
        string nullResponseMode = serialized.Replace(
            "\"speech\":\"I understand.\",\"deliveryMode\":\"whispering\",",
            "\"speech\":\"I understand.\",\"deliveryMode\":null,",
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(missing));
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(unknown));
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(duplicate));
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(invalid));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(missingResponseMode));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(nullResponseMode));
    }

    private static DialogueEvent RecentDialogueEvent(SaveSnapshot snapshot) =>
        Assert.Single(Assert.Single(snapshot.CharacterMemory.Characters)
            .RecentConversation.Turns).Activity.Request.DialogueEvent;

    private static DialogueDelivery ResponseDelivery(SaveSnapshot snapshot) =>
        Assert.Single(Assert.Single(snapshot.CharacterMemory.Characters)
            .RecentConversation.Turns).Activity.Response!.Delivery!;

    private static SaveSnapshot CreateSnapshot(
        SaveSchemaVersion version,
        DialogueDeliveryMode deliveryMode)
    {
        CharacterId seeker = new("seeker");
        CharacterId healer = new("healer");
        DialogueUtterance utterance = new(seeker, "Keep this between us.");
        CharacterTurnRequest request = new(
            healer,
            new DialogueEvent(
                new DialogueDelivery(utterance, deliveryMode),
                new DialogueAudienceSnapshot(seeker, [healer])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(healer, "I understand."),
                deliveryMode),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
        CharacterRecentConversationTurn turn = new(
            new CharacterTurnId("turn-one"),
            new CharacterTurnActivity(request, response, CharacterTurnActivityState.Completed));
        CharacterMemoryAggregate memory = new(
            healer,
            new CharacterRecentConversationWindow(healer, [turn]),
            new CharacterConversationSummarySnapshot(healer, []),
            new CharacterEpisodicMemorySnapshot(healer, []),
            new CharacterEpistemicMemorySnapshot(healer, []),
            new CharacterBeliefAssessmentSnapshot(healer, []),
            new CharacterRelationshipMemorySnapshot(healer, []),
            new CharacterCommitmentMemorySnapshot(healer, []),
            new CharacterMemoryIndex(healer, []));
        return new SaveSnapshot(
            version,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(seeker, []),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([memory]),
            EmptyFirstPatient());
    }

    private static FirstPatientSaveState EmptyFirstPatient() => new(
        new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([]), []),
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));
}
