using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProgressionPolicyTests
{
    [Fact]
    public void AllowsAnExactRetainedInfectionAtAnOccurredMeaningfulEvent()
    {
        DiseaseDefinition disease = Definition();
        CharacterDiseaseState infection = State(disease, DiseaseStage.Exposed);
        DiseaseTruthSnapshot truth = Truth(disease, infection);
        WorldEvent worldEvent = Event("first-night", WorldEventTimeEffect.Advance);

        Assert.True(DiseaseProgressionPolicy.CanAdvance(
            infection,
            truth,
            worldEvent,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([worldEvent.EventId]),
            new WorldTimeSnapshot(worldEvent.EventId)));
    }

    [Fact]
    public void RejectsUnretainedInfectionsAndNonAuthoritativeEvents()
    {
        DiseaseDefinition disease = Definition();
        CharacterDiseaseState retained = State(disease, DiseaseStage.Exposed);
        CharacterDiseaseState replacement = State(disease, DiseaseStage.Exposed);
        DiseaseTruthSnapshot truth = Truth(disease, retained);
        WorldEvent retainedEvent = Event("first-night", WorldEventTimeEffect.Advance);
        WorldEvent replacementEvent = Event("first-night", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([retainedEvent]);
        WorldEventLogSnapshot eventLog = new([retainedEvent.EventId]);
        WorldTimeSnapshot worldTime = new(retainedEvent.EventId);

        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            replacement, truth, retainedEvent, worldEvents, eventLog, worldTime));
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            retained, truth, replacementEvent, worldEvents, eventLog, worldTime));
    }

    [Fact]
    public void RejectsNoAdvanceUnoccurredRepeatedAndFinalStageEvents()
    {
        DiseaseDefinition disease = Definition();
        WorldEvent advance = Event("first-night", WorldEventTimeEffect.Advance);
        WorldEvent noAdvance = Event("patient-examined", WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot worldEvents = new([advance, noAdvance]);
        CharacterDiseaseState exposed = State(disease, DiseaseStage.Exposed);
        DiseaseTruthSnapshot exposedTruth = Truth(disease, exposed);

        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            exposed,
            exposedTruth,
            noAdvance,
            worldEvents,
            new WorldEventLogSnapshot([noAdvance.EventId]),
            new WorldTimeSnapshot(null)));
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            exposed,
            exposedTruth,
            advance,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(advance.EventId)));
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            exposed,
            exposedTruth,
            advance,
            worldEvents,
            new WorldEventLogSnapshot([advance.EventId]),
            new WorldTimeSnapshot(new WorldEventId("another-night"))));

        CharacterDiseaseState repeated = State(
            disease,
            DiseaseStage.EarlyIllness,
            advance.EventId);
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            repeated,
            Truth(disease, repeated),
            advance,
            worldEvents,
            new WorldEventLogSnapshot([advance.EventId]),
            new WorldTimeSnapshot(advance.EventId)));

        CharacterDiseaseState final = State(disease, DiseaseStage.FinalDecline);
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            final,
            Truth(disease, final),
            advance,
            worldEvents,
            new WorldEventLogSnapshot([advance.EventId]),
            new WorldTimeSnapshot(advance.EventId)));
    }

    [Fact]
    public void RejectsProgressionAfterStabilizationButNotComfort()
    {
        DiseaseDefinition disease = Definition();
        WorldEvent worldEvent = Event("first-night", WorldEventTimeEffect.Advance);
        CharacterDiseaseState comforted = new(
            new("patient"),
            disease,
            DiseaseStage.DeepSleep,
            null,
            DiseaseCareState.Comforted);
        CharacterDiseaseState stabilized = new(
            new("patient"),
            disease,
            DiseaseStage.DeepSleep,
            null,
            DiseaseCareState.Stabilized);
        WorldEventSnapshot events = new([worldEvent]);
        WorldEventLogSnapshot log = new([worldEvent.EventId]);
        WorldTimeSnapshot time = new(worldEvent.EventId);

        Assert.True(DiseaseProgressionPolicy.CanAdvance(
            comforted, Truth(disease, comforted), worldEvent, events, log, time));
        Assert.False(DiseaseProgressionPolicy.CanAdvance(
            stabilized, Truth(disease, stabilized), worldEvent, events, log, time));
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyOnePureOperation()
    {
        DiseaseDefinition disease = Definition();
        CharacterDiseaseState infection = State(disease, DiseaseStage.Exposed);
        DiseaseTruthSnapshot truth = Truth(disease, infection);
        WorldEvent worldEvent = Event("first-night", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([worldEvent.EventId]);
        WorldTimeSnapshot worldTime = new(worldEvent.EventId);

        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            null!, truth, worldEvent, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            infection, null!, worldEvent, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            infection, truth, null!, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            infection, truth, worldEvent, null!, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            infection, truth, worldEvent, worldEvents, null!, worldTime));
        Assert.Throws<ArgumentNullException>(() => DiseaseProgressionPolicy.CanAdvance(
            infection, truth, worldEvent, worldEvents, eventLog, null!));

        Type policyType = typeof(DiseaseProgressionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAdvance", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static DiseaseDefinition Definition() => new(new DiseaseId("grey-sleep"));

    internal static CharacterDiseaseState State(
        DiseaseDefinition disease,
        DiseaseStage stage,
        WorldEventId? lastProgressionEventId = null) =>
        new(
            new CharacterId("patient"),
            disease,
            stage,
            lastProgressionEventId,
            DiseaseCareState.Unassisted);

    internal static DiseaseTruthSnapshot Truth(
        DiseaseDefinition disease,
        CharacterDiseaseState infection) =>
        new(new DiseaseDefinitionSnapshot([disease]), [infection]);

    internal static WorldEvent Event(string id, WorldEventTimeEffect effect) =>
        new(new WorldEventId(id), effect);
}
