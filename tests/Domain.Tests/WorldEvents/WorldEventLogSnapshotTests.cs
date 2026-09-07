using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventLogSnapshotTests
{
    [Fact]
    public void EmptyLogIsExplicitlyValid()
    {
        WorldEventLogSnapshot snapshot = new(Array.Empty<WorldEventId>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesSuppliedOrderAndExactInstances()
    {
        WorldEventId first = new("healer-arrived");
        WorldEventId second = new("patient-stabilized");

        WorldEventLogSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        WorldEventId original = new("healer-arrived");
        WorldEventId replacement = new("patient-stabilized");
        WorldEventId[] source = [original];

        WorldEventLogSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateIdentifiersByValue()
    {
        WorldEventId first = new("healer-arrived");
        WorldEventId duplicate = new("healer-arrived");

        Assert.NotSame(first, duplicate);
        Assert.Equal(first, duplicate);
        Assert.Throws<ArgumentException>(
            () => new WorldEventLogSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new WorldEventLogSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new WorldEventLogSnapshot(new WorldEventId[] { null! }));
    }

    [Fact]
    public void LeavesIdentifierInputsUnchanged()
    {
        WorldEventId eventId = new("healer-arrived");

        _ = new WorldEventLogSnapshot(new[] { eventId });

        Assert.Equal("healer-arrived", eventId.Value);
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        WorldEventId eventId = new("healer-arrived");
        WorldEventLogSnapshot snapshot = new(new[] { eventId });
        Type snapshotType = typeof(WorldEventLogSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<WorldEventId>)snapshot.Entries).Add(eventId));

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
                Assert.Equal(typeof(IReadOnlyList<WorldEventId>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<WorldEventId>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }
}
