using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLookupTests
{
    [Fact]
    public void ReturnsNullForEmptyOrUnrelatedSnapshots()
    {
        WorldEventId eventId = new("patient-stabilized");

        Assert.Null(WorldEventLookup.FindById(eventId, Snapshot()));
        Assert.Null(WorldEventLookup.FindById(
            eventId,
            Snapshot(Event("healer-arrived", WorldEventTimeEffect.NoAdvance))));
    }

    [Fact]
    public void ReturnsExactEventForValueEqualDistinctIdentifier()
    {
        WorldEventId retainedId = new("patient-stabilized");
        WorldEvent retained = new(retainedId, WorldEventTimeEffect.Advance);
        WorldEventId requestedId = new("patient-stabilized");

        WorldEvent? result = WorldEventLookup.FindById(requestedId, Snapshot(retained));

        Assert.NotSame(requestedId, retainedId);
        Assert.Equal(requestedId, retainedId);
        Assert.NotNull(result);
        Assert.Same(retained, result);
        Assert.Same(retainedId, result.EventId);
        Assert.Same(WorldEventTimeEffect.Advance, result.TimeEffect);
    }

    [Fact]
    public void ReturnsExactMatchAmongSurroundingEntriesWithoutChangingInputs()
    {
        WorldEvent before = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEvent matching = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent after = Event("herb-collected", WorldEventTimeEffect.Advance);
        WorldEventSnapshot snapshot = Snapshot(before, matching, after);

        WorldEvent? result = WorldEventLookup.FindById(matching.EventId, snapshot);

        Assert.Same(matching, result);
        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(before, entry),
            entry => Assert.Same(matching, entry),
            entry => Assert.Same(after, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventSnapshot snapshot = Snapshot();

        Assert.Throws<ArgumentNullException>(() => WorldEventLookup.FindById(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() => WorldEventLookup.FindById(eventId, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureLookupOperation()
    {
        Type lookupType = typeof(WorldEventLookup);
        System.Reflection.MethodInfo method = Assert.Single(
            lookupType.GetMethods().Where(candidate => candidate.DeclaringType == lookupType));

        Assert.Equal("FindById", method.Name);
        Assert.Equal(typeof(WorldEvent), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType));
        Assert.Empty(lookupType.GetConstructors());
        Assert.Empty(lookupType.GetProperties());
        Assert.Empty(lookupType.GetEvents());
        Assert.True(lookupType.IsAbstract);
        Assert.True(lookupType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);

    private static WorldEventSnapshot Snapshot(params WorldEvent[] entries) => new(entries);
}
