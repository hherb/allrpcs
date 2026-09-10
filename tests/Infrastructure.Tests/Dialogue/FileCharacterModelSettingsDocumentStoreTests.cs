using System.Text;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class FileCharacterModelSettingsDocumentStoreTests
{
    [Fact]
    public void SavesLoadsAndReplacesExactDocumentWithoutBom()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string path = temporaryDirectory.File("character-model-settings.json");
        FileCharacterModelSettingsDocumentStore store = new(path);
        string first = "{\"schemaVersion\":1,\"selectedProvider\":\"openai\"}";
        string second = "{\"schemaVersion\":1,\"selectedProvider\":\"ollama\"}";

        CharacterModelSettingsDocumentWriteResult firstWrite = store.Save(first);
        CharacterModelSettingsDocumentWriteResult secondWrite = store.Save(second);
        CharacterModelSettingsDocumentReadResult read = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Saved, firstWrite);
        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Saved, secondWrite);
        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Loaded, read.Status);
        Assert.Equal(second, read.SerializedDocument);
        Assert.Equal(Encoding.UTF8.GetBytes(second), File.ReadAllBytes(path));
        Assert.Equal("{\"schemaVersion\":1,\"selectedProvider\":\"openai\"}", first);
        Assert.Equal("{\"schemaVersion\":1,\"selectedProvider\":\"ollama\"}", second);
        Assert.Empty(Directory.GetFiles(temporaryDirectory.Path, "*.tmp"));
    }

    [Fact]
    public void ReportsMissingDocumentExplicitly()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FileCharacterModelSettingsDocumentStore store = new(
            temporaryDirectory.File("missing", "character-model-settings.json"));

        CharacterModelSettingsDocumentReadResult result = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Missing, result.Status);
        Assert.Null(result.SerializedDocument);
    }

    [Fact]
    public void ReportsWriteFailureAndLeavesNoTemporaryDocument()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string blockingFile = temporaryDirectory.File("not-a-directory");
        File.WriteAllText(blockingFile, "blocking");
        FileCharacterModelSettingsDocumentStore store = new(
            Path.Combine(blockingFile, "character-model-settings.json"));

        CharacterModelSettingsDocumentWriteResult result = store.Save("{\"schemaVersion\":1}");

        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Failed, result);
        Assert.Equal("blocking", File.ReadAllText(blockingFile));
        Assert.Single(Directory.GetFiles(temporaryDirectory.Path));
    }

    [Fact]
    public void ReportsEmptyAndInvalidUtf8DocumentsAsReadFailures()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string path = temporaryDirectory.File("character-model-settings.json");
        FileCharacterModelSettingsDocumentStore store = new(path);
        File.WriteAllText(path, string.Empty);

        CharacterModelSettingsDocumentReadResult emptyResult = store.Load();
        File.WriteAllBytes(path, [0xc3, 0x28]);
        CharacterModelSettingsDocumentReadResult invalidUtf8Result = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Failed, emptyResult.Status);
        Assert.Null(emptyResult.SerializedDocument);
        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Failed, invalidUtf8Result.Status);
        Assert.Null(invalidUtf8Result.SerializedDocument);
    }

    [Fact]
    public void RejectsNullEmptyRelativeAndDirectoryPathsAndDocuments()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FileCharacterModelSettingsDocumentStore store = new(
            temporaryDirectory.File("character-model-settings.json"));

        Assert.Throws<ArgumentNullException>(
            () => new FileCharacterModelSettingsDocumentStore(null!));
        Assert.Throws<ArgumentException>(
            () => new FileCharacterModelSettingsDocumentStore(string.Empty));
        Assert.Throws<ArgumentException>(
            () => new FileCharacterModelSettingsDocumentStore("character-model-settings.json"));
        Assert.Throws<ArgumentException>(
            () => new FileCharacterModelSettingsDocumentStore(Path.GetPathRoot(Path.GetTempPath())!));
        Assert.Throws<ArgumentNullException>(() => store.Save(null!));
        Assert.Throws<ArgumentException>(() => store.Save(string.Empty));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "allrpcs-character-model-settings-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string File(params string[] segments) =>
            System.IO.Path.Combine([Path, .. segments]);

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
