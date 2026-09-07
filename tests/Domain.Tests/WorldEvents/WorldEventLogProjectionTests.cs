using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLogProjectionTests
{
    [Fact]
    public void EmptyLogProducesAnEmptyEventSnapshot()
    {
        WorldEventSnapshot result = WorldEventLogProjection.Resolve(Log(), Events());

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void ResolvesExactEventsInLogOrderAndIgnoresUnrecordedDefinitions()
    {
        WorldEvent firstDefinition = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEvent secondDefinition = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent unrecorded = Event("herb-collected", WorldEventTimeEffect.Advance);
        WorldEventSnapshot events = Events(secondDefinition, unrecorded, firstDefinition);
        WorldEventLogSnapshot log = Log(
            new WorldEventId("healer-arrived"),
            new WorldEventId("patient-stabilized"));

        WorldEventSnapshot result = WorldEventLogProjection.Resolve(log, events);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(firstDefinition, entry),
            entry => Assert.Same(secondDefinition, entry));
        Assert.Collection(
            events.Entries,
            entry => Assert.Same(secondDefinition, entry),
            entry => Assert.Same(unrecorded, entry),
            entry => Assert.Same(firstDefinition, entry));
    }

    [Fact]
    public void RejectsALogIdentifierWithoutAnAuthoritativeEvent()
    {
        WorldEvent retained = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEventLogSnapshot log = Log(
            retained.EventId,
            new WorldEventId("patient-stabilized"));
        WorldEventSnapshot events = Events(retained);

        Assert.Throws<InvalidOperationException>(
            () => WorldEventLogProjection.Resolve(log, events));
        Assert.Collection(
            log.Entries,
            entry => Assert.Same(retained.EventId, entry),
            entry => Assert.Equal("patient-stabilized", entry.Value));
        Assert.Collection(events.Entries, entry => Assert.Same(retained, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventLogSnapshot log = Log();
        WorldEventSnapshot events = Events();

        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogProjection.Resolve(null!, events));
        Assert.Throws<ArgumentNullException>(
            () => WorldEventLogProjection.Resolve(log, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureResolutionOperation()
    {
        Type projectionType = typeof(WorldEventLogProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("Resolve", method.Name);
        Assert.Equal(typeof(WorldEventSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);

    private static WorldEventLogSnapshot Log(params WorldEventId[] entries) => new(entries);

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);
}
