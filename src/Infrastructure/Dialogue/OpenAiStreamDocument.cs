using System.Text.Json;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Parses bounded-depth OpenAI Responses events, including echoed output schemas.</summary>
public static class OpenAiStreamDocument
{
    private const int MaximumEventDepth = 64;

    /// <summary>Parses strict event JSON and returns a document the caller must dispose.</summary>
    public static JsonDocument Parse(string serializedEvent)
    {
        ArgumentNullException.ThrowIfNull(serializedEvent);
        return JsonDocument.Parse(serializedEvent, new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = MaximumEventDepth,
        });
    }
}
