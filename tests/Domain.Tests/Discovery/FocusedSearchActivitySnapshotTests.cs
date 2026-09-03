using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchActivitySnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        FocusedSearchActivitySnapshot snapshot = new(Array.Empty<FocusedSearchActivity>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesSuppliedOrderAndExactInstances()
    {
        FocusedSearchActivity first = Create("seeker", "weathered-inscription");
        FocusedSearchActivity second = Create("seeker", "concealed-healing-herb");

        FocusedSearchActivitySnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        FocusedSearchActivity original = Create("seeker", "weathered-inscription");
        FocusedSearchActivity replacement = Create("travelling-healer", "concealed-healing-herb");
        FocusedSearchActivity[] source = [original];

        FocusedSearchActivitySnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateRequestsByValue()
    {
        FocusedSearchActivity first = Create("seeker", "weathered-inscription");
        FocusedSearchActivity duplicate = Create("seeker", "weathered-inscription");

        Assert.NotSame(first.Request, duplicate.Request);
        Assert.Equal(first.Request, duplicate.Request);
        Assert.Throws<ArgumentException>(
            () => new FocusedSearchActivitySnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void PermitsOneCharacterToSearchDistinctTargetObjects()
    {
        FocusedSearchActivity first = Create("seeker", "weathered-inscription");
        FocusedSearchActivity second = Create("seeker", "concealed-healing-herb");

        FocusedSearchActivitySnapshot snapshot = new(new[] { first, second });

        Assert.Equal(2, snapshot.Entries.Count);
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new FocusedSearchActivitySnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new FocusedSearchActivitySnapshot(new FocusedSearchActivity[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        FocusedSearchActivity entry = Create("seeker", "weathered-inscription");
        FocusedSearchActivitySnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(FocusedSearchActivitySnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<FocusedSearchActivity>)snapshot.Entries).Add(entry));

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
                Assert.Equal(typeof(IReadOnlyList<FocusedSearchActivity>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<FocusedSearchActivity>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static FocusedSearchActivity Create(string searcherCharacterId, string targetObjectId) =>
        new(
            new FocusedSearchRequest(
                new CharacterId(searcherCharacterId),
                new ObjectId(targetObjectId)));
}
