namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names the closed redacted outcomes of deleting an OpenAI credential.</summary>
public enum OpenAiCredentialDeleteStatus
{
    /// <summary>The existing API key was deleted.</summary>
    Deleted,

    /// <summary>No matching API key exists.</summary>
    Missing,

    /// <summary>The credential facility failed without exposing details.</summary>
    Failed,
}
