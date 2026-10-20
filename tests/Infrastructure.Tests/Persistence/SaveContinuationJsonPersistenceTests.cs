using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class SaveContinuationJsonPersistenceTests
{
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
            FirstPatientRecoverySaveStateJsonPersistenceTests.CurrentState(FirstPatientRecoveryStage.SageConsulted));

        SaveSnapshot current = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            versionEight,
            FirstPatientRecoverySaveStateJsonPersistenceTests.CurrentState(FirstPatientRecoveryStage.NotHeard).FirstPatient!);

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
        string valid = SaveSnapshotJsonSerializer.Serialize(Continuation(null));
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

    private static SeekerCustomizationSaveState Profile() => new(
        new CharacterIdentity(new CharacterId("seeker"), "Mara"),
        new SeekerPronouns("she", "her", "her", "hers", "herself"),
        "weathered-traveller",
        "forester");

    private static SaveSnapshot Continuation(SaveContinuationState? continuation)
    {
        SaveSnapshot recovery = FirstPatientRecoverySaveStateJsonPersistenceTests.CurrentState(FirstPatientRecoveryStage.NotHeard);
        return new SaveSnapshot(
            SaveSchemaVersion.Continuation,
            recovery.Discovery,
            recovery.WorldEvents,
            recovery.CharacterMemory,
            recovery.FirstPatient,
            recovery.Journal,
            recovery.Map,
            recovery.InvestigationRoutes,
            recovery.FirstPatientRecovery,
            continuation);
    }
}
