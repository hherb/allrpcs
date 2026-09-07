using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class ObservationReinterpretationCandidateSnapshotTests
{
    [Fact]
    public void EmptySnapshotIsExplicitlyValid()
    {
        ObservationReinterpretationCandidateSnapshot snapshot = new(
            Array.Empty<ObservationReinterpretationCandidate>());

        Assert.Empty(snapshot.Entries);
    }

    [Fact]
    public void PreservesSuppliedOrderAndExactInstances()
    {
        ObservationReinterpretationCandidate first = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate second = Create(
            "travelling-healer",
            "concealed-healing-herb",
            "herbal-lore");

        ObservationReinterpretationCandidateSnapshot snapshot = new(new[] { first, second });

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        ObservationReinterpretationCandidate original = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate replacement = Create(
            "travelling-healer",
            "concealed-healing-herb",
            "herbal-lore");
        ObservationReinterpretationCandidate[] source = [original];

        ObservationReinterpretationCandidateSnapshot snapshot = new(source);
        source[0] = replacement;

        Assert.Collection(snapshot.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateCandidatesByValue()
    {
        ObservationReinterpretationCandidate first = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate duplicate = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");

        Assert.NotSame(first, duplicate);
        Assert.Equal(first, duplicate);
        Assert.Throws<ArgumentException>(
            () => new ObservationReinterpretationCandidateSnapshot(new[] { first, duplicate }));
    }

    [Fact]
    public void PermitsEveryPartialMatch()
    {
        ObservationReinterpretationCandidate first = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate differentTarget = Create(
            "seeker",
            "concealed-healing-herb",
            "ancient-script-key");
        ObservationReinterpretationCandidate differentObserver = Create(
            "travelling-healer",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidate differentKnowledge = Create(
            "seeker",
            "weathered-inscription",
            "shrine-history");

        ObservationReinterpretationCandidateSnapshot snapshot = new(
            new[] { first, differentTarget, differentObserver, differentKnowledge });

        Assert.Equal(4, snapshot.Entries.Count);
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ObservationReinterpretationCandidateSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new ObservationReinterpretationCandidateSnapshot(
                new ObservationReinterpretationCandidate[] { null! }));
    }

    [Fact]
    public void LeavesCandidateInputsUnchanged()
    {
        CharacterId observerCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");
        KnowledgeId triggerKnowledgeId = new("ancient-script-key");
        ObservationReinterpretationCandidate candidate = new(
            observerCharacterId,
            targetObjectId,
            triggerKnowledgeId);

        _ = new ObservationReinterpretationCandidateSnapshot(new[] { candidate });

        Assert.Same(observerCharacterId, candidate.ObserverCharacterId);
        Assert.Same(targetObjectId, candidate.TargetObjectId);
        Assert.Same(triggerKnowledgeId, candidate.TriggerKnowledgeId);
        Assert.Equal("seeker", observerCharacterId.Value);
        Assert.Equal("weathered-inscription", targetObjectId.Value);
        Assert.Equal("ancient-script-key", triggerKnowledgeId.Value);
    }

    [Fact]
    public void ExposesOnlyTheReadOnlySnapshotSurface()
    {
        ObservationReinterpretationCandidate candidate = Create(
            "seeker",
            "weathered-inscription",
            "ancient-script-key");
        ObservationReinterpretationCandidateSnapshot snapshot = new(new[] { candidate });
        Type snapshotType = typeof(ObservationReinterpretationCandidateSnapshot);

        Assert.Throws<NotSupportedException>(
            () => ((IList<ObservationReinterpretationCandidate>)snapshot.Entries).Add(candidate));

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
                Assert.Equal(
                    typeof(IReadOnlyList<ObservationReinterpretationCandidate>),
                    property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<ObservationReinterpretationCandidate>),
                parameter.ParameterType));
        Assert.Empty(snapshotType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(snapshotType.IsSealed);
    }

    private static ObservationReinterpretationCandidate Create(
        string observerCharacterId,
        string targetObjectId,
        string triggerKnowledgeId) =>
        new(
            new CharacterId(observerCharacterId),
            new ObjectId(targetObjectId),
            new KnowledgeId(triggerKnowledgeId));
}
