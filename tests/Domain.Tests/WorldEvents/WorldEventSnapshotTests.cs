using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.WorldEvents;

public sealed class WorldEventSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        WorldEventSnapshot snapshot = new(Array.Empty<WorldEvent>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesEntryOrderAndExactInstances()
    {
        WorldEvent first = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEvent second = Event("patient-stabilized", WorldEventTimeEffect.Advance);

        WorldEventSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        WorldEvent original = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEvent replacement = Event("patient-stabilized", WorldEventTimeEffect.Advance);
        WorldEvent[] source = [original];

        WorldEventSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateEventIdentifiersByValue()
    {
        WorldEvent first = Event("patient-stabilized", WorldEventTimeEffect.NoAdvance);
        WorldEvent duplicate = Event("patient-stabilized", WorldEventTimeEffect.Advance);

        Assert.NotSame(first.EventId, duplicate.EventId);
        Assert.Equal(first.EventId, duplicate.EventId);
        Assert.NotEqual(first.TimeEffect, duplicate.TimeEffect);
        Assert.Throws<ArgumentException>(() => new WorldEventSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new WorldEventSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new WorldEventSnapshot(new WorldEvent[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        WorldEvent entry = Event("healer-arrived", WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(WorldEventSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<WorldEvent>)snapshot.Entries).Add(entry));

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
                Assert.Equal(typeof(IReadOnlyList<WorldEvent>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<WorldEvent>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static WorldEvent Event(string eventId, WorldEventTimeEffect timeEffect) =>
        new(new WorldEventId(eventId), timeEffect);
}
