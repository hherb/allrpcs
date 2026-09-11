using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCureEligibilityPolicyTests
{
    [Fact]
    public void AllowsExactRetainedInfectionAndOccurredAuthoredCureEvent()
    {
        CureInput input = Input();

        Assert.True(DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsReplacementOrAbsentInfection()
    {
        CureInput input = Input();
        CharacterDiseaseState replacement = DiseaseProgressionPolicyTests.State(
            input.Outcome.Infection.Disease,
            input.Outcome.Infection.Stage);
        DiseaseDefinition otherDisease = new(new("marsh-fever"));
        CharacterDiseaseState absent = DiseaseProgressionPolicyTests.State(
            otherDisease,
            DiseaseStage.DeepSleep);

        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            new SuccessfulDiseaseCureOutcome(replacement, input.Outcome.WorldEvent),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            new SuccessfulDiseaseCureOutcome(absent, input.Outcome.WorldEvent),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
    }

    [Fact]
    public void RejectsReplacementAbsentAndUnoccurredEvents()
    {
        CureInput input = Input();
        WorldEvent replacement = DiseaseProgressionPolicyTests.Event(
            input.Outcome.WorldEvent.EventId.Value,
            WorldEventTimeEffect.NoAdvance);
        WorldEvent absent = DiseaseProgressionPolicyTests.Event(
            "different-cure",
            WorldEventTimeEffect.NoAdvance);

        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            new SuccessfulDiseaseCureOutcome(input.Outcome.Infection, replacement),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            new SuccessfulDiseaseCureOutcome(input.Outcome.Infection, absent),
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            new WorldEventLogSnapshot([])));

        WorldEvent later = DiseaseProgressionPolicyTests.Event(
            "patient-rested",
            WorldEventTimeEffect.NoAdvance);
        Assert.False(DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome,
            input.Truth,
            new WorldEventSnapshot([input.Outcome.WorldEvent, later]),
            new WorldEventLogSnapshot([input.Outcome.WorldEvent.EventId, later.EventId])));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        CureInput input = Input();

        Assert.Throws<ArgumentNullException>(() => DiseaseCureEligibilityPolicy.CanApply(
            null!, input.Truth, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome, null!, input.WorldEvents, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome, input.Truth, null!, input.EventLog));
        Assert.Throws<ArgumentNullException>(() => DiseaseCureEligibilityPolicy.CanApply(
            input.Outcome, input.Truth, input.WorldEvents, null!));

        Type policyType = typeof(DiseaseCureEligibilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanApply", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CureInput Input()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            disease,
            DiseaseStage.DeepSleep);
        DiseaseTruthSnapshot truth = DiseaseProgressionPolicyTests.Truth(disease, infection);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-cured",
            WorldEventTimeEffect.NoAdvance);

        return new CureInput(
            new SuccessfulDiseaseCureOutcome(infection, worldEvent),
            truth,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]));
    }

    internal sealed record CureInput(
        SuccessfulDiseaseCureOutcome Outcome,
        DiseaseTruthSnapshot Truth,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog);
}
