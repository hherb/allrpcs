using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventOccurrenceTransitionTests
{
    [Fact]
    public void RecordsNoAdvanceEventAndPreservesExactWorldTime()
    {
        WorldEvent worldEvent = Event("greybox-ready", WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot worldEvents = Events(worldEvent);
        WorldEventId logged = new("healer-arrived");
        WorldEventLogSnapshot eventLog = Log(logged);
        WorldEventId current = new("patient-examined");
        WorldTimeSnapshot worldTime = Time(current);

        WorldEventOccurrenceResult result = WorldEventOccurrenceTransition.Occur(
            worldEvent.EventId,
            worldEvents,
            eventLog,
            worldTime);

        Assert.NotSame(eventLog, result.EventLog);
        Assert.Same(worldTime, result.WorldTime);
        Assert.Collection(
            result.EventLog.Entries,
            entry => Assert.Same(logged, entry),
            entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Same(current, result.WorldTime.CurrentEventId);
    }

    [Fact]
    public void RecordsAndAdvancesToExactAuthoritativeEvent()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = Events(worldEvent);
        WorldEventLogSnapshot eventLog = Log();
        WorldTimeSnapshot worldTime = Time();
        WorldEventId requested = new("patient-stabilized");

        WorldEventOccurrenceResult result = WorldEventOccurrenceTransition.Occur(
            requested,
            worldEvents,
            eventLog,
            worldTime);

        Assert.NotSame(eventLog, result.EventLog);
        Assert.NotSame(worldTime, result.WorldTime);
        Assert.NotSame(requested, worldEvent.EventId);
        Assert.Collection(
            result.EventLog.Entries,
            entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Same(worldEvent.EventId, result.WorldTime.CurrentEventId);
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
    }

    [Fact]
    public void RejectsIneligibleOccurrencesWithoutChangingInputs()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = Events(worldEvent);
        WorldEventId logged = new("patient-stabilized");
        WorldEventLogSnapshot eventLog = Log(logged);
        WorldTimeSnapshot worldTime = Time();

        Assert.Throws<InvalidOperationException>(
            () => WorldEventOccurrenceTransition.Occur(
                worldEvent.EventId,
                worldEvents,
                eventLog,
                worldTime));
        Assert.Throws<InvalidOperationException>(
            () => WorldEventOccurrenceTransition.Occur(
                new WorldEventId("absent-event"),
                worldEvents,
                Log(),
                worldTime));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(worldEvent, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(logged, entry));
        Assert.Null(worldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsNullInputsThroughOccurrencePolicy()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(Event(
            "patient-stabilized",
            WorldEventTimeEffect.Advance));
        WorldEventLogSnapshot eventLog = Log();
        WorldTimeSnapshot worldTime = Time();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrenceTransition.Occur(null!, worldEvents, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrenceTransition.Occur(eventId, null!, eventLog, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrenceTransition.Occur(eventId, worldEvents, null!, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventOccurrenceTransition.Occur(eventId, worldEvents, eventLog, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureOccurrenceOperation()
    {
        Type transitionType = typeof(WorldEventOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Occur", method.Name);
        Assert.Equal(typeof(WorldEventOccurrenceResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldTimeSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static WorldEvent Event(string id, WorldEventTimeEffect effect) =>
        new(new WorldEventId(id), effect);

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);

    private static WorldTimeSnapshot Time(WorldEventId? current = null) => new(current);
}
