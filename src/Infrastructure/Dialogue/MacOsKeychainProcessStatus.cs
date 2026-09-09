namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Names the closed redacted outcomes returned by the native keychain boundary.</summary>
public enum MacOsKeychainProcessStatus
{
    /// <summary>The native operation succeeded.</summary>
    Succeeded,

    /// <summary>The exact service and account were not found.</summary>
    Missing,

    /// <summary>The native operation failed without exposing OS details.</summary>
    Failed,
}
