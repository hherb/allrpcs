using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
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
        IngredientExperimentJournalSnapshot journal = new([]);

        FirstPatientStabilizationResult result = FirstPatientStabilizationTransition.Apply(
            experimentId,
            input.Context,
            input.Administration,
            observedConditions,
            input.WorldEvents,
            input.EventLog,
            journal);

        Assert.Same(DiseaseCareState.Stabilized, Assert.Single(result.DiseaseTruth.Infections).CareState);
        IngredientAdministrationExperimentRecord record = Assert.Single(result.ExperimentJournal.Records);
        Assert.Same(experimentId, record.ExperimentId);
        Assert.Same(input.Administration, record.Administration);
        Assert.Same(observedConditions, record.ObservedConditions);
        Assert.Empty(journal.Records);
        Assert.Same(
            DiseaseCareState.Unassisted,
            Assert.Single(input.Scenario.DiseaseTruth.Infections).CareState);
    }

    [Fact]
    public void RejectsDuplicateRecordOrIneligibleCareWithoutReturningPartialState()
    {
        StabilizingRemedyAdministrationPolicyTests.AdministrationInput input =
            StabilizingRemedyAdministrationPolicyTests.Input();
        ExperimentId experimentId = new("first-patient-cooling-dose");
        CharacterConditionSet observedConditions = new([]);
        IngredientAdministrationExperimentRecord retained = new(
            experimentId,
            input.Administration,
            observedConditions);
        IngredientExperimentJournalSnapshot duplicateJournal = new([retained]);

        Assert.Throws<ArgumentException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId,
            input.Context,
            input.Administration,
            observedConditions,
            input.WorldEvents,
            input.EventLog,
            duplicateJournal));
        Assert.Throws<InvalidOperationException>(() => FirstPatientStabilizationTransition.Apply(
            new ExperimentId("noncurrent-dose"),
            input.Context,
            input.Administration,
            observedConditions,
            input.WorldEvents,
            new WorldEventLogSnapshot([]),
            new IngredientExperimentJournalSnapshot([])));

        Assert.Same(retained, Assert.Single(duplicateJournal.Records));
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
        IngredientExperimentJournalSnapshot journal = new([]);

        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            null!, input.Context, input.Administration, observedConditions,
            input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, null!, input.Administration, observedConditions,
            input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, null!, observedConditions,
            input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, null!,
            input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            null!, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            input.WorldEvents, null!, journal));
        Assert.Throws<ArgumentNullException>(() => FirstPatientStabilizationTransition.Apply(
            experimentId, input.Context, input.Administration, observedConditions,
            input.WorldEvents, input.EventLog, null!));

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
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(IngredientExperimentJournalSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
    }
}
