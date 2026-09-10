using System.Diagnostics;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Executes one fixed Linux Secret Service command without logging it.</summary>
public interface ILinuxSecretServiceProcessRunner
{
    /// <summary>Gets whether this host exposes the required native command.</summary>
    bool IsAvailable { get; }

    /// <summary>Runs one command with separate secret input and bounded standard output.</summary>
    ValueTask<LinuxSecretServiceProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken);
}
