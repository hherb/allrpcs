using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Tests.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientStabilizationTransitionTests
{
    [Fact]
    public void StabilizesAndRecordsTheExactObservedAdministrationAtomically()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        ExperimentId experimentId = new("first-patient-cooling-dose");
        CharacterConditionSet observedConditions = new(
            [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]);
        IngredientAdministrationOutcome administration = WithDosage(input.Administration, 25);
        PreparedIngredientBatch batch = new(administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot preparedBatches = new([batch]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        FirstPatientStabilizationResult result = FirstPatientStabilizationTransition.Apply(
            experimentId,
            input.Context,
            administration,
            observedConditions,
            "Administer one cooled remedy dose.",
            "The patient's breathing becomes quieter and steadier.",
            input.WorldEvents,
            input.EventLog,
            preparedBatches,
            administrationExperiments,
            experiments);

        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(result.DiseaseTruth.Infections).CareState);
        PreparedIngredientBatch remainingBatch = Assert.Single(result.PreparedBatches.Batches);
        Assert.Same(batch.Preparation, remainingBatch.Preparation);
        Assert.Equal(75, remainingBatch.RemainingQuantity.Amount);
        IngredientAdministrationExperimentRecord record = Assert.Single(
            result.AdministrationExperiments.Records);
        Assert.Same(experimentId, record.ExperimentId);
        Assert.Same(administration, record.Administration);
        Assert.Same(observedConditions, record.ObservedConditions);
        ExperimentJournalRecord universalRecord = Assert.Single(result.Experiments.Records);
        Assert.Same(experimentId, universalRecord.ExperimentId);
        Assert.Same(administration.WorldEvent, universalRecord.WorldEvent);
        Assert.Equal("Administer one cooled remedy dose.", universalRecord.Procedure);
        Assert.Equal(
            "The patient's breathing becomes quieter and steadier.",
            universalRecord.ObservedOutcome);
        Assert.Same(batch, Assert.Single(preparedBatches.Batches));
        Assert.Empty(administrationExperiments.Records);
        Assert.Empty(experiments.Records);
        Assert.Same(
            DiseaseCareState.Unassisted,
            Assert.Single(input.Scenario.DiseaseTruth.Infections).CareState);
    }

    [Fact]
    public void RejectsUnavailableReplacementOrStaleInputsWithoutReturningPartialState()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        PreparedIngredientBatch exactBatch = new(input.Administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot preparedBatches = new([exactBatch]);
        IngredientPreparationActivity replacementActivity = IngredientPreparationOutcomeTests.Complete(
            input.Scenario.Remedy.Recipe);
        IngredientPreparationOutcome replacementPreparation = new(
            replacementActivity,
            input.Scenario.Remedy.Recipe.ResultDefinition,
            input.Scenario.Remedy.Recipe.ResultQuantity);
        PreparedIngredientBatch replacementBatch = new(replacementPreparation);
        PreparedIngredientBatchSnapshot replacementBatches = new([replacementBatch]);
        PreparedIngredientBatchSnapshot depletedBatches = new([]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<InvalidOperationException>(() => Apply(
            input,
            preparedBatches: replacementBatches,
            administrationExperiments: administrationExperiments,
            experiments: experiments));
        Assert.Throws<InvalidOperationException>(() => Apply(
            input,
            preparedBatches: depletedBatches,
            administrationExperiments: administrationExperiments,
            experiments: experiments));
        Assert.Throws<InvalidOperationException>(() => Apply(
            input,
            preparedBatches: preparedBatches,
            eventLog: new WorldEventLogSnapshot([]),
            administrationExperiments: administrationExperiments,
            experiments: experiments));

        Assert.Same(exactBatch, Assert.Single(preparedBatches.Batches));
        Assert.Same(replacementBatch, Assert.Single(replacementBatches.Batches));
        Assert.Empty(depletedBatches.Batches);
        Assert.Empty(administrationExperiments.Records);
        Assert.Empty(experiments.Records);
        Assert.Same(
            DiseaseCareState.Unassisted,
            Assert.Single(input.Scenario.DiseaseTruth.Infections).CareState);
    }

    [Fact]
    public void RejectsDuplicateFullAndTreatmentIneligibleInputsWithoutReturningPartialState()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        ExperimentId experimentId = new("first-patient-cooling-dose");
        CharacterConditionSet observedConditions = new([]);
        PreparedIngredientBatch batch = new(input.Administration.Dosage.Preparation);
        PreparedIngredientBatchSnapshot preparedBatches = new([batch]);
        IngredientAdministrationExperimentRecord duplicateRecord = new(
            experimentId,
            input.Administration,
            observedConditions);
        IngredientExperimentJournalSnapshot duplicateAdministrationExperiments = new([duplicateRecord]);
        ExperimentJournalSnapshot fullExperiments = new(Enumerable.Range(
            0,
            ExperimentJournalSnapshot.MaximumRecordCount).Select(index => UniversalRecord(index)));
        FirstPatientScenario ineligibleScenario = new(
            input.Scenario.PatientId,
            new DiseaseTruthSnapshot(input.Scenario.DiseaseTruth.Definitions, []),
            input.Scenario.Ingredients,
            input.Scenario.Remedy,
            input.Scenario.Investigation);
        FirstPatientTreatmentContext ineligibleContext = new(
            ineligibleScenario,
            input.Context.EvidenceRequirement);

        Assert.Throws<ArgumentException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId,
            input.Context,
            input.Administration,
            observedConditions,
            "Administer the cooled remedy.",
            "The patient remains under observation.",
            input.WorldEvents,
            input.EventLog,
            preparedBatches,
            duplicateAdministrationExperiments,
            new ExperimentJournalSnapshot([])));
        Assert.Throws<InvalidOperationException>(() => Apply(
            input,
            preparedBatches: preparedBatches,
            administrationExperiments: new IngredientExperimentJournalSnapshot([]),
            experiments: fullExperiments));
        Assert.Throws<InvalidOperationException>(() => Apply(
            input,
            context: ineligibleContext,
            preparedBatches: preparedBatches,
            administrationExperiments: new IngredientExperimentJournalSnapshot([]),
            experiments: new ExperimentJournalSnapshot([])));

        Assert.Same(batch, Assert.Single(preparedBatches.Batches));
        Assert.Same(duplicateRecord, Assert.Single(duplicateAdministrationExperiments.Records));
        Assert.Equal(ExperimentJournalSnapshot.MaximumRecordCount, fullExperiments.Records.Count);
        Assert.Same(
            DiseaseCareState.Unassisted,
            Assert.Single(input.Scenario.DiseaseTruth.Infections).CareState);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureTransition()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        ExperimentId experimentId = new("first-patient-cooling-dose");
        CharacterConditionSet observedConditions = new([]);
        PreparedIngredientBatchSnapshot preparedBatches = new(
            [new PreparedIngredientBatch(input.Administration.Dosage.Preparation)]);
        IngredientExperimentJournalSnapshot administrationExperiments = new([]);
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            null!, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, null!, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, null!, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, null!,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            null!, "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", null!, input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", null!, input.EventLog,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, null!,
            preparedBatches, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            null!, administrationExperiments, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, null!, experiments));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            "Procedure", "Outcome", input.WorldEvents, input.EventLog,
            preparedBatches, administrationExperiments, null!));

        Type transitionType = typeof(FirstPatientStabilizationTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(FirstPatientStabilizationResult), method.ReturnType);
        Assert.Equal(
            [
                typeof(ExperimentId),
                typeof(FirstPatientTreatmentContext),
                typeof(IngredientAdministrationOutcome),
                typeof(CharacterConditionSet),
                typeof(string),
                typeof(string),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(PreparedIngredientBatchSnapshot),
                typeof(IngredientExperimentJournalSnapshot),
                typeof(ExperimentJournalSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
    }

    private static FirstPatientStabilizationResult Apply(
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input,
        FirstPatientTreatmentContext? context = null,
        WorldEventLogSnapshot? eventLog = null,
        PreparedIngredientBatchSnapshot? preparedBatches = null,
        IngredientExperimentJournalSnapshot? administrationExperiments = null,
        ExperimentJournalSnapshot? experiments = null) => FirstPatientStabilizationTransition.Apply(
            new ExperimentId("first-patient-cooling-dose"),
            context ?? input.Context,
            input.Administration,
            new CharacterConditionSet([]),
            "Administer the cooled remedy.",
            "The patient remains under observation.",
            input.WorldEvents,
            eventLog ?? input.EventLog,
            preparedBatches ?? new PreparedIngredientBatchSnapshot(
                [new PreparedIngredientBatch(input.Administration.Dosage.Preparation)]),
            administrationExperiments ?? new IngredientExperimentJournalSnapshot([]),
            experiments ?? new ExperimentJournalSnapshot([]));

    private static IngredientAdministrationOutcome WithDosage(
        IngredientAdministrationOutcome administration,
        int amount) => new(
            administration.AdministratorId,
            administration.PatientId,
            new IngredientDosage(
                administration.Dosage.Preparation,
                new IngredientQuantity(amount, administration.Dosage.Quantity.Unit)),
            administration.WorldEvent);

    private static ExperimentJournalRecord UniversalRecord(int index)
    {
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            $"prior-experiment-{index}",
            WorldEventTimeEffect.NoAdvance);
        return new ExperimentJournalRecord(
            new ExperimentId($"prior-experiment-{index}"),
            worldEvent,
            "Prior procedure.",
            "Prior observed outcome.");
    }
}
