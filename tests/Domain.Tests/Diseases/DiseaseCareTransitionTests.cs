using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCareTransitionTests
{
    [Fact]
    public void ReplacesOnlyTheExactInfectionWithTheAuthoredCareState()
    {
        DiseaseCareEligibilityPolicyTests.CareInput input =
            DiseaseCareEligibilityPolicyTests.Input(
                DiseaseCareState.Unassisted,
                DiseaseCareState.Stabilized);
        CharacterDiseaseState unrelated = new(
            new("other-patient"),
            input.Outcome.Infection.Disease,
            DiseaseStage.EarlyIllness,
            null,
            DiseaseCareState.Unassisted);
        DiseaseTruthSnapshot truth = new(
            input.Truth.Definitions,
            [unrelated, input.Outcome.Infection]);

        DiseaseTruthSnapshot result = DiseaseCareTransition.Apply(
            input.Outcome,
            truth,
            input.WorldEvents,
            input.EventLog);

        Assert.Same(input.Truth.Definitions, result.Definitions);
        Assert.Same(unrelated, result.Infections[0]);
        CharacterDiseaseState stabilized = result.Infections[1];
        Assert.NotSame(input.Outcome.Infection, stabilized);
        Assert.Same(input.Outcome.Infection.CharacterId, stabilized.CharacterId);
        Assert.Same(input.Outcome.Infection.Disease, stabilized.Disease);
        Assert.Same(input.Outcome.Infection.Stage, stabilized.Stage);
        Assert.Same(
            input.Outcome.Infection.LastProgressionEventId,
            stabilized.LastProgressionEventId);
        Assert.Same(DiseaseCareState.Stabilized, stabilized.CareState);
    }

    [Fact]
    public void RejectsIneligibleOutcomeWithoutChangingTruth()
    {
        DiseaseCareEligibilityPolicyTests.CareInput input =
            DiseaseCareEligibilityPolicyTests.Input(
                DiseaseCareState.Comforted,
                DiseaseCareState.Comforted);

        Assert.Throws<InvalidOperationException>(() => DiseaseCareTransition.Apply(
            input.Outcome,
            input.Truth,
            input.WorldEvents,
            input.EventLog));
        Assert.Same(input.Outcome.Infection, Assert.Single(input.Truth.Infections));
        Assert.Same(DiseaseCareState.Comforted, input.Outcome.Infection.CareState);
    }
}
