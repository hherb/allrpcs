using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        FocusedSearchOutcomeSnapshot snapshot = new(Array.Empty<FocusedSearchOutcome>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesSuppliedOrderAndExactInstances()
    {
        FocusedSearchOutcome first = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome second = Create(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);

        FocusedSearchOutcomeSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void MaterializesCallerInputExactlyOnce()
    {
        FocusedSearchOutcome first = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome second = Create(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Understood);
        int enumerationCount = 0;

        IEnumerable<FocusedSearchOutcome> EnumerateOnce()
        {
            enumerationCount++;
            if (enumerationCount > 1)
            {
                throw new InvalidOperationException("The source was enumerated more than once.");
            }

            yield return first;
            yield return second;
        }

        FocusedSearchOutcomeSnapshot snapshot = new(EnumerateOnce());

        Assert.Equal(1, enumerationCount);
        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        FocusedSearchOutcome original = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome replacement = Create(
            "travelling-healer",
            "concealed-healing-herb",
            VisibilityState.Understood);
        FocusedSearchOutcome[] source = [original];

        FocusedSearchOutcomeSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateRequestsByValue()
    {
        FocusedSearchOutcome first = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome duplicate = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Understood);

        Assert.NotSame(first.Request, duplicate.Request);
        Assert.Equal(first.Request, duplicate.Request);
        Assert.NotEqual(first.ResultingVisibilityState, duplicate.ResultingVisibilityState);
        Assert.Throws<ArgumentException>(
            () => new FocusedSearchOutcomeSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void PermitsOneCharacterToHaveOutcomesForDistinctTargetObjects()
    {
        FocusedSearchOutcome first = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcome second = Create(
            "seeker",
            "concealed-healing-herb",
            VisibilityState.Identified);

        FocusedSearchOutcomeSnapshot snapshot = new(new[] { first, second });

        Assert.Equal(2, snapshot.Entries.Count);
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new FocusedSearchOutcomeSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new FocusedSearchOutcomeSnapshot(new FocusedSearchOutcome[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        FocusedSearchOutcome entry = Create(
            "seeker",
            "weathered-inscription",
            VisibilityState.Identified);
        FocusedSearchOutcomeSnapshot snapshot = new(new[] { entry });
        Type snapshotType = typeof(FocusedSearchOutcomeSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<FocusedSearchOutcome>)snapshot.Entries).Add(entry));

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
                Assert.Equal(typeof(IReadOnlyList<FocusedSearchOutcome>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<FocusedSearchOutcome>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static FocusedSearchOutcome Create(
        string searcherCharacterId,
        string targetObjectId,
        VisibilityState resultingVisibilityState) =>
        new(
            new FocusedSearchRequest(
                new CharacterId(searcherCharacterId),
                new ObjectId(targetObjectId)),
            resultingVisibilityState);
}
