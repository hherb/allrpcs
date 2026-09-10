namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names redacted outcomes from the fixed Windows Credential Manager boundary.</summary>
public enum WindowsCredentialManagerStatus
{
    /// <summary>The native operation succeeded.</summary>
    Succeeded,

    /// <summary>No credential matched the exact target.</summary>
    Missing,

    /// <summary>The required Windows facility is unavailable.</summary>
    Unavailable,

    /// <summary>The native operation failed without exposing details.</summary>
    Failed,
}
