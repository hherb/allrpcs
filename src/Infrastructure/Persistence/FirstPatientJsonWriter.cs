using System.Collections.ObjectModel;
using System.Text.Json;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Infrastructure.Persistence;

/// <summary>Writes canonical first-patient state inside a version-3 save document.</summary>
internal static class FirstPatientJsonWriter
{
    /// <summary>Writes every first-patient composition in stable supplied order.</summary>
    internal static void Write(Utf8JsonWriter writer, FirstPatientSaveState state)
    {
        ReadOnlyCollection<IngredientPreparationOutcome> preparations =
            CollectPreparations(state);
        writer.WritePropertyName("firstPatient");
        writer.WriteStartObject();
        WriteDiseaseTruth(writer, state.DiseaseTruth);
        WritePreparations(writer, preparations);
        WritePreparedBatches(writer, state.PreparedBatches, preparations);
        WriteAdministrationExperiments(
            writer,
            state.AdministrationExperiments,
            preparations);
        WriteExperiments(writer, state.Experiments);
        writer.WriteEndObject();
    }

    private static ReadOnlyCollection<IngredientPreparationOutcome> CollectPreparations(
        FirstPatientSaveState state)
    {
        List<IngredientPreparationOutcome> preparations = [];
        HashSet<IngredientPreparationOutcome> identities = new(
            ReferenceEqualityComparer.Instance);
        foreach (PreparedIngredientBatch batch in state.PreparedBatches.Batches)
        {
            if (identities.Add(batch.Preparation))
            {
                preparations.Add(batch.Preparation);
            }
        }

        foreach (IngredientAdministrationExperimentRecord record in
            state.AdministrationExperiments.Records)
        {
            IngredientPreparationOutcome preparation = record.Administration.Dosage.Preparation;
            if (identities.Add(preparation))
            {
                preparations.Add(preparation);
            }
        }

        return preparations.AsReadOnly();
    }

    private static void WriteDiseaseTruth(Utf8JsonWriter writer, DiseaseTruthSnapshot truth)
    {
        writer.WritePropertyName("diseaseTruth");
        writer.WriteStartObject();
        writer.WritePropertyName("definitions");
        writer.WriteStartArray();
        foreach (DiseaseDefinition definition in truth.Definitions.Definitions)
        {
            writer.WriteStartObject();
            writer.WriteString("diseaseId", definition.DiseaseId.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WritePropertyName("infections");
        writer.WriteStartArray();
        foreach (CharacterDiseaseState infection in truth.Infections)
        {
            writer.WriteStartObject();
            writer.WriteString("characterId", infection.CharacterId.Value);
            writer.WriteString("diseaseId", infection.Disease.DiseaseId.Value);
            writer.WriteString("stage", infection.Stage.Value);
            writer.WriteString("lastProgressionEventId", infection.LastProgressionEventId?.Value);
            writer.WriteString("careState", infection.CareState.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WritePreparations(
        Utf8JsonWriter writer,
        IReadOnlyList<IngredientPreparationOutcome> preparations)
    {
        writer.WritePropertyName("preparations");
        writer.WriteStartArray();
        foreach (IngredientPreparationOutcome preparation in preparations)
        {
            WritePreparation(writer, preparation);
        }

        writer.WriteEndArray();
    }

    private static void WritePreparedBatches(
        Utf8JsonWriter writer,
        PreparedIngredientBatchSnapshot snapshot,
        IReadOnlyList<IngredientPreparationOutcome> preparations)
    {
        writer.WritePropertyName("preparedBatches");
        writer.WriteStartArray();
        foreach (PreparedIngredientBatch batch in snapshot.Batches)
        {
            writer.WriteStartObject();
            writer.WriteNumber(
                "preparationIndex",
                GetPreparationIndex(preparations, batch.Preparation));
            WriteQuantity(writer, "remainingQuantity", batch.RemainingQuantity);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WritePreparation(
        Utf8JsonWriter writer,
        IngredientPreparationOutcome preparation)
    {
        writer.WriteStartObject();
        IngredientPreparationRecipe recipe = preparation.Activity.Recipe;
        writer.WriteString("recipeId", recipe.RecipeId.Value);
        writer.WritePropertyName("steps");
        writer.WriteStartArray();
        foreach (IngredientPreparationStep step in recipe.Plan.Steps)
        {
            writer.WriteStringValue(step.Kind.Value);
        }

        writer.WriteEndArray();
        writer.WritePropertyName("inputs");
        writer.WriteStartArray();
        foreach (IngredientPreparationInputRequirement input in recipe.Inputs)
        {
            writer.WriteStartObject();
            writer.WriteString("ingredientId", input.Definition.IngredientId.Value);
            WriteQuantity(writer, "quantity", input.Quantity);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteString("resultIngredientId", preparation.Definition.IngredientId.Value);
        WriteQuantity(writer, "resultQuantity", preparation.Quantity);
        writer.WriteNumber("completedStepCount", preparation.Activity.Progress.NextStepIndex);
        writer.WriteEndObject();
    }

    private static void WriteAdministrationExperiments(
        Utf8JsonWriter writer,
        IngredientExperimentJournalSnapshot snapshot,
        IReadOnlyList<IngredientPreparationOutcome> preparations)
    {
        writer.WritePropertyName("administrationExperiments");
        writer.WriteStartArray();
        foreach (IngredientAdministrationExperimentRecord record in snapshot.Records)
        {
            writer.WriteStartObject();
            writer.WriteString("experimentId", record.ExperimentId.Value);
            writer.WritePropertyName("administration");
            writer.WriteStartObject();
            writer.WriteString("administratorCharacterId", record.Administration.AdministratorId.Value);
            writer.WriteString("patientCharacterId", record.Administration.PatientId.Value);
            writer.WritePropertyName("dosage");
            writer.WriteStartObject();
            writer.WriteNumber(
                "preparationIndex",
                GetPreparationIndex(
                    preparations,
                    record.Administration.Dosage.Preparation));
            WriteQuantity(writer, "quantity", record.Administration.Dosage.Quantity);
            writer.WriteEndObject();
            WriteWorldEvent(writer, record.Administration.WorldEvent);
            writer.WriteEndObject();
            writer.WritePropertyName("observedConditions");
            writer.WriteStartArray();
            foreach (CharacterCondition condition in record.ObservedConditions.Entries)
            {
                writer.WriteStartObject();
                writer.WriteString("key", condition.Key);
                writer.WriteString("description", condition.Description);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static int GetPreparationIndex(
        IReadOnlyList<IngredientPreparationOutcome> preparations,
        IngredientPreparationOutcome target)
    {
        for (int index = 0; index < preparations.Count; index++)
        {
            if (ReferenceEquals(preparations[index], target))
            {
                return index;
            }
        }

        throw new InvalidOperationException(
            "Every referenced completed preparation must be serialized exactly once.");
    }

    private static void WriteExperiments(Utf8JsonWriter writer, ExperimentJournalSnapshot snapshot)
    {
        writer.WritePropertyName("experiments");
        writer.WriteStartArray();
        foreach (ExperimentJournalRecord record in snapshot.Records)
        {
            writer.WriteStartObject();
            writer.WriteString("experimentId", record.ExperimentId.Value);
            WriteWorldEvent(writer, record.WorldEvent);
            writer.WriteString("procedure", record.Procedure);
            writer.WriteString("observedOutcome", record.ObservedOutcome);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteWorldEvent(Utf8JsonWriter writer, WorldEvent worldEvent)
    {
        writer.WritePropertyName("worldEvent");
        writer.WriteStartObject();
        writer.WriteString("eventId", worldEvent.EventId.Value);
        writer.WriteString("timeEffect", worldEvent.TimeEffect.Value);
        writer.WriteEndObject();
    }

    private static void WriteQuantity(
        Utf8JsonWriter writer,
        string propertyName,
        IngredientQuantity quantity)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteNumber("amount", quantity.Amount);
        writer.WriteString("unit", quantity.Unit.Value);
        writer.WriteEndObject();
    }
}
