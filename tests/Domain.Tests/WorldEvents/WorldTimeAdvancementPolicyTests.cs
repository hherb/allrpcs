using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldTimeAdvancementPolicyTests
{
    [Fact]
    public void AllowsAnAdvancingEventFromTheInitialState()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);

        Assert.True(WorldTimeAdvancementPolicy.CanAdvance(worldEvent, Time()));
    }

    [Fact]
    public void RejectsAnEventWithNoTimeEffect()
    {
        WorldEvent worldEvent = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);

        Assert.False(WorldTimeAdvancementPolicy.CanAdvance(worldEvent, Time()));
    }

    [Fact]
    public void RejectsTheCurrentEventByExactOrValueEqualIdentifier()
    {
        WorldEventId retainedId = new("patient-stabilized");
        WorldEvent exact = new(retainedId, WorldEventTimeEffect.Advance);
        WorldEvent valueEqual = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldTimeSnapshot worldTime = Time(retainedId);

        Assert.False(WorldTimeAdvancementPolicy.CanAdvance(exact, worldTime));
        Assert.False(WorldTimeAdvancementPolicy.CanAdvance(valueEqual, worldTime));
    }

    [Fact]
    public void LeavesInputsUnchanged()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEventId retainedId = new("healer-arrived");
        WorldTimeSnapshot worldTime = Time(retainedId);

        bool result = WorldTimeAdvancementPolicy.CanAdvance(worldEvent, worldTime);

        Assert.True(result);
        Assert.Same(retainedId, worldTime.CurrentEventId);
        Assert.Equal("patient-stabilized", worldEvent.EventId.Value);
        Assert.Same(WorldEventTimeEffect.Advance, worldEvent.TimeEffect);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEvent worldEvent = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldTimeSnapshot worldTime = Time();

        Assert.Throws<ArgumentNullException>(
            () => WorldTimeAdvancementPolicy.CanAdvance(null!, worldTime));
        Assert.Throws<ArgumentNullException>(
            () => WorldTimeAdvancementPolicy.CanAdvance(worldEvent, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(WorldTimeAdvancementPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanAdvance", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEvent), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldTimeSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);

    private static WorldTimeSnapshot Time(WorldEventId? currentEventId = null) =>
        new(currentEventId);
}
