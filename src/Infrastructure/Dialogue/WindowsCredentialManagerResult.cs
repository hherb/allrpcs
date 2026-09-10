using System.Security.Cryptography;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reports a bounded redacted Windows Credential Manager outcome.</summary>
public sealed class WindowsCredentialManagerResult
{
    private readonly byte[] secretBytes;

    private WindowsCredentialManagerResult(
        WindowsCredentialManagerStatus status,
        ReadOnlyMemory<byte> secretBytes)
    {
        Status = status;
        this.secretBytes = secretBytes.ToArray();
    }

    /// <summary>Gets the redacted native outcome.</summary>
    public WindowsCredentialManagerStatus Status { get; }

    /// <summary>Gets bounded bytes returned only by a successful read.</summary>
    public ReadOnlyMemory<byte> SecretBytes => secretBytes;

    /// <summary>Creates a successful result with bounded secret bytes.</summary>
    public static WindowsCredentialManagerResult Succeeded(
        ReadOnlyMemory<byte> secretBytes) =>
        new(WindowsCredentialManagerStatus.Succeeded, secretBytes);

    /// <summary>Creates an exact-credential-not-found result.</summary>
    public static WindowsCredentialManagerResult Missing() =>
        new(WindowsCredentialManagerStatus.Missing, ReadOnlyMemory<byte>.Empty);

    /// <summary>Creates a required-facility-unavailable result.</summary>
    public static WindowsCredentialManagerResult Unavailable() =>
        new(WindowsCredentialManagerStatus.Unavailable, ReadOnlyMemory<byte>.Empty);

    /// <summary>Creates a redacted native-failure result.</summary>
    public static WindowsCredentialManagerResult Failed() =>
        new(WindowsCredentialManagerStatus.Failed, ReadOnlyMemory<byte>.Empty);

    /// <inheritdoc />
    public override string ToString() => Status.ToString();

    internal void ClearSecretBytes() => CryptographicOperations.ZeroMemory(secretBytes);
}
