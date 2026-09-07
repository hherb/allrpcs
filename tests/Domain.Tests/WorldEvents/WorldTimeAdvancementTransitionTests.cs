using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldTimeAdvancementTransitionTests
{
    [Fact]
    public void AdvancesToTheExactEventIdentifier()
    {
        WorldEventId previousId = new("healer-arrived");
        WorldTimeSnapshot original = new(previousId);
        WorldEventId advancingId = new("patient-stabilized");
        WorldEvent worldEvent = new(advancingId, WorldEventTimeEffect.Advance);

        WorldTimeSnapshot result = WorldTimeAdvancementTransition.Advance(worldEvent, original);

        Assert.NotSame(original, result);
        Assert.Same(advancingId, result.CurrentEventId);
        Assert.Same(previousId, original.CurrentEventId);
    }

    [Fact]
    public void AdvancesFromTheInitialState()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);

        WorldTimeSnapshot result = WorldTimeAdvancementTransition.Advance(
            worldEvent,
            new WorldTimeSnapshot(null));

        Assert.Same(worldEvent.EventId, result.CurrentEventId);
    }

    [Fact]
    public void RejectsIneligibleEventsWithoutChangingInputs()
    {
        WorldEventId retainedId = new("patient-stabilized");
        WorldTimeSnapshot worldTime = new(retainedId);
        WorldEvent noAdvance = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEvent duplicate = Event("patient-stabilized", WorldEventTimeEffect.Advance);

        Assert.Throws<InvalidOperationException>(
            () => WorldTimeAdvancementTransition.Advance(noAdvance, worldTime));
        Assert.Throws<InvalidOperationException>(
            () => WorldTimeAdvancementTransition.Advance(duplicate, worldTime));
        Assert.Same(retainedId, worldTime.CurrentEventId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldTimeSnapshot worldTime = new(null);

        Assert.Throws<ArgumentNullException>(
            () => WorldTimeAdvancementTransition.Advance(null!, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldTimeAdvancementTransition.Advance(worldEvent, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureTransition()
    {
        Type transitionType = typeof(WorldTimeAdvancementTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Advance", method.Name);
        Assert.Equal(typeof(WorldTimeSnapshot), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEvent), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldTimeSnapshot), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);
}
