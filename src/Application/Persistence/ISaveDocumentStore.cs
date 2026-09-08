namespace Allrpcs.Application.Persistence;

/// <summary>
/// Stores and retrieves the already-serialized complete save document without exposing storage
/// implementation details.
/// </summary>
public interface ISaveDocumentStore
{
    /// <summary>Saves one non-empty serialized document, replacing any prior document.</summary>
    SaveDocumentWriteResult Save(string serializedDocument);

    /// <summary>Loads the current serialized document or reports why none was returned.</summary>
    SaveDocumentReadResult Load();
}
