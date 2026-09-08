using System.Text;
using Allrpcs.Application.Persistence;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class FileSaveDocumentStoreTests
{
    [Fact]
    public void SavesLoadsAndReplacesExactDocumentWithoutBom()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string path = temporaryDirectory.File("save.json");
        FileSaveDocumentStore store = new(path);
        string first = "{\"schemaVersion\":1,\"state\":\"first\"}";
        string second = "{\"schemaVersion\":1,\"state\":\"second\"}";

        SaveDocumentWriteResult firstWrite = store.Save(first);
        SaveDocumentWriteResult secondWrite = store.Save(second);
        SaveDocumentReadResult read = store.Load();

        Assert.Equal(SaveDocumentWriteResult.Saved, firstWrite);
        Assert.Equal(SaveDocumentWriteResult.Saved, secondWrite);
        Assert.Equal(SaveDocumentReadStatus.Loaded, read.Status);
        Assert.Equal(second, read.SerializedDocument);
        Assert.Equal(Encoding.UTF8.GetBytes(second), File.ReadAllBytes(path));
        Assert.Equal("{\"schemaVersion\":1,\"state\":\"first\"}", first);
        Assert.Equal("{\"schemaVersion\":1,\"state\":\"second\"}", second);
        Assert.Empty(Directory.GetFiles(temporaryDirectory.Path, "*.tmp"));
    }

    [Fact]
    public void ReportsMissingDocumentExplicitly()
    {
        using TemporaryDirectory temporaryDirectory = new();
        FileSaveDocumentStore store = new(temporaryDirectory.File("missing", "save.json"));

        SaveDocumentReadResult result = store.Load();

        Assert.Equal(SaveDocumentReadStatus.Missing, result.Status);
        Assert.Null(result.SerializedDocument);
    }

    [Fact]
    public void ReportsWriteFailureAndLeavesNoTemporaryDocument()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string blockingFile = temporaryDirectory.File("not-a-directory");
        File.WriteAllText(blockingFile, "blocking");
        FileSaveDocumentStore store = new(Path.Combine(blockingFile, "save.json"));

        SaveDocumentWriteResult result = store.Save("{\"schemaVersion\":1}");

        Assert.Equal(SaveDocumentWriteResult.Failed, result);
        Assert.Equal("blocking", File.ReadAllText(blockingFile));
        Assert.Single(Directory.GetFiles(temporaryDirectory.Path));
    }

    [Fact]
    public void ReportsEmptyAndInvalidUtf8DocumentsAsReadFailures()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string path = temporaryDirectory.File("save.json");
        FileSaveDocumentStore store = new(path);
        File.WriteAllText(path, string.Empty);

        SaveDocumentReadResult emptyResult = store.Load();
        File.WriteAllBytes(path, [0xc3, 0x28]);
        SaveDocumentReadResult invalidUtf8Result = store.Load();

        Assert.Equal(SaveDocumentReadStatus.Failed, emptyResult.Status);
        Assert.Null(emptyResult.SerializedDocument);
        Assert.Equal(SaveDocumentReadStatus.Failed, invalidUtf8Result.Status);
        Assert.Null(invalidUtf8Result.SerializedDocument);
    }

    [Fact]
    public void RejectsNullEmptyRelativeAndDirectoryPaths()
    {
        Assert.Throws<ArgumentNullException>(() => new FileSaveDocumentStore(null!));
        Assert.Throws<ArgumentException>(() => new FileSaveDocumentStore(string.Empty));
        Assert.Throws<ArgumentException>(() => new FileSaveDocumentStore("save.json"));
        Assert.Throws<ArgumentException>(
            () => new FileSaveDocumentStore(Path.GetPathRoot(Path.GetTempPath())!));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "allrpcs-file-save-tests",
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
