namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names the closed redacted outcomes of loading an OpenAI credential.</summary>
public enum OpenAiCredentialLoadStatus
{
    /// <summary>A bounded API key was loaded.</summary>
    Loaded,

    /// <summary>No matching API key exists.</summary>
    Missing,

    /// <summary>The credential facility failed without exposing details.</summary>
    Failed,
}
