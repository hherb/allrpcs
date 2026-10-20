using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class FirstPatientRecoverySaveStateJsonPersistenceTests
{
    [Theory]
    [InlineData("not-heard")]
    [InlineData("patient-testimony-heard")]
    [InlineData("sage-consulted")]
    public void RoundTripsEachClosedRecoveryStage(string stageValue)
    {
        FirstPatientRecoveryStage stage = FirstPatientRecoveryStage.Parse(stageValue);
        SaveSnapshot source = CurrentState(stage);

        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.StartsWith("{\"schemaVersion\":8,", serialized, StringComparison.Ordinal);
        Assert.EndsWith(
            $"\"firstPatientRecoveryStage\":\"{stageValue}\"}}",
            serialized,
            StringComparison.Ordinal);
        Assert.Same(SaveSchemaVersion.FirstPatientRecovery, restored.SchemaVersion);
        Assert.Same(stage, restored.FirstPatientRecovery);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(restored));
    }

    [Theory]
    [InlineData("\"firstPatientRecoveryStage\":null")]
    [InlineData("\"firstPatientRecoveryStage\":true")]
    [InlineData("\"firstPatientRecoveryStage\":\"heard\"")]
    public void RejectsMalformedOrUnknownRecoveryStage(string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(
            CurrentState(FirstPatientRecoveryStage.NotHeard));
        string malformed = valid.Replace(
            "\"firstPatientRecoveryStage\":\"not-heard\"",
            replacement,
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    internal static SaveSnapshot CurrentState(FirstPatientRecoveryStage stage)
    {
        CharacterId seeker = new("seeker");
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(seeker, []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        FirstPatientSaveState firstPatient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        return new SaveSnapshot(
            SaveSchemaVersion.FirstPatientRecovery,
            discovery,
            new WorldEventSaveState(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            firstPatient,
            new JournalSaveState(seeker, [], [], [], [], [], null),
            new MapSaveState(seeker, [], [], []),
            new InvestigationRouteSaveState(
                [
                    new InvestigationRouteProgressSaveEntry("stream-laundry", [], []),
                    new InvestigationRouteProgressSaveEntry("shrine", [], []),
                    new InvestigationRouteProgressSaveEntry("road-wagon", [], []),
                ],
                [],
                null),
            stage);
    }
}
