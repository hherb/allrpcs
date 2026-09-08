using Allrpcs.Application.Persistence;

namespace Allrpcs.Application.Tests.Persistence;

public sealed class SaveDocumentStoreContractTests
{
    [Fact]
    public void SavesAndLoadsExactDocument()
    {
        InMemorySaveDocumentStore store = new();
        string document = "{\"schemaVersion\":1}";

        SaveDocumentWriteResult writeResult = store.Save(document);
        SaveDocumentReadResult readResult = store.Load();

        Assert.Equal(SaveDocumentWriteResult.Saved, writeResult);
        Assert.Equal(SaveDocumentReadStatus.Loaded, readResult.Status);
        Assert.Same(document, readResult.SerializedDocument);
    }

    [Fact]
    public void ReportsMissingDocumentExplicitly()
    {
        InMemorySaveDocumentStore store = new();

        SaveDocumentReadResult result = store.Load();

        Assert.Equal(SaveDocumentReadStatus.Missing, result.Status);
        Assert.Null(result.SerializedDocument);
    }

    [Fact]
    public void ReportsReadAndWriteFailuresWithoutChangingStoredDocument()
    {
        string existing = "{\"schemaVersion\":1,\"state\":\"existing\"}";
        string replacement = "{\"schemaVersion\":1,\"state\":\"replacement\"}";
        InMemorySaveDocumentStore store = new(existing)
        {
            FailWrites = true,
            FailReads = true,
        };

        SaveDocumentWriteResult writeResult = store.Save(replacement);
        SaveDocumentReadResult failedRead = store.Load();
        store.FailReads = false;
        SaveDocumentReadResult retainedRead = store.Load();

        Assert.Equal(SaveDocumentWriteResult.Failed, writeResult);
        Assert.Equal(SaveDocumentReadStatus.Failed, failedRead.Status);
        Assert.Null(failedRead.SerializedDocument);
        Assert.Equal(SaveDocumentReadStatus.Loaded, retainedRead.Status);
        Assert.Same(existing, retainedRead.SerializedDocument);
    }

    [Fact]
    public void ReplacesPriorDocumentWithoutChangingCallerInputs()
    {
        string first = "{\"schemaVersion\":1,\"state\":\"first\"}";
        string second = "{\"schemaVersion\":1,\"state\":\"second\"}";
        InMemorySaveDocumentStore store = new();

        Assert.Equal(SaveDocumentWriteResult.Saved, store.Save(first));
        Assert.Equal(SaveDocumentWriteResult.Saved, store.Save(second));
        SaveDocumentReadResult result = store.Load();

        Assert.Equal("{\"schemaVersion\":1,\"state\":\"first\"}", first);
        Assert.Equal("{\"schemaVersion\":1,\"state\":\"second\"}", second);
        Assert.Same(second, result.SerializedDocument);
    }

    [Fact]
    public void LoadedResultRejectsNullOrEmptyDocument()
    {
        Assert.Throws<ArgumentNullException>(() => SaveDocumentReadResult.Loaded(null!));
        Assert.Throws<ArgumentException>(() => SaveDocumentReadResult.Loaded(string.Empty));
    }

    [Fact]
    public void PortExposesOnlyStorageIndependentDocumentOperations()
    {
        Type portType = typeof(ISaveDocumentStore);
        System.Reflection.MethodInfo[] methods = portType.GetMethods()
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(portType.IsInterface);
        Assert.Collection(
            methods,
            method =>
            {
                Assert.Equal(nameof(ISaveDocumentStore.Load), method.Name);
                Assert.Equal(typeof(SaveDocumentReadResult), method.ReturnType);
                Assert.Empty(method.GetParameters());
            },
            method =>
            {
                Assert.Equal(nameof(ISaveDocumentStore.Save), method.Name);
                Assert.Equal(typeof(SaveDocumentWriteResult), method.ReturnType);
                Assert.Collection(
                    method.GetParameters(),
                    parameter => Assert.Equal(typeof(string), parameter.ParameterType));
            });
    }

    private sealed class InMemorySaveDocumentStore : ISaveDocumentStore
    {
        private string? document;

        public InMemorySaveDocumentStore(string? document = null)
        {
            this.document = document;
        }

        public bool FailReads { get; set; }

        public bool FailWrites { get; set; }

        public SaveDocumentWriteResult Save(string serializedDocument)
        {
            ArgumentException.ThrowIfNullOrEmpty(serializedDocument);
            if (FailWrites)
            {
                return SaveDocumentWriteResult.Failed;
            }

            document = serializedDocument;
            return SaveDocumentWriteResult.Saved;
        }

        public SaveDocumentReadResult Load()
        {
            if (FailReads)
            {
                return SaveDocumentReadResult.Failed();
            }

            return document is null
                ? SaveDocumentReadResult.Missing()
                : SaveDocumentReadResult.Loaded(document);
        }
    }
}
