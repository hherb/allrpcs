using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;

namespace Allrpcs.Domain.Tests.Knowledge;

public sealed class KnowledgeSnapshotTests
{
    [Fact]
    public void CopiesEntriesInStableOrderAndRetainsExactInstances()
    {
        KnowledgeEntry first = Create("moonwort-use", "Moonwort must be cooled.");
        KnowledgeEntry second = Create("stream-location", "The stream runs east of the shrine.");
        KnowledgeEntry replacement = Create("replacement", "Replacement.");
        KnowledgeEntry[] source = [first, second];

        KnowledgeSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<KnowledgeEntry>)snapshot.Entries).Add(replacement));
    }

    [Fact]
    public void AcceptsEmptyAndRejectsNullsAndValueEqualDuplicateIdentifiers()
    {
        Assert.Empty(new KnowledgeSnapshot([]).Entries);
        Assert.Throws<ArgumentNullException>(() => new KnowledgeSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new KnowledgeSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new KnowledgeSnapshot(
            [Create("moonwort-use", "First."), Create("moonwort-use", "Second.")]));
    }

    [Fact]
    public void LookupReturnsExactValueEqualMatchOrNullWithoutChangingTheSnapshot()
    {
        KnowledgeEntry entry = Create("moonwort-use", "Moonwort must be cooled.");
        KnowledgeSnapshot snapshot = new([entry]);

        KnowledgeEntry? result = KnowledgeLookup.FindById(new KnowledgeId("moonwort-use"), snapshot);

        Assert.Same(entry, result);
        Assert.Null(KnowledgeLookup.FindById(new KnowledgeId("unknown"), snapshot));
        Assert.Same(entry, Assert.Single(snapshot.Entries));
        Assert.Throws<ArgumentNullException>(() => KnowledgeLookup.FindById(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() => KnowledgeLookup.FindById(entry.Id, null!));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        Type snapshotType = typeof(KnowledgeSnapshot);
        System.Reflection.PropertyInfo property = Assert.Single(snapshotType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal("Entries", property.Name);
        Assert.Equal(typeof(IReadOnlyList<KnowledgeEntry>), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(IEnumerable<KnowledgeEntry>), parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(snapshotType.GetMethods().Where(
            method => method.DeclaringType == snapshotType && !method.IsSpecialName));
        Assert.True(snapshotType.IsSealed);
    }

    private static KnowledgeEntry Create(string id, string text) =>
        new(new KnowledgeId(id), text);
}
