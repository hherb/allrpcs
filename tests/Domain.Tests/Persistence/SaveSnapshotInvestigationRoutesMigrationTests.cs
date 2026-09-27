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

public sealed class SaveSnapshotInvestigationRoutesMigrationTests
{
    [Fact]
    public void MigratesMapVersionToFreshCanonicalEmptyRouteState()
    {
        SaveSnapshot source = MapVersion();

        SaveSnapshot first = SaveSnapshotInvestigationRoutesMigration.Migrate(source);
        SaveSnapshot second = SaveSnapshotInvestigationRoutesMigration.Migrate(source);

        Assert.Same(SaveSchemaVersion.InvestigationRoutes, first.SchemaVersion);
        Assert.Same(source.Discovery, first.Discovery);
        Assert.Same(source.WorldEvents, first.WorldEvents);
        Assert.Same(source.CharacterMemory, first.CharacterMemory);
        Assert.Same(source.FirstPatient, first.FirstPatient);
        Assert.Same(source.Journal, first.Journal);
        Assert.Same(source.Map, first.Map);
        Assert.Equal(
            ["stream-laundry", "shrine", "road-wagon"],
            first.InvestigationRoutes!.Progress.Select(entry => entry.RouteKey));
        Assert.All(first.InvestigationRoutes.Progress, entry =>
        {
            Assert.Empty(entry.RecordedLocationIds);
            Assert.Empty(entry.RecordedEvidenceIds);
        });
        Assert.Empty(first.InvestigationRoutes.AcceptedCompletionRouteKeys);
        Assert.Null(first.InvestigationRoutes.ConsumedCompletionRouteKey);
        Assert.NotSame(first.InvestigationRoutes, second.InvestigationRoutes);
        Assert.NotSame(first.InvestigationRoutes.Progress, second.InvestigationRoutes!.Progress);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNull()
    {
        SaveSnapshot current = SaveSnapshotInvestigationRoutesMigration.Migrate(MapVersion());

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotInvestigationRoutesMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotInvestigationRoutesMigration.Migrate(null!));
    }

    private static SaveSnapshot MapVersion()
    {
        CharacterId seeker = new("seeker");
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(seeker, []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        FirstPatientSaveState patient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        return new SaveSnapshot(
            SaveSchemaVersion.Map,
            discovery,
            new WorldEventSaveState(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            patient,
            new JournalSaveState(seeker, [], [], [], [], [], null),
            new MapSaveState(seeker, [], [], []));
    }
}
