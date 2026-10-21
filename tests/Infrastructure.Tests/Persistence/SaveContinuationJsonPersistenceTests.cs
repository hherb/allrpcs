using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class SaveContinuationJsonPersistenceTests
{
    private static readonly SaveContinuationState Empty = new(null, []);

    [Fact]
    public void RoundTripsTheSeekerProfileAndExactPositions()
    {
        SaveSnapshot source = Continuation(new SaveContinuationState(
            Profile(),
            [new(new CharacterId("seeker"), 2120.5f, -950.25f), new(new CharacterId("healer"), 0.1f, 1e-7f)]));

        string serialized = SaveSnapshotJsonSerializer.Serialize(source);
        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.StartsWith("{\"schemaVersion\":9,", serialized, StringComparison.Ordinal);
        Assert.Same(SaveSchemaVersion.Continuation, restored.SchemaVersion);
        Assert.Equal(Profile(), restored.Continuation!.SeekerProfile);
        Assert.Equal(source.Continuation!.Positions, restored.Continuation.Positions);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(restored));
    }

    [Fact]
    public void WritesAMissingContinuationAsNoProfileAndNoPositions()
    {
        string serialized = SaveSnapshotJsonSerializer.Serialize(Continuation(null));

        SaveSnapshot restored = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.EndsWith(
            "\"continuation\":{\"seekerProfile\":null,\"characterPositions\":[]}}",
            serialized,
            StringComparison.Ordinal);
        Assert.Null(restored.Continuation!.SeekerProfile);
        Assert.Empty(restored.Continuation.Positions);
    }

    [Fact]
    public void CurrentLoadingMigratesARecoveryVersionToAnEmptyContinuation()
    {
        string versionEight = SaveSnapshotJsonSerializer.Serialize(
            FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(FirstPatientRecoveryStage.SageConsulted));

        SaveSnapshot current = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            versionEight,
            FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(FirstPatientRecoveryStage.NotHeard).FirstPatient!);

        Assert.Same(SaveSchemaVersion.Continuation, current.SchemaVersion);
        Assert.Same(FirstPatientRecoveryStage.SageConsulted, current.FirstPatientRecovery);
        Assert.Null(current.Continuation!.SeekerProfile);
        Assert.Empty(current.Continuation.Positions);
    }

    [Theory]
    [InlineData("\"characterPositions\":[]", "\"characterPositions\":[],\"extra\":1")]
    [InlineData("\"seekerProfile\":null", "\"seekerProfile\":\"seeker\"")]
    [InlineData("\"characterPositions\":[]", "\"characterPositions\":[{\"characterId\":\"seeker\",\"x\":\"1\",\"y\":2}]")]
    [InlineData("\"characterPositions\":[]", "\"characterPositions\":[{\"characterId\":\"seeker\",\"x\":1}]")]
    [InlineData("\"characterPositions\":[]", "\"characterPositions\":[{\"characterId\":\"seeker\",\"x\":1,\"y\":2},{\"characterId\":\"seeker\",\"x\":1,\"y\":2}]")]
    [InlineData(",\"continuation\":{\"seekerProfile\":null,\"characterPositions\":[]}", "")]
    public void RejectsMalformedContinuation(string original, string replacement)
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(Continuation(Empty));
        string malformed = valid.Replace(original, replacement, StringComparison.Ordinal);

        Assert.NotEqual(valid, malformed);
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void RejectsAProfileWithANonCanonicalOptionKey()
    {
        string valid = SaveSnapshotJsonSerializer.Serialize(Continuation(new SaveContinuationState(Profile(), [])));
        string malformed = valid.Replace("\"backgroundKey\":\"forester\"", "\"backgroundKey\":\"Forester!\"", StringComparison.Ordinal);

        Assert.NotEqual(valid, malformed);
        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonDeserializer.Deserialize(malformed));
    }

    [Fact]
    public void RecordsFurtherMemoryAfterAReloadAndPrunesIndexEntriesOfEvictedEpisodes()
    {
        CharacterId healer = new("healer");
        CharacterMemorySnapshot Observe(CharacterMemorySnapshot memory, int count)
        {
            for (int index = 0; index < count; index++)
            {
                memory = CharacterMemoryRecordingTransition.RecordObservation(
                    memory, healer, [], new LocationId("village-square"), new WorldTimeSnapshot(null), $"observation-{index}");
            }

            return memory;
        }

        CharacterMemorySnapshot Reload(CharacterMemorySnapshot memory) => SaveSnapshotJsonDeserializer.Deserialize(
            SaveSnapshotJsonSerializer.Serialize(Continuation(Empty, memory))).CharacterMemory;

        // The legacy fixture's ids (found-moonwort, turn-one) do not follow the recorded pattern.
        CharacterMemorySnapshot memory = Observe(
            Reload(Observe(SaveSnapshotJsonSerializerTests.CreateCompleteCharacterMemory(), 2)),
            1);
        CharacterMemoryAggregate continued = Assert.Single(memory.Characters);
        Assert.Equal("healer-episode-3", continued.EpisodicMemories.Memories[^1].MemoryId.Value);
        Assert.Equal("healer-turn-3", continued.EpisodicMemories.Memories[^1].SourceTurnId.Value);
        int indexCount = continued.MemoryIndex.Entries.Count;

        memory = Reload(Observe(memory, CharacterEpisodicMemorySnapshot.MaximumMemoryCount - 3));

        CharacterMemoryAggregate evicted = Assert.Single(memory.Characters);
        Assert.Equal("healer-episode-1", evicted.EpisodicMemories.Memories[0].MemoryId.Value);
        Assert.Equal(indexCount - 1, evicted.MemoryIndex.Entries.Count);
        Assert.All(evicted.MemoryIndex.Entries, entry => Assert.Null(entry.MemoryReference.EpisodicMemory));
    }

    private static SeekerCustomizationSaveState Profile() => new(
        new CharacterIdentity(new CharacterId("seeker"), "Mara"),
        new SeekerPronouns("she", "her", "her", "hers", "herself"),
        "weathered-traveller",
        "forester");

    private static SaveSnapshot Continuation(SaveContinuationState? continuation, CharacterMemorySnapshot? memory = null)
    {
        SaveSnapshot recovery = FirstPatientRecoverySaveStateJsonPersistenceTests.RecoveryVersionState(FirstPatientRecoveryStage.NotHeard);
        return new SaveSnapshot(
            SaveSchemaVersion.Continuation,
            recovery.Discovery,
            recovery.WorldEvents,
            memory ?? recovery.CharacterMemory,
            recovery.FirstPatient,
            recovery.Journal,
            recovery.Map,
            recovery.InvestigationRoutes,
            recovery.FirstPatientRecovery,
            continuation);
    }
}
