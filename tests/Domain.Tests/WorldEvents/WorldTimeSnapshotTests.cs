using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldTimeSnapshotTests
{
    [Fact]
    public void NoCurrentEventIsAnExplicitInitialState()
    {
        WorldTimeSnapshot snapshot = new(null);

        Assert.Null(snapshot.CurrentEventId);
    }

    [Fact]
    public void RetainsTheExactCurrentEventIdentifier()
    {
        WorldEventId eventId = new("patient-stabilized");

        WorldTimeSnapshot snapshot = new(eventId);

        Assert.Same(eventId, snapshot.CurrentEventId);
    }

    [Fact]
    public void EqualityUsesTheCurrentEventIdentifierValue()
    {
        WorldTimeSnapshot first = new(new WorldEventId("patient-stabilized"));
        WorldTimeSnapshot same = new(new WorldEventId("patient-stabilized"));
        WorldTimeSnapshot different = new(new WorldEventId("healer-arrived"));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, different);
        Assert.NotEqual(first, new WorldTimeSnapshot(null));
    }

    [Fact]
    public void ExposesOnlyTheImmutableWorldTimeSurface()
    {
        Type snapshotType = typeof(WorldTimeSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            snapshotType.GetConstructors());

        Assert.Equal("CurrentEventId", property.Name);
        Assert.Equal(typeof(WorldEventId), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.True(snapshotType.IsSealed);
    }
}
