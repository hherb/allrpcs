using System.Collections.ObjectModel;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Strictly materializes canonical version-3 first-patient state.</summary>
internal static class FirstPatientJsonReader
{
    /// <summary>Reads the complete first-patient composition from its closed document shape.</summary>
    internal static FirstPatientSaveState Read(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(
            element,
            "diseaseTruth",
            "preparations",
            "preparedBatches",
            "administrationExperiments",
            "experiments");
        DiseaseTruthSnapshot diseaseTruth = ReadDiseaseTruth(element.GetProperty("diseaseTruth"));
        ReadOnlyCollection<IngredientPreparationOutcome> preparations =
            ReadPreparations(element.GetProperty("preparations"));
        Dictionary<string, WorldEvent> worldEvents = new(StringComparer.Ordinal);
        return new FirstPatientSaveState(
            diseaseTruth,
            ReadPreparedBatches(element.GetProperty("preparedBatches"), preparations),
            ReadAdministrationExperiments(
                element.GetProperty("administrationExperiments"),
                preparations,
                worldEvents),
            ReadExperiments(element.GetProperty("experiments"), worldEvents));
    }

    private static DiseaseTruthSnapshot ReadDiseaseTruth(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(element, "definitions", "infections");
        JsonElement definitionsElement = element.GetProperty("definitions");
        SaveSnapshotJsonDeserializer.RequireArray(definitionsElement);
        List<DiseaseDefinition> definitions = [];
        Dictionary<string, DiseaseDefinition> byId = new(StringComparer.Ordinal);
        foreach (JsonElement definitionElement in definitionsElement.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(definitionElement, "diseaseId");
            DiseaseDefinition definition = new(
                new DiseaseId(ReadString(definitionElement, "diseaseId")));
            if (!byId.TryAdd(definition.DiseaseId.Value, definition))
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            definitions.Add(definition);
        }

        JsonElement infectionsElement = element.GetProperty("infections");
        SaveSnapshotJsonDeserializer.RequireArray(infectionsElement);
        List<CharacterDiseaseState> infections = [];
        foreach (JsonElement infectionElement in infectionsElement.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                infectionElement,
                "characterId",
                "diseaseId",
                "stage",
                "lastProgressionEventId",
                "careState");
            string diseaseId = ReadString(infectionElement, "diseaseId");
            if (!byId.TryGetValue(diseaseId, out DiseaseDefinition? disease))
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            string? progressionEventId = ReadNullableString(
                infectionElement.GetProperty("lastProgressionEventId"));
            infections.Add(new CharacterDiseaseState(
                new CharacterId(ReadString(infectionElement, "characterId")),
                disease,
                DiseaseStage.Parse(ReadString(infectionElement, "stage")),
                progressionEventId is null ? null : new WorldEventId(progressionEventId),
                DiseaseCareState.Parse(ReadString(infectionElement, "careState"))));
        }

        return new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot(definitions), infections);
    }

    private static ReadOnlyCollection<IngredientPreparationOutcome> ReadPreparations(
        JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<IngredientPreparationOutcome> preparations = [];
        foreach (JsonElement preparationElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                preparationElement,
                "recipeId",
                "steps",
                "inputs",
                "resultIngredientId",
                "resultQuantity",
                "completedStepCount");
            IngredientPreparationPlan plan = ReadPlan(preparationElement.GetProperty("steps"));
            IngredientDefinition resultDefinition = new(
                new IngredientId(ReadString(preparationElement, "resultIngredientId")));
            IngredientQuantity resultQuantity = ReadQuantity(
                preparationElement.GetProperty("resultQuantity"));
            IngredientPreparationRecipe recipe = new(
                new PreparationRecipeId(ReadString(preparationElement, "recipeId")),
                plan,
                ReadInputs(preparationElement.GetProperty("inputs")),
                resultDefinition,
                resultQuantity);
            IngredientPreparationActivity activity = new(
                recipe,
                new IngredientPreparationProgress(
                    plan,
                    SaveSnapshotJsonDeserializer.ReadInt32(
                        preparationElement.GetProperty("completedStepCount"))));
            preparations.Add(new IngredientPreparationOutcome(
                activity,
                resultDefinition,
                resultQuantity));
        }

        return preparations.AsReadOnly();
    }

    private static IngredientPreparationPlan ReadPlan(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        return new IngredientPreparationPlan(element.EnumerateArray().Select(step =>
            new IngredientPreparationStep(
                IngredientPreparationKind.Parse(SaveSnapshotJsonDeserializer.ReadString(step)))));
    }

    private static ReadOnlyCollection<IngredientPreparationInputRequirement> ReadInputs(
        JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<IngredientPreparationInputRequirement> inputs = [];
        foreach (JsonElement inputElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(inputElement, "ingredientId", "quantity");
            inputs.Add(new IngredientPreparationInputRequirement(
                new IngredientDefinition(new IngredientId(ReadString(inputElement, "ingredientId"))),
                ReadQuantity(inputElement.GetProperty("quantity"))));
        }

        return inputs.AsReadOnly();
    }

    private static PreparedIngredientBatchSnapshot ReadPreparedBatches(
        JsonElement element,
        IReadOnlyList<IngredientPreparationOutcome> preparations)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<PreparedIngredientBatch> batches = [];
        foreach (JsonElement batchElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                batchElement,
                "preparationIndex",
                "remainingQuantity");
            IngredientPreparationOutcome preparation = GetPreparation(
                preparations,
                batchElement.GetProperty("preparationIndex"));
            IngredientQuantity remaining = ReadQuantity(
                batchElement.GetProperty("remainingQuantity"));
            PreparedIngredientBatch batch = new(preparation);
            if (remaining.Amount < preparation.Quantity.Amount)
            {
                IngredientQuantity consumed = new(
                    preparation.Quantity.Amount - remaining.Amount,
                    remaining.Unit);
                batch = PreparedIngredientBatchConsumptionTransition.Consume(
                    new IngredientDosage(preparation, consumed),
                    batch) ?? throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }
            else if (remaining.Amount != preparation.Quantity.Amount
                || !ReferenceEquals(remaining.Unit, preparation.Quantity.Unit))
            {
                throw SaveSnapshotJsonDeserializer.InvalidDocument();
            }

            batches.Add(batch);
        }

        return new PreparedIngredientBatchSnapshot(batches);
    }

    private static IngredientExperimentJournalSnapshot ReadAdministrationExperiments(
        JsonElement element,
        IReadOnlyList<IngredientPreparationOutcome> preparations,
        Dictionary<string, WorldEvent> worldEvents)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<IngredientAdministrationExperimentRecord> records = [];
        foreach (JsonElement recordElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                recordElement,
                "experimentId",
                "administration",
                "observedConditions");
            JsonElement administrationElement = recordElement.GetProperty("administration");
            SaveSnapshotJsonDeserializer.RequireObject(
                administrationElement,
                "administratorCharacterId",
                "patientCharacterId",
                "dosage",
                "worldEvent");
            JsonElement dosageElement = administrationElement.GetProperty("dosage");
            SaveSnapshotJsonDeserializer.RequireObject(
                dosageElement,
                "preparationIndex",
                "quantity");
            IngredientPreparationOutcome preparation = GetPreparation(
                preparations,
                dosageElement.GetProperty("preparationIndex"));
            IngredientAdministrationOutcome administration = new(
                new CharacterId(ReadString(administrationElement, "administratorCharacterId")),
                new CharacterId(ReadString(administrationElement, "patientCharacterId")),
                new IngredientDosage(
                    preparation,
                    ReadQuantity(dosageElement.GetProperty("quantity"))),
                ReadWorldEvent(administrationElement.GetProperty("worldEvent"), worldEvents));
            records.Add(new IngredientAdministrationExperimentRecord(
                new ExperimentId(ReadString(recordElement, "experimentId")),
                administration,
                ReadConditions(recordElement.GetProperty("observedConditions"))));
        }

        return new IngredientExperimentJournalSnapshot(records);
    }

    private static CharacterConditionSet ReadConditions(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<CharacterCondition> conditions = [];
        foreach (JsonElement conditionElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(conditionElement, "key", "description");
            conditions.Add(new CharacterCondition(
                ReadString(conditionElement, "key"),
                ReadString(conditionElement, "description")));
        }

        return new CharacterConditionSet(conditions);
    }

    private static ExperimentJournalSnapshot ReadExperiments(
        JsonElement element,
        Dictionary<string, WorldEvent> worldEvents)
    {
        SaveSnapshotJsonDeserializer.RequireArray(element);
        List<ExperimentJournalRecord> records = [];
        foreach (JsonElement recordElement in element.EnumerateArray())
        {
            SaveSnapshotJsonDeserializer.RequireObject(
                recordElement,
                "experimentId",
                "worldEvent",
                "procedure",
                "observedOutcome");
            records.Add(new ExperimentJournalRecord(
                new ExperimentId(ReadString(recordElement, "experimentId")),
                ReadWorldEvent(recordElement.GetProperty("worldEvent"), worldEvents),
                ReadString(recordElement, "procedure"),
                ReadString(recordElement, "observedOutcome")));
        }

        return new ExperimentJournalSnapshot(records);
    }

    private static WorldEvent ReadWorldEvent(
        JsonElement element,
        Dictionary<string, WorldEvent> worldEvents)
    {
        SaveSnapshotJsonDeserializer.RequireObject(element, "eventId", "timeEffect");
        string eventId = ReadString(element, "eventId");
        WorldEventTimeEffect timeEffect = WorldEventTimeEffect.Parse(
            ReadString(element, "timeEffect"));
        if (worldEvents.TryGetValue(eventId, out WorldEvent? existing))
        {
            return ReferenceEquals(existing.TimeEffect, timeEffect)
                ? existing
                : throw SaveSnapshotJsonDeserializer.InvalidDocument();
        }

        WorldEvent worldEvent = new(new WorldEventId(eventId), timeEffect);
        worldEvents.Add(eventId, worldEvent);
        return worldEvent;
    }

    private static IngredientPreparationOutcome GetPreparation(
        IReadOnlyList<IngredientPreparationOutcome> preparations,
        JsonElement indexElement)
    {
        int index = SaveSnapshotJsonDeserializer.ReadInt32(indexElement);
        return index >= 0 && index < preparations.Count
            ? preparations[index]
            : throw SaveSnapshotJsonDeserializer.InvalidDocument();
    }

    private static IngredientQuantity ReadQuantity(JsonElement element)
    {
        SaveSnapshotJsonDeserializer.RequireObject(element, "amount", "unit");
        return new IngredientQuantity(
            SaveSnapshotJsonDeserializer.ReadInt32(element.GetProperty("amount")),
            IngredientQuantityUnit.Parse(ReadString(element, "unit")));
    }

    private static string ReadString(JsonElement element, string propertyName) =>
        SaveSnapshotJsonDeserializer.ReadString(element.GetProperty(propertyName));

    private static string? ReadNullableString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String => SaveSnapshotJsonDeserializer.ReadString(element),
        _ => throw SaveSnapshotJsonDeserializer.InvalidDocument(),
    };
}
