using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterInventoryTests
{
    [Fact]
    public void EmptyInventoryIsExplicitlyValid()
    {
        CharacterInventory inventory = new(Array.Empty<ObjectId>());

        Assert.Empty(inventory.ObjectReferences);
    }

    [Fact]
    public void PreservesStableReferenceOrderAndTypedIdentifiers()
    {
        ObjectId first = new("healing-herb");
        ObjectId second = new("water-flask");

        CharacterInventory inventory = new(new[] { first, second });

        Assert.Equal(new[] { first, second }, inventory.ObjectReferences);
        Assert.All(inventory.ObjectReferences, reference => Assert.IsType<ObjectId>(reference));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        ObjectId original = new("healing-herb");
        ObjectId replacement = new("water-flask");
        ObjectId[] source = [original];

        CharacterInventory inventory = new(source);
        source[0] = replacement;

        Assert.Equal(new[] { original }, inventory.ObjectReferences);
    }

    [Fact]
    public void ExposesNoMutableCollection()
    {
        CharacterInventory inventory = new(new[] { new ObjectId("healing-herb") });
        IList<ObjectId> exposedReferences = Assert.IsAssignableFrom<IList<ObjectId>>(
            inventory.ObjectReferences);

        Assert.True(exposedReferences.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => exposedReferences.Add(new ObjectId("water-flask")));
    }

    [Fact]
    public void RejectsDuplicateIdentifiers()
    {
        ObjectId first = new("healing-herb");
        ObjectId duplicate = new("healing-herb");

        Assert.Throws<ArgumentException>(() => new CharacterInventory(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterInventory(null!));
        Assert.Throws<ArgumentException>(() => new CharacterInventory(new ObjectId[] { null! }));
    }

    [Fact]
    public void ExposesOnlyOrderedObjectReferences()
    {
        string[] publicProperties = typeof(CharacterInventory)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        string[] publicMethods = typeof(CharacterInventory)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(CharacterInventory) && !method.IsSpecialName)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Collection(publicProperties, property => Assert.Equal("ObjectReferences", property));
        Assert.Empty(publicMethods);
        Assert.Equal(
            typeof(IReadOnlyList<ObjectId>),
            typeof(CharacterInventory).GetProperty("ObjectReferences")!.PropertyType);
    }
}
