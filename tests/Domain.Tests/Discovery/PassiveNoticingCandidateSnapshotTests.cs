using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class PassiveNoticingCandidateSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        PassiveNoticingCandidateSnapshot snapshot = new(
            Array.Empty<PassiveNoticingCandidate>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesSuppliedOrderAndExactInstances()
    {
        PassiveNoticingCandidate first = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidate second = Create("travelling-healer", "weathered-inscription");

        PassiveNoticingCandidateSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        PassiveNoticingCandidate original = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidate replacement = Create("travelling-healer", "weathered-inscription");
        PassiveNoticingCandidate[] source = [original];

        PassiveNoticingCandidateSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateCandidatesByValue()
    {
        PassiveNoticingCandidate first = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidate duplicate = Create("seeker", "concealed-healing-herb");

        Assert.NotSame(first, duplicate);
        Assert.Equal(first, duplicate);
        Assert.Throws<ArgumentException>(
            () => new PassiveNoticingCandidateSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void PermitsSameObserverWithDifferentTargets()
    {
        PassiveNoticingCandidate first = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidate second = Create("seeker", "weathered-inscription");

        PassiveNoticingCandidateSnapshot snapshot = new(new[] { first, second });

        Assert.Equal(2, snapshot.Entries.Count);
    }

    [Fact]
    public void PermitsSameTargetWithDifferentObservers()
    {
        PassiveNoticingCandidate first = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidate second = Create("travelling-healer", "concealed-healing-herb");

        PassiveNoticingCandidateSnapshot snapshot = new(new[] { first, second });

        Assert.Equal(2, snapshot.Entries.Count);
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new PassiveNoticingCandidateSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new PassiveNoticingCandidateSnapshot(
                new PassiveNoticingCandidate[] { null! }));
    }

    [Fact]
    public void LeavesCandidateInputsUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("concealed-healing-herb");
        PassiveNoticingCandidate candidate = new(observerCharacterId, targetObjectId);

        _ = new PassiveNoticingCandidateSnapshot(new[] { candidate });

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Equal("concealed-healing-herb", targetObjectId.Value);
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        PassiveNoticingCandidate candidate = Create("seeker", "concealed-healing-herb");
        PassiveNoticingCandidateSnapshot snapshot = new(new[] { candidate });
        Type snapshotType = typeof(PassiveNoticingCandidateSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<PassiveNoticingCandidate>)snapshot.Entries).Add(candidate));

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
                Assert.Equal(typeof(IReadOnlyList<PassiveNoticingCandidate>), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<PassiveNoticingCandidate>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static PassiveNoticingCandidate Create(string observerCharacterId, string targetObjectId) =>
        new(new CharacterId(observerCharacterId), new ObjectId(targetObjectId));
}
