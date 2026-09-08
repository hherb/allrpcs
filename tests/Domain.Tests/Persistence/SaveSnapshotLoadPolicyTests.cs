using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotLoadPolicyTests
{
    [Fact]
    public void AcceptsASnapshotWithTheSupportedSchemaVersionWithoutChangingIt()
    {
        SaveSnapshot snapshot = CreateSnapshot(SaveSchemaVersion.FirstSupported);
        DiscoverySaveState discovery = snapshot.Discovery;
        WorldEventSaveState worldEvents = snapshot.WorldEvents;

        bool canLoad = SaveSnapshotLoadPolicy.CanLoad(snapshot);

        Assert.True(canLoad);
        Assert.Same(discovery, snapshot.Discovery);
        Assert.Same(worldEvents, snapshot.WorldEvents);
    }

    [Fact]
    public void RejectsASnapshotWithAStructurallyValidFutureVersionWithoutChangingIt()
    {
        SaveSnapshot snapshot = CreateSnapshot(new SaveSchemaVersion(2));
        SaveSchemaVersion version = snapshot.SchemaVersion;

        bool canLoad = SaveSnapshotLoadPolicy.CanLoad(snapshot);

        Assert.False(canLoad);
        Assert.Same(version, snapshot.SchemaVersion);
    }

    [Fact]
    public void RejectsNullSnapshot()
    {
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotLoadPolicy.CanLoad(null!));
    }

    [Fact]
    public void ExposesOnlyTheStaticLoadDecision()
    {
        Type policyType = typeof(SaveSnapshotLoadPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(SaveSnapshotLoadPolicy.CanLoad), method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(SaveSnapshot), parameter.ParameterType));
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
    }

    private static SaveSnapshot CreateSnapshot(SaveSchemaVersion version) => new(
        version,
        new DiscoverySaveState(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([])),
        new WorldEventSaveState(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null)));
}
