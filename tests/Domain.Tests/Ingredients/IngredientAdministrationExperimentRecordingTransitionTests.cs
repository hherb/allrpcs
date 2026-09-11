using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientAdministrationExperimentRecordingTransitionTests
{
    [Fact]
    public void ConstructsAndAppendsOneExactRecordAfterExistingRecords()
    {
        IngredientAdministrationExperimentRecord first =
            IngredientExperimentJournalSnapshotTests.Record("first-dose");
        IngredientExperimentJournalSnapshot journal = new([first]);
        IngredientAdministrationExperimentRecordingPolicyTests.RecordingInput input =
            IngredientAdministrationExperimentRecordingPolicyTests.Input();
        ExperimentId experimentId = new("second-dose");
        CharacterConditionSet observation = new(
            [new CharacterCondition("appearance", "Breathing is quieter.")]);

        IngredientExperimentJournalSnapshot result =
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId,
                input.Administration,
                observation,
                input.WorldEvents,
                input.EventLog,
                journal);

        IngredientAdministrationExperimentRecord recorded = result.Records[1];
        Assert.NotSame(journal, result);
        Assert.Collection(
            result.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(recorded, record));
        Assert.Same(experimentId, recorded.ExperimentId);
        Assert.Same(input.Administration, recorded.Administration);
        Assert.Same(observation, recorded.ObservedConditions);
        Assert.Same(first, Assert.Single(journal.Records));
    }

    [Fact]
    public void RejectsIneligibleEventDuplicateAndCapacityAtomically()
    {
        IngredientAdministrationExperimentRecordingPolicyTests.RecordingInput input =
            IngredientAdministrationExperimentRecordingPolicyTests.Input();
        CharacterConditionSet observation = new([]);
        IngredientAdministrationExperimentRecord retained =
            IngredientExperimentJournalSnapshotTests.Record("first-dose");
        IngredientExperimentJournalSnapshot journal = new([retained]);
        IngredientExperimentJournalSnapshot full = new(
            Enumerable
                .Range(0, IngredientExperimentJournalSnapshot.MaximumRecordCount)
                .Select(index => IngredientExperimentJournalSnapshotTests.Record(
                    $"experiment-{index}")));

        Assert.Throws<InvalidOperationException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                new ExperimentId("second-dose"),
                input.Administration,
                observation,
                input.WorldEvents,
                new WorldEventLogSnapshot([]),
                journal));
        Assert.Throws<ArgumentException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                retained.ExperimentId,
                input.Administration,
                observation,
                input.WorldEvents,
                input.EventLog,
                journal));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                new ExperimentId("extra-record"),
                input.Administration,
                observation,
                input.WorldEvents,
                input.EventLog,
                full));

        Assert.Same(retained, Assert.Single(journal.Records));
        Assert.Equal(IngredientExperimentJournalSnapshot.MaximumRecordCount, full.Records.Count);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureTransition()
    {
        IngredientAdministrationExperimentRecordingPolicyTests.RecordingInput input =
            IngredientAdministrationExperimentRecordingPolicyTests.Input();
        ExperimentId experimentId = new("first-dose");
        CharacterConditionSet observation = new([]);
        IngredientExperimentJournalSnapshot journal = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                null!, input.Administration, observation, input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId, null!, observation, input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId, input.Administration, null!, input.WorldEvents, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId, input.Administration, observation, null!, input.EventLog, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId, input.Administration, observation, input.WorldEvents, null!, journal));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId, input.Administration, observation, input.WorldEvents, input.EventLog, null!));

        Type transitionType = typeof(IngredientAdministrationExperimentRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
        Assert.Equal(typeof(IngredientExperimentJournalSnapshot), method.ReturnType);
        Assert.Equal(
            [
                typeof(ExperimentId),
                typeof(IngredientAdministrationOutcome),
                typeof(CharacterConditionSet),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(IngredientExperimentJournalSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
