using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventRecordingPolicyTests
{
    [Fact]
    public void AllowsOnlyDefinedUnrecordedEvents()
    {
        WorldEvent retained = Event("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(retained);

        Assert.True(WorldEventRecordingPolicy.CanRecord(
            retained.EventId,
            worldEvents,
            Log()));
        Assert.True(WorldEventRecordingPolicy.CanRecord(
            new WorldEventId("patient-stabilized"),
            worldEvents,
            Log()));
        Assert.False(WorldEventRecordingPolicy.CanRecord(
            new WorldEventId("absent-event"),
            worldEvents,
            Log()));
        Assert.False(WorldEventRecordingPolicy.CanRecord(
            retained.EventId,
            worldEvents,
            Log(new WorldEventId("patient-stabilized"))));
    }

    [Fact]
    public void LeavesEveryInputUnchanged()
    {
        WorldEvent retained = Event("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(retained);
        WorldEventId logged = new("healer-arrived");
        WorldEventLogSnapshot eventLog = Log(logged);
        WorldEventId requested = new("patient-stabilized");

        bool result = WorldEventRecordingPolicy.CanRecord(requested, worldEvents, eventLog);

        Assert.True(result);
        Assert.Equal("patient-stabilized", requested.Value);
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(retained, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(logged, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventSnapshot worldEvents = Events(Event("patient-stabilized"));
        WorldEventLogSnapshot eventLog = Log();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingPolicy.CanRecord(null!, worldEvents, eventLog));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingPolicy.CanRecord(eventId, null!, eventLog));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventRecordingPolicy.CanRecord(eventId, worldEvents, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRecordingEligibilityOperation()
    {
        Type policyType = typeof(WorldEventRecordingPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanRecord", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static WorldEvent Event(string eventId) =>
        new(new WorldEventId(eventId), WorldEventTimeEffect.Advance);

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);
}
