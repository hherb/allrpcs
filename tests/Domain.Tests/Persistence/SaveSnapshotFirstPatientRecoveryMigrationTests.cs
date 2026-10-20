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

public sealed class SaveSnapshotFirstPatientRecoveryMigrationTests
{
    [Fact]
    public void MigratesRouteVersionToExactInertRecoveryProgress()
    {
        SaveSnapshot source = RouteVersion();

        SaveSnapshot result = SaveSnapshotFirstPatientRecoveryMigration.Migrate(source);

        Assert.Same(SaveSchemaVersion.FirstPatientRecovery, result.SchemaVersion);
        Assert.Same(source.Discovery, result.Discovery);
        Assert.Same(source.WorldEvents, result.WorldEvents);
        Assert.Same(source.CharacterMemory, result.CharacterMemory);
        Assert.Same(source.FirstPatient, result.FirstPatient);
        Assert.Same(source.Journal, result.Journal);
        Assert.Same(source.Map, result.Map);
        Assert.Same(source.InvestigationRoutes, result.InvestigationRoutes);
        Assert.Same(FirstPatientRecoveryStage.NotHeard, result.FirstPatientRecovery);
    }

    [Fact]
    public void RejectsEveryOtherVersionAndNull()
    {
        SaveSnapshot current = SaveSnapshotFirstPatientRecoveryMigration.Migrate(RouteVersion());

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotFirstPatientRecoveryMigration.Migrate(current));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotFirstPatientRecoveryMigration.Migrate(null!));
    }

    internal static SaveSnapshot RouteVersion()
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
            SaveSchemaVersion.InvestigationRoutes,
            discovery,
            new WorldEventSaveState(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            patient,
            new JournalSaveState(seeker, [], [], [], [], [], null),
            new MapSaveState(seeker, [], [], []),
            new InvestigationRouteSaveState(
                [
                    new InvestigationRouteProgressSaveEntry("stream-laundry", [], []),
                    new InvestigationRouteProgressSaveEntry("shrine", [], []),
                    new InvestigationRouteProgressSaveEntry("road-wagon", [], []),
                ],
                [],
                null));
    }
}
