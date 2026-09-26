using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class SaveSnapshotLoadPolicyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void AcceptsASnapshotWithAKnownSchemaVersionWithoutChangingIt(int value)
    {
        SaveSnapshot snapshot = CreateSnapshot(new SaveSchemaVersion(value));
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
        SaveSnapshot snapshot = CreateSnapshot(new SaveSchemaVersion(27));
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

    private static SaveSnapshot CreateSnapshot(SaveSchemaVersion version)
    {
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(new CharacterId("seeker"), []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        WorldEventSaveState worldEvents = new(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));
        if (version == SaveSchemaVersion.FirstPatient
            || version == SaveSchemaVersion.DialogueDelivery
            || version == SaveSchemaVersion.Journal)
        {
            FirstPatientSaveState firstPatient = new(
                RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
                new PreparedIngredientBatchSnapshot([]),
                new IngredientExperimentJournalSnapshot([]),
                new ExperimentJournalSnapshot([]));
            return version == SaveSchemaVersion.Journal
                ? new SaveSnapshot(
                    version,
                    discovery,
                    worldEvents,
                    new CharacterMemorySnapshot([]),
                    firstPatient,
                    EmptyJournal(discovery.Perception.CharacterId))
                : new SaveSnapshot(
                    version,
                    discovery,
                    worldEvents,
                    new CharacterMemorySnapshot([]),
                    firstPatient);
        }

        return new SaveSnapshot(version, discovery, worldEvents);
    }

    private static JournalSaveState EmptyJournal(CharacterId seekerId) => new(
        seekerId,
        [],
        [],
        [],
        [],
        [],
        null);
}
