using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Tests.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Ingredients;

public sealed class IngredientAdministrationExperimentRecordingPolicyTests
{
    [Fact]
    public void AllowsTheExactAuthoredAdministrationEventWhenItIsTheLatestOccurrence()
    {
        RecordingInput input = Input();

        Assert.True(IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            input.Administration,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsAbsentAndValueEqualReplacementEvents()
    {
        RecordingInput input = Input();
        WorldEvent replacement = DiseaseProgressionPolicyTests.Event(
            input.Administration.WorldEvent.EventId.Value,
            WorldEventTimeEffect.NoAdvance);
        WorldEvent absent = DiseaseProgressionPolicyTests.Event(
            "different-administration",
            WorldEventTimeEffect.NoAdvance);

        Assert.False(IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            Administration(replacement),
            input.WorldEvents,
            input.EventLog));
        Assert.False(IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            Administration(absent),
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsEmptyAndStaleEventLogs()
    {
        RecordingInput input = Input();
        WorldEvent later = DiseaseProgressionPolicyTests.Event(
            "patient-rested",
            WorldEventTimeEffect.NoAdvance);

        Assert.False(IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            input.Administration,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));
        Assert.False(IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            input.Administration,
            new WorldEventSnapshot([input.Administration.WorldEvent, later]),
            new WorldEventLogSnapshot([input.Administration.WorldEvent.EventId, later.EventId])));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        RecordingInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingPolicy.CanRecord(
                null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingPolicy.CanRecord(
                input.Administration, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() =>
            IngredientAdministrationExperimentRecordingPolicy.CanRecord(
                input.Administration, input.WorldEvents, null!));

        Type policyType = typeof(IngredientAdministrationExperimentRecordingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanRecord", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(IngredientAdministrationOutcome),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    internal static RecordingInput Input()
    {
        IngredientAdministrationOutcome administration =
            IngredientAdministrationExperimentRecordTests.Administration();
        return new RecordingInput(
            administration,
            new WorldEventSnapshot([administration.WorldEvent]),
            new WorldEventLogSnapshot([administration.WorldEvent.EventId]));
    }

    private static IngredientAdministrationOutcome Administration(WorldEvent worldEvent)
    {
        IngredientAdministrationOutcome source =
            IngredientAdministrationExperimentRecordTests.Administration();
        return new IngredientAdministrationOutcome(
            source.AdministratorId,
            source.PatientId,
            source.Dosage,
            worldEvent);
    }

    internal sealed record RecordingInput(
        IngredientAdministrationOutcome Administration,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog);
}
