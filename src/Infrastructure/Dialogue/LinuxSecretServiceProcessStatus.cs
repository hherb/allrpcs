namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names redacted outcomes from the fixed Linux Secret Service process boundary.</summary>
public enum LinuxSecretServiceProcessStatus
{
    /// <summary>The native operation succeeded.</summary>
    Succeeded,

    /// <summary>No credential matched the exact lookup attributes.</summary>
    Missing,

    /// <summary>The required Linux Secret Service command is unavailable.</summary>
    Unavailable,

    /// <summary>The native operation failed without exposing details.</summary>
    Failed,
}
