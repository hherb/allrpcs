using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Runs the fixed native Linux Secret Service process with bounded output.</summary>
public sealed class LinuxSecretServiceProcessRunner : ILinuxSecretServiceProcessRunner
{
    private const int FailureOrMissingExitCode = 1;
    private const int ReadBufferSize = 256;

    /// <inheritdoc />
    public bool IsAvailable =>
        OperatingSystem.IsLinux()
        && File.Exists(LinuxSecretServiceOpenAiCredentialStore.SecretToolCommandPath);

    /// <inheritdoc />
    public async ValueTask<LinuxSecretServiceProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumStandardOutputBytes);
        cancellationToken.ThrowIfCancellationRequested();
        bool isLookup = ValidateCommand(startInfo, standardInput, maximumStandardOutputBytes);
        if (!IsAvailable)
        {
            return LinuxSecretServiceProcessResult.Unavailable();
        }

        using Process process = new() { StartInfo = startInfo };
        try
        {
            if (!process.Start())
            {
                return LinuxSecretServiceProcessResult.Failed();
            }

            Task inputTask = WriteInputAsync(process, standardInput, cancellationToken);
            Task<(byte[] Output, bool Excessive)> outputTask = ReadBoundedAsync(
                process.StandardOutput.BaseStream,
                maximumStandardOutputBytes,
                cancellationToken);
            Task<bool> errorTask = DrainAsync(
                process.StandardError.BaseStream,
                cancellationToken);
            await Task.WhenAll(
                    inputTask,
                    outputTask,
                    errorTask,
                    process.WaitForExitAsync(cancellationToken))
                .ConfigureAwait(false);

            (byte[] output, bool excessive) = await outputTask.ConfigureAwait(false);
            bool hasStandardError = await errorTask.ConfigureAwait(false);
            try
            {
                if (excessive)
                {
                    return LinuxSecretServiceProcessResult.Failed();
                }

                if (process.ExitCode == 0)
                {
                    return LinuxSecretServiceProcessResult.Succeeded(output);
                }

                return isLookup
                       && process.ExitCode == FailureOrMissingExitCode
                       && !hasStandardError
                       && output.Length == 0
                    ? LinuxSecretServiceProcessResult.Missing()
                    : LinuxSecretServiceProcessResult.Failed();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(output);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw;
        }
        catch (Exception exception) when (exception is Win32Exception
                                               or InvalidOperationException
                                               or IOException
                                               or NotSupportedException)
        {
            TryKill(process);
            return LinuxSecretServiceProcessResult.Failed();
        }
    }

    private static bool ValidateCommand(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput,
        int maximumStandardOutputBytes)
    {
        bool isSave = ArgumentsMatch(
            startInfo,
            "store",
            "--label",
            LinuxSecretServiceOpenAiCredentialStore.SecretLabel,
            "application",
            LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
            "account",
            LinuxSecretServiceOpenAiCredentialStore.AccountName);
        bool isLookup = ArgumentsMatch(
            startInfo,
            "lookup",
            "application",
            LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
            "account",
            LinuxSecretServiceOpenAiCredentialStore.AccountName);
        bool isClear = ArgumentsMatch(
            startInfo,
            "clear",
            "application",
            LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
            "account",
            LinuxSecretServiceOpenAiCredentialStore.AccountName);
        bool outputBoundMatches = isLookup
            ? maximumStandardOutputBytes == OpenAiCredentialPolicy.MaximumApiKeyLength
            : maximumStandardOutputBytes == 0;
        if (!string.Equals(
                startInfo.FileName,
                LinuxSecretServiceOpenAiCredentialStore.SecretToolCommandPath,
                StringComparison.Ordinal)
            || startInfo.UseShellExecute
            || !startInfo.CreateNoWindow
            || startInfo.Arguments.Length > 0
            || !startInfo.RedirectStandardError
            || !startInfo.RedirectStandardOutput
            || startInfo.RedirectStandardInput != isSave
            || !(isSave || isLookup || isClear)
            || !outputBoundMatches
            || (isSave && standardInput.IsEmpty)
            || (!isSave && !standardInput.IsEmpty)
            || standardInput.Length > OpenAiCredentialPolicy.MaximumApiKeyLength)
        {
            throw new ArgumentException(
                "Command must use the bounded native Secret Service boundary.",
                nameof(startInfo));
        }

        return isLookup;
    }

    private static bool ArgumentsMatch(ProcessStartInfo startInfo, params string[] expected) =>
        startInfo.ArgumentList.SequenceEqual(expected, StringComparer.Ordinal);

    private static async Task WriteInputAsync(
        Process process,
        ReadOnlyMemory<byte> standardInput,
        CancellationToken cancellationToken)
    {
        if (!process.StartInfo.RedirectStandardInput)
        {
            return;
        }

        await process.StandardInput.BaseStream.WriteAsync(standardInput, cancellationToken)
            .ConfigureAwait(false);
        await process.StandardInput.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        process.StandardInput.Close();
    }

    private static async Task<(byte[] Output, bool Excessive)> ReadBoundedAsync(
        Stream stream,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        using MemoryStream retained = new(Math.Min(maximumBytes, ReadBufferSize));
        byte[] buffer = new byte[ReadBufferSize];
        try
        {
            bool excessive = false;
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                int retainCount = Math.Min(read, Math.Max(0, maximumBytes - (int)retained.Length));
                if (retainCount > 0)
                {
                    retained.Write(buffer, 0, retainCount);
                }

                excessive |= read > retainCount;
            }

            return (retained.ToArray(), excessive);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(buffer);
        }
    }

    private static async Task<bool> DrainAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[ReadBufferSize];
        try
        {
            bool hasData = false;
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                hasData = true;
            }

            return hasData;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(buffer);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException
                                               or NotSupportedException
                                               or Win32Exception)
        {
            // Cancellation or failure remains the authoritative redacted outcome.
        }
    }
}
