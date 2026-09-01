using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Tests.Locations;

public sealed class LocationDescriptionTests
{
    [Fact]
    public void PreservesAuthoredTextExactly()
    {
        const string Text = "  Weathered stones surround the well.\r\nMoss marks the northern wall.  ";

        LocationDescription description = new(Text);

        Assert.Equal(Text, description.Text);
    }

    [Fact]
    public void EmptyDescriptionIsExplicitlyValid()
    {
        LocationDescription description = new(string.Empty);

        Assert.Equal(string.Empty, description.Text);
    }

    [Fact]
    public void EqualityUsesAuthoredText()
    {
        LocationDescription first = new("A shallow stream crosses the path.");
        LocationDescription same = new("A shallow stream crosses the path.");
        LocationDescription different = new("A deep stream crosses the path.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void AcceptsTheMaximumDescriptionLength()
    {
        string text = new('d', LocationDescription.MaximumLength);

        LocationDescription description = new(text);

        Assert.Equal(text, description.Text);
    }

    [Fact]
    public void RejectsOversizedAndNullDescriptions()
    {
        string oversized = new('d', LocationDescription.MaximumLength + 1);

        Assert.Throws<ArgumentException>(() => new LocationDescription(oversized));
        Assert.Throws<ArgumentNullException>(() => new LocationDescription(null!));
    }

    [Fact]
    public void ExposesOnlyAuthoredLocationDescriptionState()
    {
        Type descriptionType = typeof(LocationDescription);
        (string Name, Type Type, bool CanWrite)[] properties = descriptionType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(new[] { ("Text", typeof(string), false) }, properties);
        Assert.Empty(descriptionType.GetEvents());
        Assert.True(descriptionType.IsSealed);
    }

    [Fact]
    public void RemainsStructurallySeparateFromIdentityIdentifiersAndCharacterText()
    {
        Type descriptionType = typeof(LocationDescription);

        Assert.NotEqual(typeof(LocationIdentity), descriptionType);
        Assert.NotEqual(typeof(LocationId), descriptionType);
        Assert.NotEqual(typeof(PublicCharacterBackground), descriptionType);
        Assert.NotEqual(typeof(PrivateCharacterBackground), descriptionType);
    }
}
