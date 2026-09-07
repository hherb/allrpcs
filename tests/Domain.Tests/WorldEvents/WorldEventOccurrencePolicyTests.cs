using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventOccurrencePolicyTests
{
    [Fact]
    public void AllowsDefinedUnrecordedEventsWithEligibleTimeEffects()
    {
        WorldEvent noAdvance = Event("greybox-ready", WorldEventTimeEffect.NoAdvance);
        WorldEvent advance = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = Events(noAdvance, advance);

        Assert.True(WorldEventOccurrencePolicy.CanOccur(
            noAdvance.EventId,
            worldEvents,
            Log(),
            Time()));
        Assert.True(WorldEventOccurrencePolicy.CanOccur(
            new WorldEventId("patient-stabilized"),
            worldEvents,
            Log(),
            Time(new WorldEventId("patient-examined"))));
    }

    [Fact]
    public void RejectsAbsentRecordedAndRepeatedAdvancingEvents()
    {
        WorldEvent advance = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = Events(advance);

        Assert.False(WorldEventOccurrencePolicy.CanOccur(
            new WorldEventId("absent-event"),
            worldEvents,
            Log(),
            Time()));
        Assert.False(WorldEventOccurrencePolicy.CanOccur(
            advance.EventId,
            worldEvents,
            Log(new WorldEventId("patient-stabilized")),
            Time()));
        Assert.False(WorldEventOccurrencePolicy.CanOccur(
            advance.EventId,
            worldEvents,
            Log(),
            Time(new WorldEventId("patient-stabilized"))));
    }

    [Fact]
    public void LeavesEveryInputUnchanged()
    {
        WorldEvent advance = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = Events(advance);
        WorldEventId logged = new("healer-arrived");
        WorldEventLogSnapshot eventLog = Log(logged);
        WorldEventId current = new("patient-examined");
        WorldTimeSnapshot worldTime = Time(current);
        WorldEventId requested = new("patient-stabilized");

        Assert.True(WorldEventOccurrencePolicy.CanOccur(
            requested,
            worldEvents,
            eventLog,
            worldTime));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(advance, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(logged, entry));
        Assert.Same(current, worldTime.CurrentEventId);
        Assert.Equal("patient-stabilized", requested.Value);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(Event(
            "patient-stabilized",
            WorldEventTimeEffect.Advance));
        WorldEventLogSnapshot eventLog = Log();
        WorldTimeSnapshot worldTime = Time();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrencePolicy.CanOccur(null!, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrencePolicy.CanOccur(eventId, null!, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrencePolicy.CanOccur(eventId, worldEvents, null!, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrencePolicy.CanOccur(eventId, worldEvents, eventLog, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureOccurrenceEligibilityOperation()
    {
        Type policyType = typeof(WorldEventOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
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

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);

    private static WorldTimeSnapshot Time(WorldEventId? current = null) => new(current);
}
