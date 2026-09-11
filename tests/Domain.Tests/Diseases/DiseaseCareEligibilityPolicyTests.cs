using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCareEligibilityPolicyTests
{
    [Fact]
    public void AllowsAnExactStrongerOutcomeAtTheCurrentAuthoredEvent()
    {
        CareInput input = Input(DiseaseCareState.Unassisted, DiseaseCareState.Comforted);

        Assert.True(DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsRepeatedOrWeakerCare()
    {
        CareInput comfortedAgain = Input(
            DiseaseCareState.Comforted,
            DiseaseCareState.Comforted);
        CareInput stabilizedToComforted = Input(
            DiseaseCareState.Stabilized,
            DiseaseCareState.Comforted);
        CareInput stabilizedAgain = Input(
            DiseaseCareState.Stabilized,
            DiseaseCareState.Stabilized);

        Assert.False(CanApply(comfortedAgain));
        Assert.False(CanApply(stabilizedToComforted));
        Assert.False(CanApply(stabilizedAgain));
        Assert.True(CanApply(Input(
            DiseaseCareState.Comforted,
            DiseaseCareState.Stabilized)));
    }

    [Fact]
    public void RejectsReplacementInfectionAndAbsentOrNoncurrentEvent()
    {
        CareInput input = Input(DiseaseCareState.Unassisted, DiseaseCareState.Stabilized);
        CharacterDiseaseState replacement = DiseaseProgressionPolicyTests.State(
            input.Outcome.Infection.Disease,
            input.Outcome.Infection.Stage);
        WorldEvent replacementEvent = DiseaseProgressionPolicyTests.Event(
            input.Outcome.WorldEvent.EventId.Value,
            WorldEventTimeEffect.NoAdvance);

        Assert.False(DiseaseCareEligibilityPolicy.CanApply(
            new DiseaseCareOutcome(
                replacement,
                DiseaseCareState.Stabilized,
                input.Outcome.WorldEvent),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseCareEligibilityPolicy.CanApply(
            new DiseaseCareOutcome(
                input.Outcome.Infection,
                DiseaseCareState.Stabilized,
                replacementEvent),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));

        WorldEvent later = DiseaseProgressionPolicyTests.Event(
            "patient-rested",
            WorldEventTimeEffect.NoAdvance);
        Assert.False(DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            new WorldEventSnapshot([input.Outcome.WorldEvent, later]),
            new WorldEventLogSnapshot([input.Outcome.WorldEvent.EventId, later.EventId])));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        CareInput input = Input(DiseaseCareState.Unassisted, DiseaseCareState.Comforted);

        Assert.Throws<ArgumentNullException>(() => DiseaseCareEligibilityPolicy.CanApply(
            null!, input.Truth, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome, input.Truth, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCareEligibilityPolicy.CanApply(
            input.Outcome, input.Truth, input.WorldEvents, null!));

        Type policyType = typeof(DiseaseCareEligibilityPolicy);
        Assert.Single(policyType.GetMethods().Where(method => method.DeclaringType == policyType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CareInput Input(
        DiseaseCareState current,
        DiseaseCareState resulting)
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = new(
            new("patient"),
            disease,
            DiseaseStage.DeepSleep,
            null,
            current);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-treated",
            WorldEventTimeEffect.NoAdvance);

        return new CareInput(
            new DiseaseCareOutcome(infection, resulting, worldEvent),
            DiseaseProgressionPolicyTests.Truth(disease, infection),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]));
    }

    private static bool CanApply(CareInput input) => DiseaseCareEligibilityPolicy.CanApply(
        input.Outcome,
        input.Truth,
        input.WorldEvents,
        input.EventLog);

    internal sealed record CareInput(
        DiseaseCareOutcome Outcome,
        DiseaseTruthSnapshot Truth,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog);
}
