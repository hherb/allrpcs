using Allrpcs.Application.Input;
using Allrpcs.Infrastructure.Input;

namespace Allrpcs.Infrastructure.Tests.Input;

public sealed class FileControlPreferencesStoreTests
{
    private static readonly string[] Siblings = ["world.json", "provider.json", "audio.json"];
    [Fact]
    public void MissingMalformedAndOversizedDocumentsUseCompleteDefaultsWithoutOverwriting()
    {
        WithDirectory(directory =>
        {
            string path = Path.Combine(directory, "controls.json"); FileControlPreferencesStore store = new(path);
            Assert.Equal(ControlPreferencesLoadStatus.Missing, store.Load().Status); Assert.False(File.Exists(path));
            foreach (byte[] bytes in new[] { new byte[] { 255, 254 }, "{bad"u8.ToArray(), new byte[65537] })
            {
                File.WriteAllBytes(path, bytes); ControlPreferencesLoadResult result = store.Load();
                Assert.Equal(ControlPreferencesLoadStatus.Invalid, result.Status); Assert.Equal(29, result.Preferences.Actions.Count);
                Assert.Equal(bytes, File.ReadAllBytes(path));
            }
        });
    }
    [Fact]
    public void AtomicSaveReloadResetNeverTouchesSiblingDocuments()
    {
        WithDirectory(directory =>
        {
            foreach (string name in Siblings) { File.WriteAllText(Path.Combine(directory, name), name); }
            string path = Path.Combine(directory, "controls.json"); FileControlPreferencesStore store = new(path);
            ControlPreferences value = ControlBindingEdit.Set(ControlPreferences.Defaults(), "save_current", new("mouseButton", 8));
            Assert.True(store.Save(value)); FileControlPreferencesStore restarted = new(path);
            Assert.Equal(ControlPreferencesLoadStatus.Loaded, restarted.Load().Status);
            Assert.Equal(ControlPreferencesJson.Serialize(value), ControlPreferencesJson.Serialize(restarted.Load().Preferences));
            Assert.True(restarted.Save(ControlPreferences.Defaults())); Assert.Single(restarted.Load().Preferences.Get("save_current").Bindings);
            Assert.Empty(Directory.GetFiles(directory, ".*.tmp"));
            foreach (string name in Siblings) { Assert.Equal(name, File.ReadAllText(Path.Combine(directory, name))); }
        });
    }
    [Fact]
    public void ReplacementFailureKeepsPreviousTargetAndCleansOwnedTemporaryFile()
    {
        WithDirectory(directory =>
        {
            string target = Path.Combine(directory, "controls.json"); Directory.CreateDirectory(target);
            string retained = Path.Combine(target, "retained"); File.WriteAllText(retained, "unchanged");
            FileControlPreferencesStore store = new(target); Assert.False(store.Save(ControlPreferences.Defaults()));
            Assert.Equal("unchanged", File.ReadAllText(retained)); Assert.Empty(Directory.GetFiles(directory, ".*.tmp"));
        });
    }
    [Fact]
    public void CompositionRejectsRelativeAndDirectoryOnlyPaths()
    {
        Assert.Throws<ArgumentException>(() => new FileControlPreferencesStore("controls.json"));
        Assert.Throws<ArgumentException>(() => new FileControlPreferencesStore(Path.GetPathRoot(Path.GetTempPath())!));
    }
    private static void WithDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "allrpcs-control-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory); try { action(directory); } finally { Directory.Delete(directory, true); }
    }
}
