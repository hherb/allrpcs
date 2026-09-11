using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProgressionTransitionTests
{
    public static TheoryData<DiseaseStage, DiseaseStage> Progressions => new()
    {
        { DiseaseStage.Exposed, DiseaseStage.EarlyIllness },
        { DiseaseStage.EarlyIllness, DiseaseStage.Drowsing },
        { DiseaseStage.Drowsing, DiseaseStage.DeepSleep },
        { DiseaseStage.DeepSleep, DiseaseStage.Wasting },
        { DiseaseStage.Wasting, DiseaseStage.FinalDecline },
    };

    [Theory]
    [MemberData(nameof(Progressions))]
    public void AdvancesExactlyOneCanonicalStage(
        DiseaseStage current,
        DiseaseStage expected)
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(disease, current);
        DiseaseTruthSnapshot truth = DiseaseProgressionPolicyTests.Truth(disease, infection);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "first-night",
            WorldEventTimeEffect.Advance);

        DiseaseTruthSnapshot result = DiseaseProgressionTransition.Advance(
            infection,
            truth,
            worldEvent,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]),
            new WorldTimeSnapshot(worldEvent.EventId));

        CharacterDiseaseState advanced = Assert.Single(result.Infections);
        Assert.Same(truth.Definitions, result.Definitions);
        Assert.Same(infection.CharacterId, advanced.CharacterId);
        Assert.Same(infection.Disease, advanced.Disease);
        Assert.Same(expected, advanced.Stage);
        Assert.Same(worldEvent.EventId, advanced.LastProgressionEventId);
    }

    [Fact]
    public void ReplacesOnlyTheExactInfectionAndLeavesEveryInputUnchanged()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState target = DiseaseProgressionPolicyTests.State(
            disease,
            DiseaseStage.EarlyIllness);
        CharacterDiseaseState other = new(
            new CharacterId("healer"),
            disease,
            DiseaseStage.Exposed,
            null,
            DiseaseCareState.Unassisted);
        DiseaseDefinitionSnapshot definitions = new([disease]);
        DiseaseTruthSnapshot truth = new(definitions, [target, other]);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "first-night",
            WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([worldEvent.EventId]);
        WorldTimeSnapshot worldTime = new(worldEvent.EventId);

        DiseaseTruthSnapshot result = DiseaseProgressionTransition.Advance(
            target, truth, worldEvent, worldEvents, eventLog, worldTime);

        Assert.NotSame(truth, result);
        Assert.Collection(
            result.Infections,
            infection =>
            {
                Assert.NotSame(target, infection);
                Assert.Same(DiseaseStage.Drowsing, infection.Stage);
            },
            infection => Assert.Same(other, infection));
        Assert.Collection(
            truth.Infections,
            infection => Assert.Same(target, infection),
            infection => Assert.Same(other, infection));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Same(worldEvent.EventId, worldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsIneligibleProgressionWithoutChangingTruth()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        CharacterDiseaseState infection = DiseaseProgressionPolicyTests.State(
            disease,
            DiseaseStage.FinalDecline);
        DiseaseTruthSnapshot truth = DiseaseProgressionPolicyTests.Truth(disease, infection);
        WorldEvent worldEvent = DiseaseProgressionPolicyTests.Event(
            "first-night",
            WorldEventTimeEffect.Advance);

        Assert.Throws<InvalidOperationException>(() => DiseaseProgressionTransition.Advance(
            infection,
            truth,
            worldEvent,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]),
            new WorldTimeSnapshot(worldEvent.EventId)));
        Assert.Same(infection, Assert.Single(truth.Infections));
    }
}
