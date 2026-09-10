using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class CharacterModelSettingsDocumentControllerTests
{
    [Fact]
    public void SavesCanonicalSecretFreeDocumentThroughExactStore()
    {
        RecordingDocumentStore store = new();
        CharacterModelSettingsDocumentController controller = new(store);
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-5");
        CharacterModelSettingsSelectionSnapshot selection = new(
            [settings],
            CharacterModelProvider.OpenAi);

        CharacterModelSettingsDocumentWriteResult result = controller.Save(selection);

        Assert.Equal(CharacterModelSettingsDocumentWriteResult.Saved, result);
        Assert.Equal(
            "{\"schemaVersion\":1,\"selectedProvider\":\"openai\","
                + "\"settings\":[{\"provider\":\"openai\",\"modelName\":\"gpt-5\"}]}",
            store.SavedDocument);
        Assert.Same(settings, Assert.Single(selection.Settings));
        Assert.Same(CharacterModelProvider.OpenAi, selection.SelectedProvider);
    }

    [Fact]
    public void ReturnsExactMaterializedSelectionFromLoadedDocument()
    {
        string document = "{\"schemaVersion\":1,\"selectedProvider\":\"ollama\","
            + "\"settings\":[{\"provider\":\"openai\",\"modelName\":\"gpt-5\"},"
            + "{\"provider\":\"ollama\",\"modelName\":\"gemma3\"}]}";
        RecordingDocumentStore store = new(
            CharacterModelSettingsDocumentReadResult.Loaded(document));
        CharacterModelSettingsDocumentController controller = new(store);

        CharacterModelSettingsDocumentLoadResult result = controller.Load();

        Assert.Equal(CharacterModelSettingsDocumentLoadStatus.Loaded, result.Status);
        CharacterModelSettingsSelectionSnapshot selection =
            Assert.IsType<CharacterModelSettingsSelectionSnapshot>(result.Selection);
        Assert.Same(selection, result.Selection);
        Assert.Same(CharacterModelProvider.Ollama, selection.SelectedProvider);
        Assert.Collection(
            selection.Settings,
            settings =>
            {
                Assert.Same(CharacterModelProvider.OpenAi, settings.Provider);
                Assert.Equal("gpt-5", settings.ModelName);
            },
            settings =>
            {
                Assert.Same(CharacterModelProvider.Ollama, settings.Provider);
                Assert.Equal("gemma3", settings.ModelName);
            });
    }

    [Theory]
    [InlineData(CharacterModelSettingsDocumentReadStatus.Missing)]
    [InlineData(CharacterModelSettingsDocumentReadStatus.Failed)]
    public void PreservesStorageOutcomeWithoutCreatingASelection(
        CharacterModelSettingsDocumentReadStatus readStatus)
    {
        CharacterModelSettingsDocumentReadResult readResult = readStatus switch
        {
            CharacterModelSettingsDocumentReadStatus.Missing =>
                CharacterModelSettingsDocumentReadResult.Missing(),
            CharacterModelSettingsDocumentReadStatus.Failed =>
                CharacterModelSettingsDocumentReadResult.Failed(),
            _ => throw new InvalidOperationException(),
        };
        CharacterModelSettingsDocumentController controller = new(
            new RecordingDocumentStore(readResult));

        CharacterModelSettingsDocumentLoadResult result = controller.Load();

        CharacterModelSettingsDocumentLoadStatus expected = readStatus
            == CharacterModelSettingsDocumentReadStatus.Missing
                ? CharacterModelSettingsDocumentLoadStatus.Missing
                : CharacterModelSettingsDocumentLoadStatus.StorageFailed;
        Assert.Equal(expected, result.Status);
        Assert.Null(result.Selection);
    }

    [Fact]
    public void RejectsMalformedLoadedDocumentWithoutLeakingParserFailure()
    {
        RecordingDocumentStore store = new(
            CharacterModelSettingsDocumentReadResult.Loaded(
                "{\"schemaVersion\":2,\"credential\":\"secret\"}"));
        CharacterModelSettingsDocumentController controller = new(store);

        CharacterModelSettingsDocumentLoadResult result = controller.Load();

        Assert.Equal(CharacterModelSettingsDocumentLoadStatus.InvalidDocument, result.Status);
        Assert.Null(result.Selection);
    }

    [Fact]
    public void RejectsNullDependenciesAndInputs()
    {
        RecordingDocumentStore store = new();
        CharacterModelSettingsDocumentController controller = new(store);

        Assert.Throws<ArgumentNullException>(
            () => new CharacterModelSettingsDocumentController(null!));
        Assert.Throws<ArgumentNullException>(() => controller.Save(null!));
    }

    [Fact]
    public void LoadResultFactoriesEnforceClosedInvariants()
    {
        CharacterModelSettingsSelectionSnapshot selection = new(
            [new CharacterModelSettings(CharacterModelProvider.Anthropic, "claude-sonnet-4")],
            CharacterModelProvider.Anthropic);

        CharacterModelSettingsDocumentLoadResult loaded =
            CharacterModelSettingsDocumentLoadResult.Loaded(selection);
        CharacterModelSettingsDocumentLoadResult missing =
            CharacterModelSettingsDocumentLoadResult.Missing();
        CharacterModelSettingsDocumentLoadResult storageFailed =
            CharacterModelSettingsDocumentLoadResult.StorageFailed();
        CharacterModelSettingsDocumentLoadResult invalid =
            CharacterModelSettingsDocumentLoadResult.InvalidDocument();

        Assert.Same(selection, loaded.Selection);
        Assert.All(
            [missing, storageFailed, invalid],
            result => Assert.Null(result.Selection));
        Assert.Equal(
            [
                CharacterModelSettingsDocumentLoadStatus.Loaded,
                CharacterModelSettingsDocumentLoadStatus.Missing,
                CharacterModelSettingsDocumentLoadStatus.StorageFailed,
                CharacterModelSettingsDocumentLoadStatus.InvalidDocument,
            ],
            Enum.GetValues<CharacterModelSettingsDocumentLoadStatus>());
    }

    private sealed class RecordingDocumentStore : ICharacterModelSettingsDocumentStore
    {
        private readonly CharacterModelSettingsDocumentReadResult readResult;

        public RecordingDocumentStore(
            CharacterModelSettingsDocumentReadResult? readResult = null)
        {
            this.readResult = readResult ?? CharacterModelSettingsDocumentReadResult.Missing();
        }

        public string? SavedDocument { get; private set; }

        public CharacterModelSettingsDocumentWriteResult Save(string serializedDocument)
        {
            SavedDocument = serializedDocument;
            return CharacterModelSettingsDocumentWriteResult.Saved;
        }

        public CharacterModelSettingsDocumentReadResult Load() => readResult;
    }
}
