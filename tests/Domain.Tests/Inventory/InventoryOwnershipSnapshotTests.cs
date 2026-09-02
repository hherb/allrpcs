using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryOwnershipSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        InventoryOwnershipSnapshot snapshot = new(Array.Empty<InventoryOwnership>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesEntryOrderAndInstances()
    {
        InventoryOwnership first = Create("stone-mortar", "travelling-healer");
        InventoryOwnership second = Create("healing-herb", "seeker");

        InventoryOwnershipSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        InventoryOwnership original = Create("stone-mortar", "travelling-healer");
        InventoryOwnership replacement = Create("healing-herb", "seeker");
        InventoryOwnership[] source = [original];

        InventoryOwnershipSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateObjectIdentifiersEvenWhenOwnersDiffer()
    {
        InventoryOwnership first = Create("stone-mortar", "travelling-healer");
        InventoryOwnership duplicate = Create("stone-mortar", "seeker");

        Assert.Throws<ArgumentException>(
            () => new InventoryOwnershipSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new InventoryOwnershipSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new InventoryOwnershipSnapshot(new InventoryOwnership[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        InventoryOwnership entry = Create("stone-mortar", "travelling-healer");
        InventoryOwnershipSnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(InventoryOwnershipSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<InventoryOwnership>)snapshot.Entries).Add(entry));

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
                Assert.Equal(typeof(IReadOnlyList<InventoryOwnership>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<InventoryOwnership>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static InventoryOwnership Create(string objectId, string ownerCharacterId) =>
        new(new ObjectId(objectId), new CharacterId(ownerCharacterId));
}
