using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnWorldEventOccurrencePolicyTests
{
    [Fact]
    public void AllowsAnExactAuthoritativeEligibleOccurrence()
    {
        WorldEvent worldEvent = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(worldEvent);

        bool canOccur = CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            occurrence,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));

        Assert.True(canOccur);
    }

    [Fact]
    public void RejectsADistinctValueEqualWorldEventDefinition()
    {
        WorldEvent authoritative = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        WorldEvent distinctEqual = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);

        Assert.Equal(authoritative, distinctEqual);
        Assert.NotSame(authoritative, distinctEqual);
        Assert.False(CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            Occurrence(distinctEqual),
            new WorldEventSnapshot([authoritative]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null)));
    }

    [Fact]
    public void RejectsAnAbsentRecordedOrTimeIneligibleOccurrence()
    {
        WorldEvent noAdvance = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        WorldEvent advance = Event("healer-examined-moonwort", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([noAdvance, advance]);

        Assert.False(CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            Occurrence(Event("absent", WorldEventTimeEffect.NoAdvance)),
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null)));
        Assert.False(CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            Occurrence(noAdvance),
            worldEvents,
            new WorldEventLogSnapshot([noAdvance.EventId]),
            new WorldTimeSnapshot(null)));
        Assert.False(CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            Occurrence(advance),
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(advance.EventId)));
    }

    [Fact]
    public void LeavesEveryInputUnchanged()
    {
        WorldEvent worldEvent = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventId retainedId = new("seeker-spoke");
        WorldEventLogSnapshot eventLog = new([retainedId]);
        WorldEventId currentTimeId = new("journey-began");
        WorldTimeSnapshot worldTime = new(currentTimeId);

        Assert.True(CharacterTurnWorldEventOccurrencePolicy.CanOccur(
            occurrence,
            worldEvents,
            eventLog,
            worldTime));
        Assert.Same(worldEvent, occurrence.WorldEvent);
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(retainedId, entry));
        Assert.Same(currentTimeId, worldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEvent worldEvent = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                null!, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                occurrence, null!, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                occurrence, worldEvents, null!, worldTime));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrencePolicy.CanOccur(
                occurrence, worldEvents, eventLog, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(CharacterTurnWorldEventOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterTurnWorldEventOccurrence),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldTimeSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static WorldEvent Event(string id, WorldEventTimeEffect effect) =>
        new(new WorldEventId(id), effect);

    private static CharacterTurnWorldEventOccurrence Occurrence(WorldEvent worldEvent) =>
        new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                new DialogueDelivery(
                    new DialogueUtterance(new CharacterId("healer"), "I will examine it."),
                    DialogueDeliveryMode.OrdinarySpeech)));
}
