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

public sealed class MapSaveStateJsonPersistenceTests
{
    [Fact]
    public void SerializesStableCompleteMapDocument()
    {
        string serialized = SaveSnapshotJsonSerializer.Serialize(MapVersion(PopulatedMap()));

        Assert.StartsWith("{\"schemaVersion\":6,", serialized, StringComparison.Ordinal);
        Assert.Contains(
            "\"map\":{\"seekerId\":\"seeker\","
            + "\"reportedLocationIds\":[\"stream\"],"
            + "\"verifiedLocationIds\":[\"old-shrine\"],"
            + "\"notes\":[{\"locationId\":\"old-shrine\","
            + "\"text\":\"Layered marks cross the stone.\"}]}",
            serialized,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTripsEveryMapValueAndIndependentCollections()
    {
        SaveSnapshot source = MapVersion(PopulatedMap());

        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(
            SaveSnapshotJsonSerializer.Serialize(source));

        MapSaveState map = restored.Map!;
        Assert.Same(SaveSchemaVersion.Map, restored.SchemaVersion);
        Assert.Equal("seeker", map.SeekerId.Value);
        Assert.Equal("stream", Assert.Single(map.ReportedLocationIds).Value);
        Assert.Equal("old-shrine", Assert.Single(map.VerifiedLocationIds).Value);
        MapNoteSaveEntry note = Assert.Single(map.Notes);
        Assert.Equal("old-shrine", note.LocationId.Value);
        Assert.Equal("Layered marks cross the stone.", note.Text);
        Assert.NotSame(source.Map, map);
        Assert.NotSame(source.Map!.ReportedLocationIds, map.ReportedLocationIds);
        Assert.NotSame(source.Map.VerifiedLocationIds, map.VerifiedLocationIds);
        Assert.NotSame(source.Map.Notes, map.Notes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CurrentLoadingMigratesEveryPriorVersionToFreshEmptyMap(int version)
    {
        SaveSnapshot source = PriorVersion(version);

        SaveSnapshot first = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            SaveSnapshotJsonSerializer.Serialize(source),
            InitialFirstPatient());
        SaveSnapshot second = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            SaveSnapshotJsonSerializer.Serialize(source),
            InitialFirstPatient());

        Assert.Same(SaveSchemaVersion.Map, first.SchemaVersion);
        Assert.Equal(source.Discovery.Perception.CharacterId, first.Map!.SeekerId);
        Assert.Empty(first.Map.ReportedLocationIds);
        Assert.Empty(first.Map.VerifiedLocationIds);
        Assert.Empty(first.Map.Notes);
        Assert.NotSame(first.Map, second.Map);
        Assert.NotSame(first.Map.ReportedLocationIds, second.Map!.ReportedLocationIds);
    }

    [Theory]
    [InlineData("\"map\":null")]
    [InlineData("\"map\":[]")]
    [InlineData("\"map\":{\"seekerId\":\"seeker\"}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"reportedLocationIds\":[],"
        + "\"verifiedLocationIds\":[],\"notes\":[],\"extra\":true}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"seekerId\":\"other\","
        + "\"reportedLocationIds\":[],\"verifiedLocationIds\":[],\"notes\":[]}")]
    [InlineData(
        "\"map\":{\"seekerId\":7,\"reportedLocationIds\":[],"
        + "\"verifiedLocationIds\":[],\"notes\":[]}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"Seeker\",\"reportedLocationIds\":[],"
        + "\"verifiedLocationIds\":[],\"notes\":[]}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"reportedLocationIds\":{},"
        + "\"verifiedLocationIds\":[],\"notes\":[]}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"reportedLocationIds\":[7],"
        + "\"verifiedLocationIds\":[],\"notes\":[]}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"reportedLocationIds\":[],"
        + "\"verifiedLocationIds\":[],\"notes\":[{\"locationId\":\"stream\"}]}")]
    [InlineData(
        "\"map\":{\"seekerId\":\"seeker\",\"reportedLocationIds\":[],"
        + "\"verifiedLocationIds\":[],\"notes\":[{\"locationId\":\"stream\","
        + "\"text\":7}]}")]
    public void RejectsInvalidMapStructure(string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(MapVersion(EmptyMap()));
        int start = valid.IndexOf("\"map\":", StringComparison.Ordinal);
        string malformed = valid[..start] + replacement + "}";

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void RejectsOversizedMapCollections()
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(MapVersion(EmptyMap()));
        string oversized = string.Join(',', Enumerable.Repeat("\"stream\"", 12));
        string malformed = valid.Replace(
            "\"reportedLocationIds\":[]",
            $"\"reportedLocationIds\":[{oversized}]",
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsBlankMapNoteText(string text)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(MapVersion(PopulatedMap()));
        string malformed = valid.Replace(
            "Layered marks cross the stone.",
            text,
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void RejectsOversizedMapNoteText()
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(MapVersion(PopulatedMap()));
        string malformed = valid.Replace(
            "Layered marks cross the stone.",
            new string('a', 1_001),
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    private static SaveSnapshot MapVersion(MapSaveState map)
    {
        SaveSnapshot journal = PriorVersion(5);
        return new SaveSnapshot(
            SaveSchemaVersion.Map,
            journal.Discovery,
            journal.WorldEvents,
            journal.CharacterMemory,
            journal.FirstPatient,
            journal.Journal,
            map);
    }

    private static SaveSnapshot PriorVersion(int version)
    {
        CharacterId seekerId = new("seeker");
        DiscoverySaveState discovery = new(
            new CharacterPerceptionSnapshot(seekerId, []),
            new FocusedSearchActivitySnapshot([]),
            new FocusedSearchOutcomeSnapshot([]),
            new PassiveNoticingCandidateSnapshot([]),
            new ObservationReinterpretationCandidateSnapshot([]));
        WorldEventSaveState worldEvents = new(
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null));
        CharacterMemorySnapshot memory = new([]);
        FirstPatientSaveState firstPatient = InitialFirstPatient();
        JournalSaveState journal = EmptyJournal(seekerId);
        return version switch
        {
            1 => new SaveSnapshot(SaveSchemaVersion.FirstSupported, discovery, worldEvents),
            2 => new SaveSnapshot(SaveSchemaVersion.CharacterMemory, discovery, worldEvents, memory),
            3 => new SaveSnapshot(
                SaveSchemaVersion.FirstPatient,
                discovery,
                worldEvents,
                memory,
                firstPatient),
            4 => new SaveSnapshot(
                SaveSchemaVersion.DialogueDelivery,
                discovery,
                worldEvents,
                memory,
                firstPatient),
            5 => new SaveSnapshot(
                SaveSchemaVersion.Journal,
                discovery,
                worldEvents,
                memory,
                firstPatient,
                journal),
            _ => throw new ArgumentOutOfRangeException(nameof(version)),
        };
    }

    private static FirstPatientSaveState InitialFirstPatient() => new(
        RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));

    private static JournalSaveState EmptyJournal(CharacterId seekerId) => new(
        seekerId,
        [],
        [],
        [],
        [],
        [],
        null);

    private static MapSaveState EmptyMap() => new(
        new CharacterId("seeker"),
        [],
        [],
        []);

    private static MapSaveState PopulatedMap() => new(
        new CharacterId("seeker"),
        [new LocationId("stream")],
        [new LocationId("old-shrine")],
        [
            new MapNoteSaveEntry(
                new LocationId("old-shrine"),
                "Layered marks cross the stone."),
        ]);
}
