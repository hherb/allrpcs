using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldTimeProjectionTests
{
    [Fact]
    public void EmptyOrNoAdvanceEventsProduceTheInitialState()
    {
        WorldTimeSnapshot empty = WorldTimeProjection.FromEvents(Events());
        WorldTimeSnapshot noAdvance = WorldTimeProjection.FromEvents(
            Events(
                Event("greybox-ready", WorldEventTimeEffect.NoAdvance),
                Event("healer-arrived", WorldEventTimeEffect.NoAdvance)));

        Assert.Null(empty.CurrentEventId);
        Assert.Null(noAdvance.CurrentEventId);
    }

    [Fact]
    public void RetainsTheExactLatestAdvancingEventIdentifier()
    {
        WorldEvent firstAdvance = Event("healer-arrived", WorldEventTimeEffect.Advance);
        WorldEvent intervening = Event("herb-examined", WorldEventTimeEffect.NoAdvance);
        WorldEvent latestAdvance = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent after = Event("journal-opened", WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot events = Events(firstAdvance, intervening, latestAdvance, after);

        WorldTimeSnapshot result = WorldTimeProjection.FromEvents(events);

        Assert.Same(latestAdvance.EventId, result.CurrentEventId);
        Assert.Collection(
            events.Entries,
            entry => Assert.Same(firstAdvance, entry),
            entry => Assert.Same(intervening, entry),
            entry => Assert.Same(latestAdvance, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => WorldTimeProjection.FromEvents(null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureProjectionOperation()
    {
        Type projectionType = typeof(WorldTimeProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("FromEvents", method.Name);
        Assert.Equal(typeof(WorldTimeSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract);
        Assert.True(projectionType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);

    private static WorldEventSnapshot Events(params WorldEvent[] entries) => new(entries);
}
