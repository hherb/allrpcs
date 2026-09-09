using System.Diagnostics;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Executes one caller-described native macOS Keychain command without logging it.</summary>
public interface IMacOsKeychainProcessRunner
{
    /// <summary>
    /// Runs one command with separate caller-owned secret input and bounded standard output.
    /// </summary>
    ValueTask<MacOsKeychainProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken);
}
