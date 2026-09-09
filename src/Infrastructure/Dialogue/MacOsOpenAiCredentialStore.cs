using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Stores one OpenAI API key under an exact macOS Keychain service and account.</summary>
public sealed class MacOsOpenAiCredentialStore : IOpenAiCredentialStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly IMacOsKeychainProcessRunner processRunner;

    /// <summary>Gets the exact generic-password service identity.</summary>
    public const string ServiceName = "Allrpcs.OpenAI";

    /// <summary>Gets the exact generic-password account identity.</summary>
    public const string AccountName = "api-key";

    /// <summary>Gets the absolute native Keychain command path.</summary>
    public const string SecurityCommandPath = "/usr/bin/security";

    private const int MaximumNativeOutputBytes = OpenAiCredentialPolicy.MaximumApiKeyLength + 2;

    /// <summary>Creates a store behind a caller-owned native process boundary.</summary>
    public MacOsOpenAiCredentialStore(IMacOsKeychainProcessRunner processRunner)
    {
        ArgumentNullException.ThrowIfNull(processRunner);
        this.processRunner = processRunner;
    }

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        cancellationToken.ThrowIfCancellationRequested();
        if (!OpenAiCredentialPolicy.IsValid(apiKey))
        {
            return OpenAiCredentialWriteStatus.Failed;
        }

        byte[] secretBytes = StrictUtf8.GetBytes(apiKey);
        try
        {
            MacOsKeychainProcessResult result = await processRunner.RunAsync(
                    CreateCommand(
                        "add-generic-password",
                        "-U",
                        "-a",
                        AccountName,
                        "-s",
                        ServiceName,
                        "-w"),
                    secretBytes,
                    maximumStandardOutputBytes: 0,
                    cancellationToken)
                .ConfigureAwait(false);
            return result.Status == MacOsKeychainProcessStatus.Succeeded
                ? OpenAiCredentialWriteStatus.Saved
                : OpenAiCredentialWriteStatus.Failed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(secretBytes);
        }
    }

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialLoadResult> LoadApiKeyAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MacOsKeychainProcessResult result = await processRunner.RunAsync(
                CreateCommand(
                    "find-generic-password",
                    "-a",
                    AccountName,
                    "-s",
                    ServiceName,
                    "-w"),
                ReadOnlyMemory<byte>.Empty,
                MaximumNativeOutputBytes,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.Status == MacOsKeychainProcessStatus.Missing)
        {
            return OpenAiCredentialLoadResult.Missing();
        }

        if (result.Status != MacOsKeychainProcessStatus.Succeeded)
        {
            return OpenAiCredentialLoadResult.Failed();
        }

        try
        {
            string apiKey = RemoveTerminalLineEnding(StrictUtf8.GetString(result.StandardOutput.Span));
            return OpenAiCredentialPolicy.IsValid(apiKey)
                ? OpenAiCredentialLoadResult.Loaded(apiKey)
                : OpenAiCredentialLoadResult.Failed();
        }
        catch (DecoderFallbackException)
        {
            return OpenAiCredentialLoadResult.Failed();
        }
        finally
        {
            result.ClearStandardOutput();
        }
    }

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialDeleteStatus> DeleteApiKeyAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MacOsKeychainProcessResult result = await processRunner.RunAsync(
                CreateCommand(
                    "delete-generic-password",
                    "-a",
                    AccountName,
                    "-s",
                    ServiceName),
                ReadOnlyMemory<byte>.Empty,
                maximumStandardOutputBytes: 0,
                cancellationToken)
            .ConfigureAwait(false);
        return result.Status switch
        {
            MacOsKeychainProcessStatus.Succeeded => OpenAiCredentialDeleteStatus.Deleted,
            MacOsKeychainProcessStatus.Missing => OpenAiCredentialDeleteStatus.Missing,
            _ => OpenAiCredentialDeleteStatus.Failed,
        };
    }

    private static ProcessStartInfo CreateCommand(params string[] arguments)
    {
        ProcessStartInfo startInfo = new(SecurityCommandPath)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = arguments[^1] == "-w" && arguments[0] == "add-generic-password",
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static string RemoveTerminalLineEnding(string value)
    {
        if (value.EndsWith("\r\n", StringComparison.Ordinal))
        {
            return value[..^2];
        }

        return value.EndsWith('\n') ? value[..^1] : value;
    }
}
