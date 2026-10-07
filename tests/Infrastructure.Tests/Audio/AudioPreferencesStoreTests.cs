using Allrpcs.Application.Audio;
using Allrpcs.Infrastructure.Audio;

namespace Allrpcs.Infrastructure.Tests.Audio;

public sealed class AudioPreferencesStoreTests
{
    [Fact]
    public void RoundTripsWholeSnapshotAcrossStoreInstances()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"allrpcs-audio-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "preferences.json");
        try
        {
            FileAudioPreferencesStore store = new(path);
            Assert.Equal(AudioPreferencesLoadStatus.Missing, store.Load().Status);
            AudioPreferences preferences = new(
                AudioPreferences.Defaults().WithChannel(AudioCategory.Music, 0.37, true).Channels,
                AudioMixPreset.ReducedRange, true, false);
            Assert.True(store.Save(preferences));
            AudioPreferencesLoadResult restored = new FileAudioPreferencesStore(path).Load();
            Assert.Equal(AudioPreferencesLoadStatus.Loaded, restored.Status);
            Assert.Equal(AudioPreferencesJson.Serialize(preferences), AudioPreferencesJson.Serialize(restored.Preferences));
            Assert.True(store.Save(AudioPreferences.Defaults()));
            Assert.Single(Directory.GetFiles(directory));
            File.WriteAllText(path, "{broken");
            restored = store.Load();
            Assert.Equal(AudioPreferencesLoadStatus.Invalid, restored.Status);
            Assert.Equal(AudioPreferencesJson.Serialize(AudioPreferences.Defaults()), AudioPreferencesJson.Serialize(restored.Preferences));
        }
        finally { if (Directory.Exists(directory)) { Directory.Delete(directory, recursive: true); } }
    }

    [Theory]
    [InlineData("\"schemaVersion\":1", "\"schemaVersion\":2")]
    [InlineData("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1")]
    [InlineData("\"mono\":false", "\"mono\":\"false\"")]
    [InlineData("\"preset\":\"Default\"", "\"preset\":\"unknown\"")]
    [InlineData("\"gain\":1", "\"gain\":2")]
    [InlineData("\"category\":\"Master\"", "\"category\":\"Music\"")]
    [InlineData("\"category\":\"Master\"", "\"category\":\"0\"")]
    [InlineData("\"captions\":true,", "")]
    [InlineData("\"muted\":false", "\"muted\":false,\"unknown\":true")]
    public void RejectsMalformedOrAmbiguousDocuments(string original, string replacement)
    {
        string json = AudioPreferencesJson.Serialize(AudioPreferences.Defaults()).Replace(original, replacement, StringComparison.Ordinal);
        Assert.ThrowsAny<ArgumentException>(() => AudioPreferencesJson.Deserialize(json));
    }

    [Fact]
    public void FailedReplacementDoesNotDeleteExistingDocumentOrLeakTemporaryFile()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"allrpcs-audio-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "preferences.json");
        Directory.CreateDirectory(path);
        string retained = Path.Combine(path, "retained");
        File.WriteAllText(retained, "previous content");
        try
        {
            Assert.False(new FileAudioPreferencesStore(path).Save(AudioPreferences.Defaults()));
            Assert.Equal("previous content", File.ReadAllText(retained));
            Assert.Empty(Directory.GetFiles(directory));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
