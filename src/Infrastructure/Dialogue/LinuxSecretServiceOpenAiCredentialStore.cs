using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Stores one OpenAI API key under fixed Linux Secret Service attributes.</summary>
public sealed class LinuxSecretServiceOpenAiCredentialStore : IOpenAiCredentialStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly ILinuxSecretServiceProcessRunner processRunner;

    /// <summary>Gets the exact application attribute value.</summary>
    public const string ApplicationName = "Allrpcs.OpenAI";

    /// <summary>Gets the exact account attribute value.</summary>
    public const string AccountName = "api-key";

    /// <summary>Gets the fixed user-visible Secret Service item label.</summary>
    public const string SecretLabel = "Allrpcs OpenAI API key";

    /// <summary>Gets the absolute native Secret Service command path.</summary>
    public const string SecretToolCommandPath = "/usr/bin/secret-tool";

    private const int MaximumNativeOutputBytes = OpenAiCredentialPolicy.MaximumApiKeyLength;

    /// <summary>Creates a store behind a caller-owned native process boundary.</summary>
    public LinuxSecretServiceOpenAiCredentialStore(
        ILinuxSecretServiceProcessRunner processRunner)
    {
        ArgumentNullException.ThrowIfNull(processRunner);
        this.processRunner = processRunner;
    }

    /// <summary>Gets whether the required native Secret Service command is available.</summary>
    public bool IsAvailable => processRunner.IsAvailable;

    /// <inheritdoc />
    public async ValueTask<OpenAiCredentialWriteStatus> SaveApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        cancellationToken.ThrowIfCancellationRequested();
        if (!OpenAiCredentialPolicy.IsValid(apiKey) || !IsAvailable)
        {
            return OpenAiCredentialWriteStatus.Failed;
        }

        byte[] secretBytes = StrictUtf8.GetBytes(apiKey);
        try
        {
            LinuxSecretServiceProcessResult result = await processRunner.RunAsync(
                    CreateCommand(
                        redirectStandardInput: true,
                        "store",
                        "--label",
                        SecretLabel,
                        "application",
                        ApplicationName,
                        "account",
                        AccountName),
                    secretBytes,
                    maximumStandardOutputBytes: 0,
                    cancellationToken)
                .ConfigureAwait(false);
            return result.Status == LinuxSecretServiceProcessStatus.Succeeded
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
        if (!IsAvailable)
        {
            return OpenAiCredentialLoadResult.Failed();
        }

        LinuxSecretServiceProcessResult result = await LookupAsync(cancellationToken)
            .ConfigureAwait(false);
        if (result.Status == LinuxSecretServiceProcessStatus.Missing)
        {
            return OpenAiCredentialLoadResult.Missing();
        }

        if (result.Status != LinuxSecretServiceProcessStatus.Succeeded)
        {
            return OpenAiCredentialLoadResult.Failed();
        }

        try
        {
            string apiKey = StrictUtf8.GetString(result.StandardOutput.Span);
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
        if (!IsAvailable)
        {
            return OpenAiCredentialDeleteStatus.Failed;
        }

        LinuxSecretServiceProcessResult lookup = await LookupAsync(cancellationToken)
            .ConfigureAwait(false);
        if (lookup.Status == LinuxSecretServiceProcessStatus.Missing)
        {
            return OpenAiCredentialDeleteStatus.Missing;
        }

        if (lookup.Status != LinuxSecretServiceProcessStatus.Succeeded)
        {
            return OpenAiCredentialDeleteStatus.Failed;
        }

        lookup.ClearStandardOutput();
        LinuxSecretServiceProcessResult clear = await processRunner.RunAsync(
                CreateCommand(
                    redirectStandardInput: false,
                    "clear",
                    "application",
                    ApplicationName,
                    "account",
                    AccountName),
                ReadOnlyMemory<byte>.Empty,
                maximumStandardOutputBytes: 0,
                cancellationToken)
            .ConfigureAwait(false);
        return clear.Status == LinuxSecretServiceProcessStatus.Succeeded
            ? OpenAiCredentialDeleteStatus.Deleted
            : OpenAiCredentialDeleteStatus.Failed;
    }

    private ValueTask<LinuxSecretServiceProcessResult> LookupAsync(
        CancellationToken cancellationToken) =>
        processRunner.RunAsync(
            CreateCommand(
                redirectStandardInput: false,
                "lookup",
                "application",
                ApplicationName,
                "account",
                AccountName),
            ReadOnlyMemory<byte>.Empty,
            MaximumNativeOutputBytes,
            cancellationToken);

    private static ProcessStartInfo CreateCommand(
        bool redirectStandardInput,
        params string[] arguments)
    {
        ProcessStartInfo startInfo = new(SecretToolCommandPath)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = redirectStandardInput,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }
}
