using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventTests
{
    [Fact]
    public void RetainsExactIdentifierAndTimeEffectInstances()
    {
        WorldEventId eventId = new("patient-stabilized");
        WorldEventTimeEffect timeEffect = WorldEventTimeEffect.Advance;

        WorldEvent worldEvent = new(eventId, timeEffect);

        Assert.Same(eventId, worldEvent.EventId);
        Assert.Same(timeEffect, worldEvent.TimeEffect);
    }

    [Fact]
    public void EqualityUsesIdentifierAndTimeEffectValues()
    {
        WorldEvent first = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        WorldEvent same = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        WorldEvent differentIdentifier = new(
            new WorldEventId("healer-arrived"),
            WorldEventTimeEffect.Advance);
        WorldEvent differentTimeEffect = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.NoAdvance);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentIdentifier);
        Assert.NotEqual(first, differentTimeEffect);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WorldEvent(null!, WorldEventTimeEffect.NoAdvance));
        Assert.Throws<ArgumentNullException>(
            () => new WorldEvent(new WorldEventId("healer-arrived"), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableWorldEventSurface()
    {
        Type worldEventType = typeof(WorldEvent);
        (string Name, Type Type)[] expectedProperties =
        [
            ("EventId", typeof(WorldEventId)),
            ("TimeEffect", typeof(WorldEventTimeEffect)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = worldEventType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            worldEventType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(WorldEventId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventTimeEffect), parameter.ParameterType));
        Assert.Empty(worldEventType.GetEvents());
        Assert.True(worldEventType.IsSealed);
    }
}
