using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class FirstPatientAdministrationOccurrenceTransitionTests
{
    [Fact]
    public void OccursTheExactEventAndStabilizesFromTheExactRetainedBatchAtomically()
    {
        Input input = CreateInput();

        FirstPatientAdministrationOccurrenceResult result =
            FirstPatientAdministrationOccurrenceTransition.Occur(
                input.ExperimentId,
                input.Context,
                input.AdministratorId,
                input.WorldEvent,
                input.Batch,
                input.DosageQuantity,
                input.ObservedConditions,
                input.Procedure,
                input.ObservedOutcome,
                input.WorldEvents,
                input.EventLog,
                input.WorldTime,
                input.PreparedBatches,
                input.AdministrationExperiments,
                input.Experiments);

        Assert.Collection(
            result.EventLog.Entries,
            entry => Assert.Same(input.PriorWorldEvent.EventId, entry),
            entry => Assert.Same(input.WorldEvent.EventId, entry));
        Assert.Same(input.WorldEvent.EventId, result.WorldTime.CurrentEventId);
        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(result.DiseaseTruth.Infections).CareState);
        PreparedIngredientBatch remainingBatch = Assert.Single(result.PreparedBatches.Batches);
        Assert.Same(input.Batch.Preparation, remainingBatch.Preparation);
        Assert.Equal(75, remainingBatch.RemainingQuantity.Amount);
        IngredientAdministrationExperimentRecord record = Assert.Single(
            result.AdministrationExperiments.Records);
        Assert.Same(input.ExperimentId, record.ExperimentId);
        Assert.Same(input.AdministratorId, record.Administration.AdministratorId);
        Assert.Same(input.Context.Scenario.PatientId, record.Administration.PatientId);
        Assert.Same(input.Batch.Preparation, record.Administration.Dosage.Preparation);
        Assert.Same(input.DosageQuantity, record.Administration.Dosage.Quantity);
        Assert.Same(input.WorldEvent, record.Administration.WorldEvent);
        Assert.Same(input.ObservedConditions, record.ObservedConditions);
        Assert.Same(input.WorldEvent, Assert.Single(result.Experiments.Records).WorldEvent);
        AssertUnchanged(input);
    }

    [Fact]
    public void RejectsReplacementEventBatchAndUnavailableQuantityWithoutPartialState()
    {
        Input input = CreateInput();
        WorldEvent replacementEvent = new(
            new WorldEventId(input.WorldEvent.EventId.Value),
            input.WorldEvent.TimeEffect);
        PreparedIngredientBatch replacementBatch = new(input.Batch.Preparation);
        IngredientQuantity unavailableQuantity = new(
            input.Batch.RemainingQuantity.Amount,
            input.Batch.RemainingQuantity.Unit);
        IngredientDosage priorDosage = new(
            input.Batch.Preparation,
            new IngredientQuantity(90, input.Batch.RemainingQuantity.Unit));
        PreparedIngredientBatch partiallyConsumedBatch =
            PreparedIngredientBatchConsumptionTransition.Consume(priorDosage, input.Batch)
            ?? throw new InvalidOperationException("The test batch was unexpectedly depleted.");
        PreparedIngredientBatchSnapshot partiallyConsumedBatches = new([partiallyConsumedBatch]);

        Assert.Throws<InvalidOperationException>(() => Occur(input, worldEvent: replacementEvent));
        Assert.Throws<InvalidOperationException>(() => Occur(input, batch: replacementBatch));
        Assert.Throws<InvalidOperationException>(() => Occur(
            input,
            batch: partiallyConsumedBatch,
            dosageQuantity: unavailableQuantity,
            preparedBatches: partiallyConsumedBatches));

        AssertUnchanged(input);
        Assert.Same(partiallyConsumedBatch, Assert.Single(partiallyConsumedBatches.Batches));
        Assert.Equal(10, partiallyConsumedBatch.RemainingQuantity.Amount);
    }

    [Fact]
    public void RejectsStaleRepeatedAndWrongContextInputsWithoutPartialState()
    {
        Input input = CreateInput();
        WorldEventLogSnapshot staleLog = new([input.PriorWorldEvent.EventId]);
        WorldEventLogSnapshot repeatedLog = new(
            [input.PriorWorldEvent.EventId, input.WorldEvent.EventId]);
        FirstPatientTreatmentContext wrongContext =
            RepresentativeFirstPatientTreatmentContextSource.Create();

        Assert.Throws<InvalidOperationException>(() => Occur(input, eventLog: repeatedLog));
        Assert.Throws<InvalidOperationException>(() => Occur(
            input,
            context: wrongContext,
            eventLog: staleLog));

        AssertUnchanged(input);
        Assert.Single(staleLog.Entries);
        Assert.Equal(2, repeatedLog.Entries.Count);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureTransition()
    {
        Input input = CreateInput();
        Type transitionType = typeof(FirstPatientAdministrationOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));
        object?[] validArguments =
        [
            input.ExperimentId,
            input.Context,
            input.AdministratorId,
            input.WorldEvent,
            input.Batch,
            input.DosageQuantity,
            input.ObservedConditions,
            input.Procedure,
            input.ObservedOutcome,
            input.WorldEvents,
            input.EventLog,
            input.WorldTime,
            input.PreparedBatches,
            input.AdministrationExperiments,
            input.Experiments,
        ];

        for (int index = 0; index < validArguments.Length; index++)
        {
            object?[] arguments = validArguments.ToArray();
            arguments[index] = null;
            System.Reflection.TargetInvocationException exception =
                Assert.Throws<System.Reflection.TargetInvocationException>(
                    () => method.Invoke(null, arguments));
            Assert.IsType<ArgumentNullException>(exception.InnerException);
        }

        Assert.Equal("Occur", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(FirstPatientAdministrationOccurrenceResult), method.ReturnType);
        Assert.Equal(
            validArguments.Select(argument => argument!.GetType()),
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
    }

    private static FirstPatientAdministrationOccurrenceResult Occur(
        Input input,
        FirstPatientTreatmentContext? context = null,
        WorldEvent? worldEvent = null,
        PreparedIngredientBatch? batch = null,
        IngredientQuantity? dosageQuantity = null,
        WorldEventLogSnapshot? eventLog = null,
        PreparedIngredientBatchSnapshot? preparedBatches = null) =>
        FirstPatientAdministrationOccurrenceTransition.Occur(
            input.ExperimentId,
            context ?? input.Context,
            input.AdministratorId,
            worldEvent ?? input.WorldEvent,
            batch ?? input.Batch,
            dosageQuantity ?? input.DosageQuantity,
            input.ObservedConditions,
            input.Procedure,
            input.ObservedOutcome,
            input.WorldEvents,
            eventLog ?? input.EventLog,
            input.WorldTime,
            preparedBatches ?? input.PreparedBatches,
            input.AdministrationExperiments,
            input.Experiments);

    private static Input CreateInput()
    {
        FirstPatientTreatmentContext context = RepresentativeFirstPatientTreatmentContextSource.Create();
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientPreparationActivity activity = IngredientPreparationOutcomeTests.Complete(recipe);
        IngredientPreparationOutcome preparation = new(
            activity,
            recipe.ResultDefinition,
            recipe.ResultQuantity);
        PreparedIngredientBatch batch = new(preparation);
        WorldEvent priorWorldEvent = new(
            new WorldEventId("first-patient-preparation-completed"),
            WorldEventTimeEffect.NoAdvance);
        WorldEvent worldEvent = new(
            new WorldEventId("healer-administered-first-patient-remedy"),
            WorldEventTimeEffect.Advance);

        return new Input(
            new ExperimentId("first-patient-remedy-administration"),
            context,
            new CharacterId("healer"),
            priorWorldEvent,
            worldEvent,
            batch,
            new IngredientQuantity(25, batch.RemainingQuantity.Unit),
            new CharacterConditionSet(
                [new CharacterCondition("breathing", "Breathing becomes quieter and steadier.")]),
            "Administer one cooled remedy dose.",
            "The patient's breathing becomes quieter and steadier.",
            new WorldEventSnapshot([priorWorldEvent, worldEvent]),
            new WorldEventLogSnapshot([priorWorldEvent.EventId]),
            new WorldTimeSnapshot(null),
            new PreparedIngredientBatchSnapshot([batch]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
    }

    private static void AssertUnchanged(Input input)
    {
        Assert.Collection(
            input.WorldEvents.Entries,
            entry => Assert.Same(input.PriorWorldEvent, entry),
            entry => Assert.Same(input.WorldEvent, entry));
        Assert.Collection(input.EventLog.Entries, entry => Assert.Same(input.PriorWorldEvent.EventId, entry));
        Assert.Null(input.WorldTime.CurrentEventId);
        Assert.Same(input.Batch, Assert.Single(input.PreparedBatches.Batches));
        Assert.Empty(input.AdministrationExperiments.Records);
        Assert.Empty(input.Experiments.Records);
        Assert.Same(
            DiseaseCareState.Unassisted,
            Assert.Single(input.Context.Scenario.DiseaseTruth.Infections).CareState);
    }

    private sealed record Input(
        ExperimentId ExperimentId,
        FirstPatientTreatmentContext Context,
        CharacterId AdministratorId,
        WorldEvent PriorWorldEvent,
        WorldEvent WorldEvent,
        PreparedIngredientBatch Batch,
        IngredientQuantity DosageQuantity,
        CharacterConditionSet ObservedConditions,
        string Procedure,
        string ObservedOutcome,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        PreparedIngredientBatchSnapshot PreparedBatches,
        IngredientExperimentJournalSnapshot AdministrationExperiments,
        ExperimentJournalSnapshot Experiments);
}
