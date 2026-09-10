using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>
/// Composes strict secret-free provider-settings JSON with one storage-independent document port.
/// </summary>
public sealed class CharacterModelSettingsDocumentController
{
    private readonly ICharacterModelSettingsDocumentStore documentStore;

    /// <summary>Creates document orchestration over the exact caller-owned store.</summary>
    public CharacterModelSettingsDocumentController(
        ICharacterModelSettingsDocumentStore documentStore)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        this.documentStore = documentStore;
    }

    /// <summary>Serializes and saves one complete provider-settings selection.</summary>
    public CharacterModelSettingsDocumentWriteResult Save(
        CharacterModelSettingsSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return documentStore.Save(CharacterModelSettingsJsonSerializer.Serialize(selection));
    }

    /// <summary>Loads and strictly materializes the current provider-settings document.</summary>
    public CharacterModelSettingsDocumentLoadResult Load()
    {
        CharacterModelSettingsDocumentReadResult readResult = documentStore.Load();
        return readResult.Status switch
        {
            CharacterModelSettingsDocumentReadStatus.Loaded =>
                Deserialize(readResult.SerializedDocument!),
            CharacterModelSettingsDocumentReadStatus.Missing =>
                CharacterModelSettingsDocumentLoadResult.Missing(),
            CharacterModelSettingsDocumentReadStatus.Failed =>
                CharacterModelSettingsDocumentLoadResult.StorageFailed(),
            _ => throw new InvalidOperationException(
                "The provider-settings document store returned an unknown read status."),
        };
    }

    private static CharacterModelSettingsDocumentLoadResult Deserialize(string serializedDocument)
    {
        try
        {
            CharacterModelSettingsSelectionSnapshot selection =
                CharacterModelSettingsJsonDeserializer.Deserialize(serializedDocument);
            return CharacterModelSettingsDocumentLoadResult.Loaded(selection);
        }
        catch (ArgumentException)
        {
            return CharacterModelSettingsDocumentLoadResult.InvalidDocument();
        }
    }
}
