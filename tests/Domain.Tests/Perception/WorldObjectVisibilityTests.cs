using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Perception;

public sealed class WorldObjectVisibilityTests
{
    [Fact]
    public void RetainsObjectAndVisibilityStateExactly()
    {
        ObjectId objectId = new("concealed-herb");

        WorldObjectVisibility visibility = new(objectId, VisibilityState.Noticed);

        Assert.Same(objectId, visibility.ObjectId);
        Assert.Same(VisibilityState.Noticed, visibility.VisibilityState);
    }

    [Fact]
    public void EqualityUsesObjectAndVisibilityStateValues()
    {
        WorldObjectVisibility first = new(
            new ObjectId("concealed-herb"),
            VisibilityState.Identified);
        WorldObjectVisibility same = new(
            new ObjectId("concealed-herb"),
            VisibilityState.Identified);
        WorldObjectVisibility differentObject = new(
            new ObjectId("visible-herb"),
            VisibilityState.Identified);
        WorldObjectVisibility differentState = new(
            new ObjectId("concealed-herb"),
            VisibilityState.Understood);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentObject);
        Assert.NotEqual(first, differentState);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WorldObjectVisibility(null!, VisibilityState.Unnoticed));
        Assert.Throws<ArgumentNullException>(
            () => new WorldObjectVisibility(new ObjectId("concealed-herb"), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableAssociationSurface()
    {
        string[] publicProperties = typeof(WorldObjectVisibility)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(
            publicProperties,
            property => Assert.Equal("ObjectId", property),
            property => Assert.Equal("VisibilityState", property));
        Assert.All(
            typeof(WorldObjectVisibility).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.True(typeof(WorldObjectVisibility).IsSealed);
    }
}
