namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Stores and retrieves the already-serialized provider-settings document without exposing storage
/// implementation details.
/// </summary>
public interface ICharacterModelSettingsDocumentStore
{
    /// <summary>Saves one non-empty serialized document, replacing any prior document.</summary>
    CharacterModelSettingsDocumentWriteResult Save(string serializedDocument);

    /// <summary>Loads the current serialized document or reports why none was returned.</summary>
    CharacterModelSettingsDocumentReadResult Load();
}
