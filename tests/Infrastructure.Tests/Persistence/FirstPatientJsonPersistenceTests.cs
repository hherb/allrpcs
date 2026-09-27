using System.Text.Json;
using Allrpcs.Domain.Characters;
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

public sealed class FirstPatientJsonPersistenceTests
{
    [Fact]
    public void SerializeWritesEmptyFirstPatientVersionExplicitly()
    {
        FirstPatientSaveState firstPatient = EmptyFirstPatient();
        SaveSnapshot snapshot = CreateRoot(firstPatient);

        Assert.Equal(EmptyFirstPatientJson, SaveSnapshotJsonSerializer.Serialize(snapshot));
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Empty(firstPatient.DiseaseTruth.Definitions.Definitions);
        Assert.Empty(firstPatient.DiseaseTruth.Infections);
    }

    [Fact]
    public void SerializeWritesStableCompleteFirstPatientDocumentWithoutMutatingInputs()
    {
        SaveSnapshot snapshot = CreateCompleteSnapshot();

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);

        using JsonDocument document = JsonDocument.Parse(serialized);
        JsonElement firstPatient = document.RootElement.GetProperty("firstPatient");
        JsonElement diseaseTruth = firstPatient.GetProperty("diseaseTruth");
        JsonElement batch = firstPatient.GetProperty("preparedBatches")[0];
        JsonElement preparation = firstPatient.GetProperty("preparations")[0];
        JsonElement administration = firstPatient.GetProperty("administrationExperiments")[0];
        JsonElement experiment = firstPatient.GetProperty("experiments")[0];

        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("grey-sleep", diseaseTruth.GetProperty("definitions")[0]
            .GetProperty("diseaseId").GetString());
        Assert.Equal("stabilized", diseaseTruth.GetProperty("infections")[0]
            .GetProperty("careState").GetString());
        Assert.Equal("cooling-moonwort-remedy", preparation.GetProperty("recipeId").GetString());
        Assert.Equal(4, preparation.GetProperty("completedStepCount").GetInt32());
        Assert.Equal(0, batch.GetProperty("preparationIndex").GetInt32());
        Assert.Equal(75, batch.GetProperty("remainingQuantity").GetProperty("amount").GetInt32());
        Assert.Equal("first-patient-remedy-administration",
            administration.GetProperty("experimentId").GetString());
        Assert.Equal("breathing", administration.GetProperty("observedConditions")[0]
            .GetProperty("key").GetString());
        Assert.Equal("Administer one cooled remedy dose.",
            experiment.GetProperty("procedure").GetString());
        Assert.DoesNotContain("provider", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("credential", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reasoning", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("path", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("uiText", serialized, StringComparison.Ordinal);
        AssertCompleteSnapshotUnchanged(snapshot);
    }

    [Fact]
    public void SerializeWritesValueEqualFirstPatientSnapshotsIdentically()
    {
        SaveSnapshot first = CreateCompleteSnapshot();
        SaveSnapshot second = CreateCompleteSnapshot();

        Assert.NotSame(first, second);
        Assert.Equal(
            SaveSnapshotJsonSerializer.Serialize(first),
            SaveSnapshotJsonSerializer.Serialize(second));
    }

    [Fact]
    public void DeserializeRoundTripsCompleteStateWithCanonicalSharedReferences()
    {
        SaveSnapshot source = CreateCompleteSnapshot();
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.Same(SaveSchemaVersion.FirstPatient, result.SchemaVersion);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(result));
        FirstPatientSaveState firstPatient = Assert.IsType<FirstPatientSaveState>(
            result.FirstPatient);
        DiseaseDefinition disease = Assert.Single(
            firstPatient.DiseaseTruth.Definitions.Definitions);
        Assert.Same(disease, Assert.Single(firstPatient.DiseaseTruth.Infections).Disease);
        PreparedIngredientBatch batch = Assert.Single(firstPatient.PreparedBatches.Batches);
        IngredientAdministrationExperimentRecord administration = Assert.Single(
            firstPatient.AdministrationExperiments.Records);
        ExperimentJournalRecord experiment = Assert.Single(firstPatient.Experiments.Records);
        Assert.Same(batch.Preparation, administration.Administration.Dosage.Preparation);
        Assert.Same(administration.Administration.WorldEvent, experiment.WorldEvent);
        AssertCompleteSnapshotUnchanged(source);
    }

    [Fact]
    public void DeserializeCurrentMigratesVersionThreeToCurrentSchema()
    {
        SaveSnapshot source = CreateCompleteSnapshot();
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        FirstPatientSaveState initialFirstPatient = EmptyFirstPatient();

        SaveSnapshot result = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            serialized,
            initialFirstPatient);

        Assert.Same(SaveSchemaVersion.InvestigationRoutes, result.SchemaVersion);
        Assert.NotSame(initialFirstPatient, result.FirstPatient);
        Assert.NotNull(result.Journal);
        Assert.NotNull(result.Map);
        Assert.NotEqual(serialized, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Fact]
    public void DeserializeRejectsInvalidFirstPatientShapesReferencesAndValues()
    {
        string complete = SaveSnapshotJsonSerializer.Serialize(CreateCompleteSnapshot());
        string unknownProperty = complete.Replace(
            "\"preparations\":[",
            "\"unknown\":true,\"preparations\":[",
            StringComparison.Ordinal);
        string missingPreparation = complete.Replace(
            "\"preparationIndex\":0",
            "\"preparationIndex\":1",
            StringComparison.Ordinal);
        string incompletePreparation = complete.Replace(
            "\"completedStepCount\":4",
            "\"completedStepCount\":3",
            StringComparison.Ordinal);
        string mismatchedEvent = complete.Replace(
            "\"timeEffect\":\"advance\"},\"procedure\"",
            "\"timeEffect\":\"no-advance\"},\"procedure\"",
            StringComparison.Ordinal);
        string nonCanonicalIndex = complete.Replace(
            "\"preparationIndex\":0",
            "\"preparationIndex\":0.0",
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(unknownProperty));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(missingPreparation));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(incompletePreparation));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(mismatchedEvent));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(nonCanonicalIndex));
    }

    private static SaveSnapshot CreateCompleteSnapshot()
    {
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        DiseaseTruthSnapshot initialTruth = context.Scenario.DiseaseTruth;
        CharacterDiseaseState infection = Assert.Single(initialTruth.Infections);
        DiseaseTruthSnapshot stabilizedTruth = new(
            initialTruth.Definitions,
            [
                new CharacterDiseaseState(
                    infection.CharacterId,
                    infection.Disease,
                    infection.Stage,
                    new WorldEventId("healer-administered-first-patient-remedy"),
                    DiseaseCareState.Stabilized),
            ]);
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count));
        IngredientPreparationOutcome preparation =
            IngredientPreparationCompletionTransition.Complete(activity);
        PreparedIngredientBatch batch = new(preparation);
        IngredientDosage dosage = new(
            preparation,
            new IngredientQuantity(25, IngredientQuantityUnit.Millilitre));
        WorldEvent worldEvent = new(
            new WorldEventId("healer-administered-first-patient-remedy"),
            WorldEventTimeEffect.Advance);
        ExperimentId experimentId = new("first-patient-remedy-administration");
        CharacterConditionSet observedConditions = new(
            [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]);
        IngredientAdministrationOutcome administration = new(
            new CharacterId("healer"),
            context.Scenario.PatientId,
            dosage,
            worldEvent);
        FirstPatientSaveState firstPatient = new(
            stabilizedTruth,
            new PreparedIngredientBatchSnapshot(
                [PreparedIngredientBatchConsumptionTransition.Consume(dosage, batch)!]),
            new IngredientExperimentJournalSnapshot(
                [new IngredientAdministrationExperimentRecord(
                    experimentId,
                    administration,
                    observedConditions)]),
            new ExperimentJournalSnapshot(
                [new ExperimentJournalRecord(
                    experimentId,
                    worldEvent,
                    "Administer one cooled remedy dose.",
                    "The patient's breathing becomes quieter and steadier.")]));
        return CreateRoot(firstPatient);
    }

    private static SaveSnapshot CreateRoot(FirstPatientSaveState firstPatient)
    {
        CharacterId owner = new("seeker");
        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatient,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(owner, []),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            firstPatient);
    }

    private static FirstPatientSaveState EmptyFirstPatient() => new(
        new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot([]), []),
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));

    private static void AssertCompleteSnapshotUnchanged(SaveSnapshot snapshot)
    {
        FirstPatientSaveState firstPatient = Assert.IsType<FirstPatientSaveState>(
            snapshot.FirstPatient);
        Assert.Same(SaveSchemaVersion.FirstPatient, snapshot.SchemaVersion);
        Assert.Single(firstPatient.DiseaseTruth.Definitions.Definitions);
        Assert.Same(
            DiseaseCareState.Stabilized,
            Assert.Single(firstPatient.DiseaseTruth.Infections).CareState);
        Assert.Equal(75, Assert.Single(firstPatient.PreparedBatches.Batches)
            .RemainingQuantity.Amount);
        Assert.Single(firstPatient.AdministrationExperiments.Records);
        Assert.Single(firstPatient.Experiments.Records);
    }

    private const string EmptyFirstPatientJson =
        "{\"schemaVersion\":3,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null},"
        + "\"characterMemory\":{\"characters\":[]},\"firstPatient\":{\"diseaseTruth\":{"
        + "\"definitions\":[],\"infections\":[]},\"preparations\":[],\"preparedBatches\":[],"
        + "\"administrationExperiments\":[],\"experiments\":[]}}";
}
