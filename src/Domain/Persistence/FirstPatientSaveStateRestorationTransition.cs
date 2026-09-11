using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Persistence;

/// <summary>Rebinds saved first-patient state to exact authored definitions after full preflight.</summary>
public static class FirstPatientSaveStateRestorationTransition
{
    /// <summary>Returns fresh immutable state whose definitions are exact authored instances.</summary>
    public static FirstPatientSaveState Rebind(
        FirstPatientSaveState saved,
        FirstPatientTreatmentContext authoredContext,
        WorldEventSnapshot authoredWorldEvents)
    {
        ArgumentNullException.ThrowIfNull(saved);
        ArgumentNullException.ThrowIfNull(authoredContext);
        ArgumentNullException.ThrowIfNull(authoredWorldEvents);
        if (!FirstPatientTreatmentContextPolicy.IsAuthorized(authoredContext))
        {
            throw Incompatible();
        }

        DiseaseTruthSnapshot diseaseTruth = RebindDiseaseTruth(
            saved.DiseaseTruth,
            authoredContext,
            authoredWorldEvents);
        Dictionary<IngredientPreparationOutcome, IngredientPreparationOutcome> preparations =
            new(ReferenceEqualityComparer.Instance);
        PreparedIngredientBatchSnapshot preparedBatches = RebindBatches(
            saved.PreparedBatches,
            authoredContext.Scenario.Remedy.Recipe,
            preparations);
        IngredientExperimentJournalSnapshot administrationExperiments = RebindAdministrations(
            saved.AdministrationExperiments,
            authoredContext,
            authoredWorldEvents,
            preparations);
        ExperimentJournalSnapshot experiments = RebindExperiments(
            saved.Experiments,
            authoredWorldEvents);
        RequireAdministrationExperimentPairs(administrationExperiments, experiments);

        return new FirstPatientSaveState(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);
    }

    private static DiseaseTruthSnapshot RebindDiseaseTruth(
        DiseaseTruthSnapshot saved,
        FirstPatientTreatmentContext authoredContext,
        WorldEventSnapshot authoredWorldEvents)
    {
        DiseaseTruthSnapshot authored = authoredContext.Scenario.DiseaseTruth;
        if (saved.Definitions.Definitions.Count != authored.Definitions.Definitions.Count
            || saved.Infections.Count != authored.Infections.Count)
        {
            throw Incompatible();
        }

        List<DiseaseDefinition> definitions = [];
        foreach (DiseaseDefinition savedDefinition in saved.Definitions.Definitions)
        {
            DiseaseDefinition? definition = authored.Definitions.Definitions.SingleOrDefault(
                candidate => candidate.DiseaseId == savedDefinition.DiseaseId);
            definitions.Add(definition ?? throw Incompatible());
        }

        List<CharacterDiseaseState> infections = [];
        foreach (CharacterDiseaseState savedInfection in saved.Infections)
        {
            CharacterDiseaseState? authoredInfection = authored.Infections.SingleOrDefault(
                candidate => candidate.CharacterId == savedInfection.CharacterId
                    && candidate.Disease.DiseaseId == savedInfection.Disease.DiseaseId);
            DiseaseDefinition? disease = definitions.SingleOrDefault(
                candidate => candidate.DiseaseId == savedInfection.Disease.DiseaseId);
            if (authoredInfection is null
                || disease is null
                || !ReferenceEquals(authoredInfection.Disease, disease))
            {
                throw Incompatible();
            }

            WorldEventId? progressionEventId = savedInfection.LastProgressionEventId is null
                ? null
                : FindAuthoredEvent(savedInfection.LastProgressionEventId, authoredWorldEvents).EventId;
            infections.Add(new CharacterDiseaseState(
                authoredInfection.CharacterId,
                disease,
                savedInfection.Stage,
                progressionEventId,
                savedInfection.CareState));
        }

        return new DiseaseTruthSnapshot(new DiseaseDefinitionSnapshot(definitions), infections);
    }

    private static PreparedIngredientBatchSnapshot RebindBatches(
        PreparedIngredientBatchSnapshot saved,
        IngredientPreparationRecipe authoredRecipe,
        Dictionary<IngredientPreparationOutcome, IngredientPreparationOutcome> preparations)
    {
        List<PreparedIngredientBatch> batches = [];
        foreach (PreparedIngredientBatch savedBatch in saved.Batches)
        {
            IngredientPreparationOutcome preparation = RebindPreparation(
                savedBatch.Preparation,
                authoredRecipe,
                preparations);
            RequireUnit(savedBatch.RemainingQuantity.Unit, authoredRecipe.ResultQuantity.Unit);
            batches.Add(new PreparedIngredientBatch(
                preparation,
                new IngredientQuantity(
                    savedBatch.RemainingQuantity.Amount,
                    authoredRecipe.ResultQuantity.Unit)));
        }

        return new PreparedIngredientBatchSnapshot(batches);
    }

    private static IngredientExperimentJournalSnapshot RebindAdministrations(
        IngredientExperimentJournalSnapshot saved,
        FirstPatientTreatmentContext authoredContext,
        WorldEventSnapshot authoredWorldEvents,
        Dictionary<IngredientPreparationOutcome, IngredientPreparationOutcome> preparations)
    {
        List<IngredientAdministrationExperimentRecord> records = [];
        IngredientPreparationRecipe authoredRecipe = authoredContext.Scenario.Remedy.Recipe;
        foreach (IngredientAdministrationExperimentRecord savedRecord in saved.Records)
        {
            IngredientAdministrationOutcome savedAdministration = savedRecord.Administration;
            if (savedAdministration.PatientId != authoredContext.Scenario.PatientId)
            {
                throw Incompatible();
            }

            IngredientPreparationOutcome preparation = RebindPreparation(
                savedAdministration.Dosage.Preparation,
                authoredRecipe,
                preparations);
            RequireUnit(
                savedAdministration.Dosage.Quantity.Unit,
                authoredRecipe.ResultQuantity.Unit);
            WorldEvent worldEvent = FindAuthoredEvent(
                savedAdministration.WorldEvent,
                authoredWorldEvents);
            IngredientAdministrationOutcome administration = new(
                savedAdministration.AdministratorId,
                authoredContext.Scenario.PatientId,
                new IngredientDosage(
                    preparation,
                    new IngredientQuantity(
                        savedAdministration.Dosage.Quantity.Amount,
                        authoredRecipe.ResultQuantity.Unit)),
                worldEvent);
            records.Add(new IngredientAdministrationExperimentRecord(
                savedRecord.ExperimentId,
                administration,
                savedRecord.ObservedConditions));
        }

        return new IngredientExperimentJournalSnapshot(records);
    }

    private static ExperimentJournalSnapshot RebindExperiments(
        ExperimentJournalSnapshot saved,
        WorldEventSnapshot authoredWorldEvents)
    {
        List<ExperimentJournalRecord> records = [];
        foreach (ExperimentJournalRecord savedRecord in saved.Records)
        {
            records.Add(new ExperimentJournalRecord(
                savedRecord.ExperimentId,
                FindAuthoredEvent(savedRecord.WorldEvent, authoredWorldEvents),
                savedRecord.Procedure,
                savedRecord.ObservedOutcome));
        }

        return new ExperimentJournalSnapshot(records);
    }

    private static IngredientPreparationOutcome RebindPreparation(
        IngredientPreparationOutcome saved,
        IngredientPreparationRecipe authoredRecipe,
        Dictionary<IngredientPreparationOutcome, IngredientPreparationOutcome> preparations)
    {
        if (preparations.TryGetValue(saved, out IngredientPreparationOutcome? existing))
        {
            return existing;
        }

        IngredientPreparationRecipe savedRecipe = saved.Activity.Recipe;
        if (!IsCompatibleRecipe(savedRecipe, authoredRecipe)
            || saved.Activity.Progress.NextStepIndex != authoredRecipe.Plan.Steps.Count)
        {
            throw Incompatible();
        }

        IngredientPreparationActivity activity = new(
            authoredRecipe,
            new IngredientPreparationProgress(
                authoredRecipe.Plan,
                saved.Activity.Progress.NextStepIndex));
        IngredientPreparationOutcome rebound = new(
            activity,
            authoredRecipe.ResultDefinition,
            authoredRecipe.ResultQuantity);
        preparations.Add(saved, rebound);
        return rebound;
    }

    private static bool IsCompatibleRecipe(
        IngredientPreparationRecipe saved,
        IngredientPreparationRecipe authored)
    {
        if (saved.RecipeId != authored.RecipeId
            || saved.Plan.Steps.Count != authored.Plan.Steps.Count
            || saved.Inputs.Count != authored.Inputs.Count
            || saved.ResultDefinition.IngredientId != authored.ResultDefinition.IngredientId
            || !IsCompatibleQuantity(saved.ResultQuantity, authored.ResultQuantity))
        {
            return false;
        }

        for (int index = 0; index < authored.Plan.Steps.Count; index++)
        {
            if (!ReferenceEquals(saved.Plan.Steps[index].Kind, authored.Plan.Steps[index].Kind))
            {
                return false;
            }
        }

        for (int index = 0; index < authored.Inputs.Count; index++)
        {
            IngredientPreparationInputRequirement savedInput = saved.Inputs[index];
            IngredientPreparationInputRequirement authoredInput = authored.Inputs[index];
            if (savedInput.Definition.IngredientId != authoredInput.Definition.IngredientId
                || !IsCompatibleQuantity(savedInput.Quantity, authoredInput.Quantity))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCompatibleQuantity(
        IngredientQuantity saved,
        IngredientQuantity authored) =>
        saved.Amount == authored.Amount && ReferenceEquals(saved.Unit, authored.Unit);

    private static void RequireUnit(
        IngredientQuantityUnit saved,
        IngredientQuantityUnit authored)
    {
        if (!ReferenceEquals(saved, authored))
        {
            throw Incompatible();
        }
    }

    private static WorldEvent FindAuthoredEvent(
        WorldEvent saved,
        WorldEventSnapshot authoredWorldEvents)
    {
        WorldEvent authored = FindAuthoredEvent(saved.EventId, authoredWorldEvents);
        return ReferenceEquals(saved.TimeEffect, authored.TimeEffect)
            ? authored
            : throw Incompatible();
    }

    private static WorldEvent FindAuthoredEvent(
        WorldEventId savedEventId,
        WorldEventSnapshot authoredWorldEvents) =>
        WorldEventLookup.FindById(savedEventId, authoredWorldEvents) ?? throw Incompatible();

    private static void RequireAdministrationExperimentPairs(
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        foreach (IngredientAdministrationExperimentRecord administration in
            administrationExperiments.Records)
        {
            ExperimentJournalRecord? experiment = experiments.Records.SingleOrDefault(
                candidate => candidate.ExperimentId == administration.ExperimentId);
            if (experiment is null
                || !ReferenceEquals(experiment.WorldEvent, administration.Administration.WorldEvent))
            {
                throw Incompatible();
            }
        }
    }

    private static InvalidOperationException Incompatible() => new(
        "Saved first-patient state is incompatible with the authored definitions.");
}
