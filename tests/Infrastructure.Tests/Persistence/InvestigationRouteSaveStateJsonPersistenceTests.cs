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

public sealed class InvestigationRouteSaveStateJsonPersistenceTests
{
    [Fact]
    public void SerializesAndRoundTripsCompleteIdOnlyLifecycle()
    {
        SaveSnapshot source = CurrentState();

        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.StartsWith("{\"schemaVersion\":7,", serialized, StringComparison.Ordinal);
        Assert.Contains(
            "\"investigationRoutes\":{\"progress\":[{\"routeKey\":\"stream-laundry\","
            + "\"recordedLocationIds\":[\"stream\",\"abandoned-laundry\"],"
            + "\"recordedEvidenceIds\":[\"residue\"]}",
            serialized,
            StringComparison.Ordinal);
        InvestigationRouteSaveState routes = restored.InvestigationRoutes!;
        Assert.Same(SaveSchemaVersion.InvestigationRoutes, restored.SchemaVersion);
        Assert.Equal(3, routes.Progress.Count);
        Assert.Equal("stream-laundry", Assert.Single(routes.AcceptedCompletionRouteKeys));
        Assert.Equal("stream-laundry", routes.ConsumedCompletionRouteKey);
        Assert.NotSame(source.InvestigationRoutes, routes);
        Assert.NotSame(source.InvestigationRoutes!.Progress, routes.Progress);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(restored));
    }

    [Theory]
    [InlineData("\"investigationRoutes\":null")]
    [InlineData("\"investigationRoutes\":[]")]
    [InlineData("\"investigationRoutes\":{\"progress\":[]}")]
    [InlineData(
        "\"investigationRoutes\":{\"progress\":[],\"acceptedCompletionRouteKeys\":[],"
        + "\"consumedCompletionRouteKey\":null,\"extra\":true}")]
    public void RejectsMalformedClosedRouteStructure(string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(CurrentState());
        int start = valid.IndexOf("\"investigationRoutes\":", StringComparison.Ordinal);
        string malformed = valid[..start] + replacement + "}";

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void RejectsOversizedRouteCollections()
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(CurrentState());
        string entry = "{\"routeKey\":\"stream-laundry\",\"recordedLocationIds\":[],"
            + "\"recordedEvidenceIds\":[]}";
        string malformed = valid.Replace(
            "\"progress\":[" + valid.Split("\"progress\":[", StringSplitOptions.None)[1]
                .Split("],\"acceptedCompletionRouteKeys\"", StringSplitOptions.None)[0] + "]",
            $"\"progress\":[{entry},{entry},{entry},{entry}]",
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    private static SaveSnapshot CurrentState()
    {
        CharacterId seekerId = new("seeker");
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(seekerId, []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        FirstPatientSaveState firstPatient = new(
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        InvestigationRouteSaveState routes = new(
            [
                new(
                    "stream-laundry",
                    [new LocationId("stream"), new LocationId("abandoned-laundry")],
                    [new EvidenceId("residue")]),
                new("shrine", [new LocationId("old-shrine")], []),
                new("road-wagon", [], []),
            ],
            ["stream-laundry"],
            "stream-laundry");
        return new SaveSnapshot(
            SaveSchemaVersion.InvestigationRoutes,
            discovery,
            new WorldEventSaveState(new WorldEventLogSnapshot([]), new WorldTimeSnapshot(null)),
            new CharacterMemorySnapshot([]),
            firstPatient,
            new JournalSaveState(seekerId, [], [], [], [], [], null),
            new MapSaveState(seekerId, [], [], []),
            routes);
    }
}
