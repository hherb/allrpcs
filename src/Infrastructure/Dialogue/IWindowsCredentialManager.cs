namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Executes exact Windows Credential Manager operations without logging them.</summary>
public interface IWindowsCredentialManager
{
    /// <summary>Gets whether this host exposes Windows Credential Manager.</summary>
    bool IsAvailable { get; }

    /// <summary>Writes or replaces one caller-owned bounded secret.</summary>
    ValueTask<WindowsCredentialManagerResult> WriteAsync(
        string targetName,
        string userName,
        ReadOnlyMemory<byte> secretBytes,
        CancellationToken cancellationToken);

    /// <summary>Reads one bounded secret with its expected user identity.</summary>
    ValueTask<WindowsCredentialManagerResult> ReadAsync(
        string targetName,
        string expectedUserName,
        int maximumSecretBytes,
        CancellationToken cancellationToken);

    /// <summary>Deletes one exact target.</summary>
    ValueTask<WindowsCredentialManagerResult> DeleteAsync(
        string targetName,
        CancellationToken cancellationToken);
}
