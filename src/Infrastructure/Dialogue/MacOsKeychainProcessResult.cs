namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Reports a bounded redacted native keychain-process outcome.</summary>
public sealed class MacOsKeychainProcessResult
{
    private readonly byte[] standardOutput;

    private MacOsKeychainProcessResult(
        MacOsKeychainProcessStatus status,
        ReadOnlyMemory<byte> standardOutput)
    {
        Status = status;
        this.standardOutput = standardOutput.ToArray();
    }

    /// <summary>Gets the redacted process outcome.</summary>
    public MacOsKeychainProcessStatus Status { get; }

    /// <summary>Gets the bounded output bytes returned only by a successful operation.</summary>
    public ReadOnlyMemory<byte> StandardOutput => standardOutput;

    /// <summary>Creates a successful result with bounded standard output.</summary>
    public static MacOsKeychainProcessResult Succeeded(ReadOnlyMemory<byte> standardOutput) =>
        new(MacOsKeychainProcessStatus.Succeeded, standardOutput);

    /// <summary>Creates an item-not-found result.</summary>
    public static MacOsKeychainProcessResult Missing() =>
        new(MacOsKeychainProcessStatus.Missing, ReadOnlyMemory<byte>.Empty);

    /// <summary>Creates a redacted process-failure result.</summary>
    public static MacOsKeychainProcessResult Failed() =>
        new(MacOsKeychainProcessStatus.Failed, ReadOnlyMemory<byte>.Empty);

    /// <inheritdoc />
    public override string ToString() => Status.ToString();

    internal void ClearStandardOutput() =>
        System.Security.Cryptography.CryptographicOperations.ZeroMemory(standardOutput);
}
