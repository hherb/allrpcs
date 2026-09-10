using System.Security.Cryptography;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reports a bounded redacted Linux Secret Service process outcome.</summary>
public sealed class LinuxSecretServiceProcessResult
{
    private readonly byte[] standardOutput;

    private LinuxSecretServiceProcessResult(
        LinuxSecretServiceProcessStatus status,
        ReadOnlyMemory<byte> standardOutput)
    {
        Status = status;
        this.standardOutput = standardOutput.ToArray();
    }

    /// <summary>Gets the redacted process outcome.</summary>
    public LinuxSecretServiceProcessStatus Status { get; }

    /// <summary>Gets bounded output bytes returned only by a successful lookup.</summary>
    public ReadOnlyMemory<byte> StandardOutput => standardOutput;

    /// <summary>Creates a successful result with bounded standard output.</summary>
    public static LinuxSecretServiceProcessResult Succeeded(
        ReadOnlyMemory<byte> standardOutput) =>
        new(LinuxSecretServiceProcessStatus.Succeeded, standardOutput);

    /// <summary>Creates an exact-credential-not-found result.</summary>
    public static LinuxSecretServiceProcessResult Missing() =>
        new(LinuxSecretServiceProcessStatus.Missing, ReadOnlyMemory<byte>.Empty);

    /// <summary>Creates a required-facility-unavailable result.</summary>
    public static LinuxSecretServiceProcessResult Unavailable() =>
        new(LinuxSecretServiceProcessStatus.Unavailable, ReadOnlyMemory<byte>.Empty);

    /// <summary>Creates a redacted process-failure result.</summary>
    public static LinuxSecretServiceProcessResult Failed() =>
        new(LinuxSecretServiceProcessStatus.Failed, ReadOnlyMemory<byte>.Empty);

    /// <inheritdoc />
    public override string ToString() => Status.ToString();

    internal void ClearStandardOutput() =>
        CryptographicOperations.ZeroMemory(standardOutput);
}
