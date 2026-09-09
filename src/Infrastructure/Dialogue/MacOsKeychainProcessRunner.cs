using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Runs the fixed native macOS Keychain process with bounded output.</summary>
public sealed class MacOsKeychainProcessRunner : IMacOsKeychainProcessRunner
{
    private const int ItemNotFoundExitCode = 44;
    private const int ReadBufferSize = 256;

    /// <inheritdoc />
    public async ValueTask<MacOsKeychainProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput,
        int maximumStandardOutputBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumStandardOutputBytes);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateCommand(startInfo, standardInput);

        using Process process = new() { StartInfo = startInfo };
        try
        {
            if (!process.Start())
            {
                return MacOsKeychainProcessResult.Failed();
            }

            Task inputTask = WriteInputAsync(process, standardInput, cancellationToken);
            Task<(byte[] Output, bool Excessive)> outputTask = ReadBoundedAsync(
                process.StandardOutput.BaseStream,
                maximumStandardOutputBytes,
                cancellationToken);
            Task errorTask = process.StandardError.BaseStream.CopyToAsync(
                Stream.Null,
                cancellationToken);
            await Task.WhenAll(
                    inputTask,
                    outputTask,
                    errorTask,
                    process.WaitForExitAsync(cancellationToken))
                .ConfigureAwait(false);

            (byte[] output, bool excessive) = await outputTask.ConfigureAwait(false);
            try
            {
                if (excessive)
                {
                    return MacOsKeychainProcessResult.Failed();
                }

                return process.ExitCode switch
                {
                    0 => MacOsKeychainProcessResult.Succeeded(output),
                    ItemNotFoundExitCode => MacOsKeychainProcessResult.Missing(),
                    _ => MacOsKeychainProcessResult.Failed(),
                };
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
            return MacOsKeychainProcessResult.Failed();
        }
    }

    private static void ValidateCommand(
        ProcessStartInfo startInfo,
        ReadOnlyMemory<byte> standardInput)
    {
        bool isSave = ArgumentsMatch(
            startInfo,
            "add-generic-password",
            "-U",
            "-a",
            MacOsOpenAiCredentialStore.AccountName,
            "-s",
            MacOsOpenAiCredentialStore.ServiceName,
            "-w");
        bool isLoad = ArgumentsMatch(
            startInfo,
            "find-generic-password",
            "-a",
            MacOsOpenAiCredentialStore.AccountName,
            "-s",
            MacOsOpenAiCredentialStore.ServiceName,
            "-w");
        bool isDelete = ArgumentsMatch(
            startInfo,
            "delete-generic-password",
            "-a",
            MacOsOpenAiCredentialStore.AccountName,
            "-s",
            MacOsOpenAiCredentialStore.ServiceName);
        if (!string.Equals(
                startInfo.FileName,
                MacOsOpenAiCredentialStore.SecurityCommandPath,
                StringComparison.Ordinal)
            || startInfo.UseShellExecute
            || startInfo.Arguments.Length > 0
            || !startInfo.RedirectStandardError
            || !startInfo.RedirectStandardOutput
            || startInfo.RedirectStandardInput != isSave
            || !(isSave || isLoad || isDelete)
            || (isSave && standardInput.IsEmpty)
            || (!isSave && !standardInput.IsEmpty)
            || standardInput.Length > OpenAiCredentialPolicy.MaximumApiKeyLength)
        {
            throw new ArgumentException("Command must use the bounded native Keychain boundary.", nameof(startInfo));
        }
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
        await process.StandardInput.BaseStream.WriteAsync("\n"u8.ToArray(), cancellationToken)
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
