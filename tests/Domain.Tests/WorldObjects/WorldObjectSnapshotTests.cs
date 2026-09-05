using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.WorldObjects;

public sealed class WorldObjectSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        WorldObjectSnapshot snapshot = new(Array.Empty<WorldObject>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesEntryOrderAndInstances()
    {
        WorldObject first = Create("healing-herb", "Healing Herb", "A narrow-leafed plant.");
        WorldObject second = Create("stone-mortar", "Stone Mortar", "A heavy stone mortar.");

        WorldObjectSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        WorldObject original = Create("healing-herb", "Healing Herb", "A narrow-leafed plant.");
        WorldObject replacement = Create("stone-mortar", "Stone Mortar", "A heavy stone mortar.");
        WorldObject[] source = [original];

        WorldObjectSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateObjectIdentifiersByValue()
    {
        WorldObject first = Create("healing-herb", "Healing Herb", "A narrow-leafed plant.");
        WorldObject duplicate = Create("healing-herb", "Unknown Herb", "An unidentified plant.");

        Assert.NotSame(first.Identity.Id, duplicate.Identity.Id);
        Assert.Equal(first.Identity.Id, duplicate.Identity.Id);
        Assert.NotEqual(first.Identity.DisplayName, duplicate.Identity.DisplayName);
        Assert.NotEqual(first.Description.Text, duplicate.Description.Text);
        Assert.Throws<ArgumentException>(() => new WorldObjectSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new WorldObjectSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new WorldObjectSnapshot(new WorldObject[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        WorldObject entry = Create("healing-herb", "Healing Herb", "A narrow-leafed plant.");
        WorldObjectSnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(WorldObjectSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<WorldObject>)snapshot.Entries).Add(entry));

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
                Assert.Equal(typeof(IReadOnlyList<WorldObject>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<WorldObject>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static WorldObject Create(string objectId, string displayName, string description) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), displayName),
            new WorldObjectDescription(description));
}
