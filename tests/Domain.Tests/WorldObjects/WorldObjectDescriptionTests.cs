using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Locations;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectDescriptionTests
{
    [Fact]
    public void PreservesAuthoredTextExactly()
    {
        const string Text = "  One wheel is missing.\r\nFaded paint marks the side.  ";

        WorldObjectDescription description = new(Text);

        Assert.Equal(Text, description.Text);
    }

    [Fact]
    public void EmptyDescriptionIsExplicitlyValid()
    {
        WorldObjectDescription description = new(string.Empty);

        Assert.Equal(string.Empty, description.Text);
    }

    [Fact]
    public void EqualityUsesAuthoredText()
    {
        WorldObjectDescription first = new("One wheel is missing.");
        WorldObjectDescription same = new("One wheel is missing.");
        WorldObjectDescription different = new("Both wheels are missing.");

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void AcceptsTheMaximumDescriptionLength()
    {
        string text = new('d', WorldObjectDescription.MaximumLength);

        WorldObjectDescription description = new(text);

        Assert.Equal(text, description.Text);
    }

    [Fact]
    public void RejectsOversizedAndNullDescriptions()
    {
        string oversized = new('d', WorldObjectDescription.MaximumLength + 1);

        Assert.Throws<ArgumentException>(() => new WorldObjectDescription(oversized));
        Assert.Throws<ArgumentNullException>(() => new WorldObjectDescription(null!));
    }

    [Fact]
    public void ExposesOnlyAuthoredWorldObjectDescriptionState()
    {
        Type descriptionType = typeof(WorldObjectDescription);
        (string Name, Type Type, bool CanWrite)[] properties = descriptionType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(new[] { ("Text", typeof(string), false) }, properties);
        Assert.Empty(descriptionType.GetEvents());
        Assert.True(descriptionType.IsSealed);
    }

    [Fact]
    public void RemainsStructurallySeparateFromIdentifiersLocationAndCharacterText()
    {
        Type descriptionType = typeof(WorldObjectDescription);

        Assert.NotEqual(typeof(WorldObjectIdentity), descriptionType);
        Assert.NotEqual(typeof(ObjectId), descriptionType);
        Assert.NotEqual(typeof(LocationDescription), descriptionType);
        Assert.NotEqual(typeof(PublicCharacterBackground), descriptionType);
        Assert.NotEqual(typeof(PrivateCharacterBackground), descriptionType);
    }
}
