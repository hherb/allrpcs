using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventRecordingTransitionTests
{
    [Fact]
    public void AppendsTheExactAuthoritativeIdentifierAfterStableLogEntries()
    {
        WorldEvent retained = Event("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(retained);
        WorldEventId first = new("healer-arrived");
        WorldEventLogSnapshot eventLog = Log(first);
        WorldEventId requested = new("patient-stabilized");

        WorldEventLogSnapshot result = WorldEventRecordingTransition.Record(
            requested,
            worldEvents,
            eventLog);

        Assert.NotSame(eventLog, result);
        Assert.NotSame(requested, retained.EventId);
        Assert.Collection(eventLog.Entries, entry => Assert.Same(first, entry));
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(retained.EventId, entry));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsAbsentAndAlreadyRecordedEventsWithoutMutation()
    {
        WorldEvent retained = Event("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(retained);
        WorldEventId logged = new("patient-stabilized");
        WorldEventLogSnapshot eventLog = Log(logged);

        Assert.Throws<InvalidOperationException>(
            () => WorldEventRecordingTransition.Record(
                new WorldEventId("absent-event"),
                worldEvents,
                eventLog));
        Assert.Throws<InvalidOperationException>(
            () => WorldEventRecordingTransition.Record(
                retained.EventId,
                worldEvents,
                eventLog));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(retained, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(logged, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughRecordingPolicy()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(Event("patient-stabilized"));
        WorldEventLogSnapshot eventLog = Log();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingTransition.Record(null!, worldEvents, eventLog));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingTransition.Record(eventId, null!, eventLog));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingTransition.Record(eventId, worldEvents, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingOperation()
    {
        Type transitionType = typeof(WorldEventRecordingTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Record", method.Name);
        Assert.Equal(typeof(WorldEventLogSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static WorldEvent Event(string eventId) =>
        new(new WorldEventId(eventId), WorldEventTimeEffect.Advance);

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);
}
