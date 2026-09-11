using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class FirstPatientSaveStateRestorationTransitionTests
{
    [Fact]
    public void RebindsCompleteSavedStateToExactAuthoredDefinitionsWithoutChangingInputs()
    {
        RestorationInput input = Input();
        FirstPatientSaveState saved = SavedState(input);
        CharacterDiseaseState savedInfection = Assert.Single(saved.DiseaseTruth.Infections);
        PreparedIngredientBatch savedBatch = Assert.Single(saved.PreparedBatches.Batches);
        IngredientAdministrationExperimentRecord savedAdministration = Assert.Single(
            saved.AdministrationExperiments.Records);
        ExperimentJournalRecord savedExperiment = Assert.Single(saved.Experiments.Records);

        FirstPatientSaveState result = FirstPatientSaveStateRestorationTransition.Rebind(
            saved,
            input.Context,
            input.WorldEvents);

        CharacterDiseaseState infection = Assert.Single(result.DiseaseTruth.Infections);
        DiseaseDefinition authoredDisease = Assert.Single(
            input.Context.Scenario.DiseaseTruth.Definitions.Definitions);
        PreparedIngredientBatch batch = Assert.Single(result.PreparedBatches.Batches);
        IngredientPreparationRecipe authoredRecipe = input.Context.Scenario.Remedy.Recipe;
        IngredientAdministrationExperimentRecord administration = Assert.Single(
            result.AdministrationExperiments.Records);
        ExperimentJournalRecord experiment = Assert.Single(result.Experiments.Records);
        Assert.NotSame(saved, result);
        Assert.NotSame(saved.DiseaseTruth, result.DiseaseTruth);
        Assert.Same(authoredDisease, Assert.Single(result.DiseaseTruth.Definitions.Definitions));
        Assert.Same(input.Context.Scenario.PatientId, infection.CharacterId);
        Assert.Same(authoredDisease, infection.Disease);
        Assert.Same(savedInfection.Stage, infection.Stage);
        Assert.Same(savedInfection.CareState, infection.CareState);
        Assert.Same(input.AdministrationEvent.EventId, infection.LastProgressionEventId);
        Assert.NotSame(savedBatch, batch);
        Assert.NotSame(savedBatch.Preparation, batch.Preparation);
        Assert.Same(authoredRecipe, batch.Preparation.Activity.Recipe);
        Assert.Same(authoredRecipe.Plan, batch.Preparation.Activity.Progress.Plan);
        Assert.Equal(
            authoredRecipe.Plan.Steps,
            batch.Preparation.Activity.Progress.Plan.Steps);
        Assert.Same(authoredRecipe.ResultDefinition, batch.Preparation.Definition);
        Assert.Same(authoredRecipe.ResultQuantity, batch.Preparation.Quantity);
        Assert.Equal(savedBatch.RemainingQuantity.Amount, batch.RemainingQuantity.Amount);
        Assert.Same(authoredRecipe.ResultQuantity.Unit, batch.RemainingQuantity.Unit);
        Assert.NotSame(savedAdministration, administration);
        Assert.Same(batch.Preparation, administration.Administration.Dosage.Preparation);
        Assert.Same(input.Context.Scenario.PatientId, administration.Administration.PatientId);
        Assert.Same(input.AdministrationEvent, administration.Administration.WorldEvent);
        Assert.Same(
            authoredRecipe.ResultQuantity.Unit,
            administration.Administration.Dosage.Quantity.Unit);
        Assert.Same(savedAdministration.ObservedConditions, administration.ObservedConditions);
        Assert.NotSame(savedExperiment, experiment);
        Assert.Same(input.AdministrationEvent, experiment.WorldEvent);
        Assert.Same(administration.Administration.WorldEvent, experiment.WorldEvent);
        Assert.Equal(savedExperiment.Procedure, experiment.Procedure);
        Assert.Equal(savedExperiment.ObservedOutcome, experiment.ObservedOutcome);
        Assert.Same(savedInfection, Assert.Single(saved.DiseaseTruth.Infections));
        Assert.Same(savedBatch, Assert.Single(saved.PreparedBatches.Batches));
        Assert.Same(savedAdministration, Assert.Single(saved.AdministrationExperiments.Records));
        Assert.Same(savedExperiment, Assert.Single(saved.Experiments.Records));
        Assert.Same(input.AuthoredInfection, Assert.Single(
            input.Context.Scenario.DiseaseTruth.Infections));
    }

    [Fact]
    public void RejectsUnknownDiseaseInfectionAndPreparationDefinitionsAtomically()
    {
        RestorationInput input = Input();
        FirstPatientSaveState valid = SavedState(input);

        AssertRejected(input, SavedState(input, diseaseId: "other-disease"));
        AssertRejected(input, SavedState(input, patientId: "other-patient"));
        AssertRejected(input, SavedState(input, recipeId: "other-recipe"));
        AssertRejected(input, SavedState(input, inputIngredientId: "other-ingredient"));
        AssertRejected(
            input,
            SavedState(input, firstStep: IngredientPreparationKind.Heating));
        AssertRejected(
            input,
            SavedState(input, resultUnit: IngredientQuantityUnit.Count));
        Assert.Same(input.SavedDisease, Assert.Single(
            valid.DiseaseTruth.Definitions.Definitions));
        Assert.Same(input.SavedPreparation, Assert.Single(valid.PreparedBatches.Batches).Preparation);
    }

    [Fact]
    public void RejectsUnknownOrIncompatibleProgressionAdministrationAndExperimentEvents()
    {
        RestorationInput input = Input();

        AssertRejected(input, SavedState(input, progressionEventId: "unknown-event"));
        AssertRejected(input, SavedState(input, administrationEventId: "unknown-event"));
        AssertRejected(input, SavedState(input, experimentEventId: "unknown-event"));
        AssertRejected(
            input,
            SavedState(input, administrationTimeEffect: WorldEventTimeEffect.NoAdvance));
        AssertRejected(input, SavedState(input, experimentId: "different-experiment"));
    }

    [Fact]
    public void RejectsNullsInvalidAuthoredContextAndExposesOnlyThePureOperation()
    {
        RestorationInput input = Input();
        FirstPatientSaveState saved = SavedState(input);
        FirstPatientScenario scenario = input.Context.Scenario;
        FirstPatientTreatmentContext invalidContext = new(
            new FirstPatientScenario(
                new CharacterId(scenario.PatientId.Value),
                scenario.DiseaseTruth,
                scenario.Ingredients,
                scenario.Remedy,
                scenario.Investigation),
            input.Context.EvidenceRequirement,
            input.Context.Conclusions);

        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientSaveStateRestorationTransition.Rebind(null!, input.Context, input.WorldEvents));
        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientSaveStateRestorationTransition.Rebind(saved, null!, input.WorldEvents));
        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientSaveStateRestorationTransition.Rebind(saved, input.Context, null!));
        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientSaveStateRestorationTransition.Rebind(saved, invalidContext, input.WorldEvents));

        Type transitionType = typeof(FirstPatientSaveStateRestorationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(FirstPatientSaveStateRestorationTransition.Rebind), method.Name);
        Assert.Equal(typeof(FirstPatientSaveState), method.ReturnType);
        Assert.Equal(
            [typeof(FirstPatientSaveState), typeof(FirstPatientTreatmentContext), typeof(WorldEventSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
    }

    private static void AssertRejected(RestorationInput input, FirstPatientSaveState saved)
    {
        PreparedIngredientBatch batch = Assert.Single(saved.PreparedBatches.Batches);
        IngredientAdministrationExperimentRecord administration = Assert.Single(
            saved.AdministrationExperiments.Records);
        ExperimentJournalRecord experiment = Assert.Single(saved.Experiments.Records);

        Assert.Throws<InvalidOperationException>(() =>
            FirstPatientSaveStateRestorationTransition.Rebind(
                saved,
                input.Context,
                input.WorldEvents));
        Assert.Same(batch, Assert.Single(saved.PreparedBatches.Batches));
        Assert.Same(administration, Assert.Single(saved.AdministrationExperiments.Records));
        Assert.Same(experiment, Assert.Single(saved.Experiments.Records));
        Assert.Same(input.AuthoredInfection, Assert.Single(
            input.Context.Scenario.DiseaseTruth.Infections));
    }

    private static RestorationInput Input()
    {
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        WorldEvent administrationEvent = new(
            new WorldEventId("healer-administered-first-patient-remedy"),
            WorldEventTimeEffect.Advance);
        DiseaseDefinition savedDisease = new(new DiseaseId("grey-sleep"));
        IngredientPreparationOutcome savedPreparation = CreatePreparation(context);
        return new RestorationInput(
            context,
            new WorldEventSnapshot([administrationEvent]),
            administrationEvent,
            Assert.Single(context.Scenario.DiseaseTruth.Infections),
            savedDisease,
            savedPreparation);
    }

    private static FirstPatientSaveState SavedState(
        RestorationInput input,
        string diseaseId = "grey-sleep",
        string patientId = "first-sleeping-patient",
        string recipeId = "cooling-moonwort-remedy",
        string inputIngredientId = "moonwort-leaf",
        IngredientPreparationKind? firstStep = null,
        IngredientQuantityUnit? resultUnit = null,
        string progressionEventId = "healer-administered-first-patient-remedy",
        string administrationEventId = "healer-administered-first-patient-remedy",
        string experimentEventId = "healer-administered-first-patient-remedy",
        WorldEventTimeEffect? administrationTimeEffect = null,
        string experimentId = "first-patient-remedy-administration")
    {
        DiseaseDefinition disease = diseaseId == input.SavedDisease.DiseaseId.Value
            ? input.SavedDisease
            : new DiseaseDefinition(new DiseaseId(diseaseId));
        IngredientPreparationOutcome preparation = recipeId == "cooling-moonwort-remedy"
            && inputIngredientId == "moonwort-leaf"
            && firstStep is null
            && resultUnit is null
                ? input.SavedPreparation
                : CreatePreparation(
                    input.Context,
                    recipeId,
                    inputIngredientId,
                    firstStep,
                    resultUnit);
        PreparedIngredientBatch batch = PreparedIngredientBatchConsumptionTransition.Consume(
            new IngredientDosage(
                preparation,
                new IngredientQuantity(25, preparation.Quantity.Unit)),
            new PreparedIngredientBatch(preparation))!;
        WorldEvent administrationEvent = new(
            new WorldEventId(administrationEventId),
            administrationTimeEffect ?? WorldEventTimeEffect.Advance);
        IngredientAdministrationOutcome administration = new(
            new CharacterId("healer"),
            new CharacterId(patientId),
            new IngredientDosage(
                preparation,
                new IngredientQuantity(25, preparation.Quantity.Unit)),
            administrationEvent);
        ExperimentId administrationExperimentId = new("first-patient-remedy-administration");

        return new FirstPatientSaveState(
            new DiseaseTruthSnapshot(
                new DiseaseDefinitionSnapshot([disease]),
                [
                    new CharacterDiseaseState(
                        new CharacterId(patientId),
                        disease,
                        DiseaseStage.DeepSleep,
                        new WorldEventId(progressionEventId),
                        DiseaseCareState.Stabilized),
                ]),
            new PreparedIngredientBatchSnapshot([batch]),
            new IngredientExperimentJournalSnapshot(
                [
                    new IngredientAdministrationExperimentRecord(
                        administrationExperimentId,
                        administration,
                        new CharacterConditionSet(
                            [new CharacterCondition("breathing", "Breathing is steadier.")])),
                ]),
            new ExperimentJournalSnapshot(
                [
                    new ExperimentJournalRecord(
                        new ExperimentId(experimentId),
                        experimentEventId == administrationEventId
                            ? administrationEvent
                            : new WorldEvent(
                                new WorldEventId(experimentEventId),
                                WorldEventTimeEffect.Advance),
                        "Administer one cooled remedy dose.",
                        "The patient's breathing becomes quieter and steadier."),
                ]));
    }

    private static IngredientPreparationOutcome CreatePreparation(
        FirstPatientTreatmentContext context,
        string recipeId = "cooling-moonwort-remedy",
        string inputIngredientId = "moonwort-leaf",
        IngredientPreparationKind? firstStep = null,
        IngredientQuantityUnit? resultUnit = null)
    {
        IngredientPreparationRecipe authored = context.Scenario.Remedy.Recipe;
        IngredientPreparationPlan plan = new(authored.Plan.Steps.Select((step, index) =>
            new IngredientPreparationStep(index == 0 && firstStep is not null ? firstStep : step.Kind)));
        IngredientDefinition result = new(
            new IngredientId(authored.ResultDefinition.IngredientId.Value));
        IngredientQuantity quantity = new(
            authored.ResultQuantity.Amount,
            resultUnit ?? authored.ResultQuantity.Unit);
        IngredientPreparationRecipe recipe = new(
            new PreparationRecipeId(recipeId),
            plan,
            authored.Inputs.Select((requirement, index) =>
                new IngredientPreparationInputRequirement(
                    new IngredientDefinition(new IngredientId(
                        index == 0
                            ? inputIngredientId
                            : requirement.Definition.IngredientId.Value)),
                    new IngredientQuantity(
                        requirement.Quantity.Amount,
                        requirement.Quantity.Unit))),
            result,
            quantity);
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(plan, plan.Steps.Count));
        return new IngredientPreparationOutcome(activity, result, quantity);
    }

    private sealed record RestorationInput(
        FirstPatientTreatmentContext Context,
        WorldEventSnapshot WorldEvents,
        WorldEvent AdministrationEvent,
        CharacterDiseaseState AuthoredInfection,
        DiseaseDefinition SavedDisease,
        IngredientPreparationOutcome SavedPreparation);
}
