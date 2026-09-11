using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCareOutcomeTests
{
    [Theory]
    [MemberData(nameof(EffectiveCareStates))]
    public void RetainsExactInfectionCareStateAndEvent(DiseaseCareState careState)
    {
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            DiseaseProgressionPolicyTests.Definition(),
            DiseaseStage.DeepSleep);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-treated",
            WorldEventTimeEffect.NoAdvance);

        DiseaseCareOutcome outcome = new(infection, careState, worldEvent);

        Assert.Same(infection, outcome.Infection);
        Assert.Same(careState, outcome.ResultingCareState);
        Assert.Same(worldEvent, outcome.WorldEvent);
    }

    [Fact]
    public void RejectsNullsAndAnOutcomeThatRemovesCare()
    {
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            DiseaseProgressionPolicyTests.Definition(),
            DiseaseStage.DeepSleep);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "patient-treated",
            WorldEventTimeEffect.NoAdvance);

        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseCareOutcome(null!, DiseaseCareState.Comforted, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseCareOutcome(infection, null!, worldEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new DiseaseCareOutcome(infection, DiseaseCareState.Comforted, null!));
        Assert.Throws<ArgumentException>(() =>
            new DiseaseCareOutcome(infection, DiseaseCareState.Unassisted, worldEvent));
    }

    public static TheoryData<DiseaseCareState> EffectiveCareStates => new()
    {
        DiseaseCareState.Comforted,
        DiseaseCareState.Stabilized,
    };
}
