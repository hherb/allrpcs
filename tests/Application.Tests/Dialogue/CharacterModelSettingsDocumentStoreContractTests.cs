using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsDocumentStoreContractTests
{
    [Fact]
    public void SavesAndLoadsExactDocument()
    {
        InMemoryCharacterModelSettingsDocumentStore store = new();
        string document = "{\"schemaVersion\":1}";

        CharacterModelSettingsDocumentWriteResult writeResult = store.Save(document);
        CharacterModelSettingsDocumentReadResult readResult = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Saved, writeResult);
        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Loaded, readResult.Status);
        Assert.Same(document, readResult.SerializedDocument);
    }

    [Fact]
    public void ReportsMissingDocumentExplicitly()
    {
        InMemoryCharacterModelSettingsDocumentStore store = new();

        CharacterModelSettingsDocumentReadResult result = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Missing, result.Status);
        Assert.Null(result.SerializedDocument);
    }

    [Fact]
    public void ReportsFailuresWithoutReplacingStoredDocument()
    {
        string existing = "{\"schemaVersion\":1,\"selectedProvider\":\"openai\"}";
        string replacement = "{\"schemaVersion\":1,\"selectedProvider\":\"ollama\"}";
        InMemoryCharacterModelSettingsDocumentStore store = new(existing)
        {
            FailWrites = true,
            FailReads = true,
        };

        CharacterModelSettingsDocumentWriteResult writeResult = store.Save(replacement);
        CharacterModelSettingsDocumentReadResult failedRead = store.Load();
        store.FailReads = false;
        CharacterModelSettingsDocumentReadResult retainedRead = store.Load();

        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Failed, writeResult);
        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Failed, failedRead.Status);
        Assert.Null(failedRead.SerializedDocument);
        Assert.Equal(CharacterModelSettingsDocumentReadStatus.Loaded, retainedRead.Status);
        Assert.Same(existing, retainedRead.SerializedDocument);
    }

    [Fact]
    public void RejectsNullOrEmptyDocuments()
    {
        InMemoryCharacterModelSettingsDocumentStore store = new();

        Assert.Throws<ArgumentNullException>(() => store.Save(null!));
        Assert.Throws<ArgumentException>(() => store.Save(string.Empty));
        Assert.Throws<ArgumentNullException>(
            () => CharacterModelSettingsDocumentReadResult.Loaded(null!));
        Assert.Throws<ArgumentException>(
            () => CharacterModelSettingsDocumentReadResult.Loaded(string.Empty));
    }

    [Fact]
    public void ResultFactoriesEnforceCompleteInvariants()
    {
        string document = "{\"schemaVersion\":1}";

        CharacterModelSettingsDocumentReadResult loaded =
            CharacterModelSettingsDocumentReadResult.Loaded(document);
        CharacterModelSettingsDocumentReadResult missing =
            CharacterModelSettingsDocumentReadResult.Missing();
        CharacterModelSettingsDocumentReadResult failed =
            CharacterModelSettingsDocumentReadResult.Failed();

        Assert.Same(document, loaded.SerializedDocument);
        Assert.Null(missing.SerializedDocument);
        Assert.Null(failed.SerializedDocument);
        Assert.Equal(
            [
                CharacterModelSettingsDocumentReadStatus.Loaded,
                CharacterModelSettingsDocumentReadStatus.Missing,
                CharacterModelSettingsDocumentReadStatus.Failed,
            ],
            Enum.GetValues<CharacterModelSettingsDocumentReadStatus>());
        Assert.Equal(
            [
                CharacterModelSettingsDocumentWriteResult.Saved,
                CharacterModelSettingsDocumentWriteResult.Failed,
            ],
            Enum.GetValues<CharacterModelSettingsDocumentWriteResult>());
    }

    [Fact]
    public void PortExposesOnlyStorageIndependentDocumentOperations()
    {
        Type portType = typeof(ICharacterModelSettingsDocumentStore);
        System.Reflection.MethodInfo[] methods = portType.GetMethods()
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(portType.IsInterface);
        Assert.Collection(
            methods,
            method =>
            {
                Assert.Equal(nameof(ICharacterModelSettingsDocumentStore.Load), method.Name);
                Assert.Equal(typeof(CharacterModelSettingsDocumentReadResult), method.ReturnType);
                Assert.Empty(method.GetParameters());
            },
            method =>
            {
                Assert.Equal(nameof(ICharacterModelSettingsDocumentStore.Save), method.Name);
                Assert.Equal(typeof(CharacterModelSettingsDocumentWriteResult), method.ReturnType);
                Assert.Collection(
                    method.GetParameters(),
                    parameter => Assert.Equal(typeof(string), parameter.ParameterType));
            });
    }

    private sealed class InMemoryCharacterModelSettingsDocumentStore
        : ICharacterModelSettingsDocumentStore
    {
        private string? document;

        public InMemoryCharacterModelSettingsDocumentStore(string? document = null)
        {
            this.document = document;
        }

        public bool FailReads { get; set; }

        public bool FailWrites { get; set; }

        public CharacterModelSettingsDocumentWriteResult Save(string serializedDocument)
        {
            ArgumentException.ThrowIfNullOrEmpty(serializedDocument);
            if (FailWrites)
            {
                return CharacterModelSettingsDocumentWriteResult.Failed;
            }

            document = serializedDocument;
            return CharacterModelSettingsDocumentWriteResult.Saved;
        }

        public CharacterModelSettingsDocumentReadResult Load()
        {
            if (FailReads)
            {
                return CharacterModelSettingsDocumentReadResult.Failed();
            }

            return document is null
                ? CharacterModelSettingsDocumentReadResult.Missing()
                : CharacterModelSettingsDocumentReadResult.Loaded(document);
        }
    }
}
