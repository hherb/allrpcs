using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryCapacitySnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        InventoryCapacitySnapshot snapshot = new(Array.Empty<InventoryCapacity>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesEntryOrderAndInstances()
    {
        InventoryCapacity first = Create("travelling-healer", 12);
        InventoryCapacity second = Create("seeker", 6);

        InventoryCapacitySnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        InventoryCapacity original = Create("travelling-healer", 12);
        InventoryCapacity replacement = Create("seeker", 6);
        InventoryCapacity[] source = [original];

        InventoryCapacitySnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateCharacterIdentifiersEvenWhenMaximumsDiffer()
    {
        InventoryCapacity first = Create("travelling-healer", 12);
        InventoryCapacity duplicate = Create("travelling-healer", 6);

        Assert.Throws<ArgumentException>(
            () => new InventoryCapacitySnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new InventoryCapacitySnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new InventoryCapacitySnapshot(new InventoryCapacity[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        InventoryCapacity entry = Create("travelling-healer", 12);
        InventoryCapacitySnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(InventoryCapacitySnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<InventoryCapacity>)snapshot.Entries).Add(entry));

        (string Name, Type Type, bool CanWrite)[] actualProperties = snapshotType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = snapshotType
            .GetMethods()
            .Where(method => method.DeclaringType == snapshotType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            snapshotType.GetConstructors());

        Assert.Collection(
            actualProperties,
            property =>
            {
                Assert.Equal("Entries", property.Name);
                Assert.Equal(typeof(IReadOnlyList<InventoryCapacity>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InventoryCapacity>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static InventoryCapacity Create(string characterId, int maximumObjectCount) =>
        new(new CharacterId(characterId), maximumObjectCount);
}
