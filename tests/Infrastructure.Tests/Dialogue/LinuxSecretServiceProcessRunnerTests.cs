using System.Diagnostics;
using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class LinuxSecretServiceProcessRunnerTests
{
    [Fact]
    public async Task RejectsShellOrNonNativeCommandsBeforeCheckingAvailability()
    {
        LinuxSecretServiceProcessRunner runner = new();
        ProcessStartInfo shell = ValidLoadCommand();
        shell.UseShellExecute = true;
        ProcessStartInfo wrongExecutable = ValidLoadCommand();
        wrongExecutable.FileName = "/bin/sh";

        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(runner, shell));
        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(runner, wrongExecutable));
    }

    [Fact]
    public async Task RejectsAlteredIdentityArgumentsAndUnexpectedSecretInput()
    {
        LinuxSecretServiceProcessRunner runner = new();
        ProcessStartInfo alteredIdentity = ValidLoadCommand();
        alteredIdentity.ArgumentList[3] = "other-application";

        await Assert.ThrowsAsync<ArgumentException>(() => RunAsync(runner, alteredIdentity));
        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(
            ValidLoadCommand(),
            Encoding.UTF8.GetBytes("secret"),
            OpenAiCredentialPolicy.MaximumApiKeyLength,
            default).AsTask());
    }

    [Fact]
    public async Task RejectsExcessiveInputAndIncorrectOutputBounds()
    {
        LinuxSecretServiceProcessRunner runner = new();
        byte[] excessive = new byte[OpenAiCredentialPolicy.MaximumApiKeyLength + 1];

        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(
            ValidSaveCommand(),
            excessive,
            maximumStandardOutputBytes: 0,
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(
            ValidSaveCommand(),
            Encoding.UTF8.GetBytes("secret"),
            maximumStandardOutputBytes: 1,
            default).AsTask());
    }

    [Fact]
    public async Task HonorsPreCancellationBeforeCheckingAvailability()
    {
        LinuxSecretServiceProcessRunner runner = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.RunAsync(
            ValidSaveCommand(),
            Encoding.UTF8.GetBytes("secret"),
            maximumStandardOutputBytes: 0,
            cancellation.Token).AsTask());
    }

    [Fact]
    public async Task ReportsUnavailableOnNonLinuxHostsWithoutStartingAProcess()
    {
        if (OperatingSystem.IsLinux())
        {
            return;
        }

        LinuxSecretServiceProcessRunner runner = new();

        LinuxSecretServiceProcessResult result = await RunAsync(runner, ValidLoadCommand());

        Assert.False(runner.IsAvailable);
        Assert.Equal(LinuxSecretServiceProcessStatus.Unavailable, result.Status);
    }

    [Fact]
    public void ProcessResultsCopyOutputAndNeverRenderIt()
    {
        byte[] output = Encoding.UTF8.GetBytes("secret");

        LinuxSecretServiceProcessResult result =
            LinuxSecretServiceProcessResult.Succeeded(output);
        output[0] = (byte)'X';

        Assert.Equal("secret", Encoding.UTF8.GetString(result.StandardOutput.Span));
        Assert.Equal("Succeeded", result.ToString());
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    private static Task<LinuxSecretServiceProcessResult> RunAsync(
        LinuxSecretServiceProcessRunner runner,
        ProcessStartInfo startInfo) => runner.RunAsync(
            startInfo,
            ReadOnlyMemory<byte>.Empty,
            OpenAiCredentialPolicy.MaximumApiKeyLength,
            default).AsTask();

    private static ProcessStartInfo ValidLoadCommand() => CreateCommand(
        redirectInput: false,
        "lookup",
        "application",
        LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
        "account",
        LinuxSecretServiceOpenAiCredentialStore.AccountName);

    private static ProcessStartInfo ValidSaveCommand() => CreateCommand(
        redirectInput: true,
        "store",
        "--label",
        LinuxSecretServiceOpenAiCredentialStore.SecretLabel,
        "application",
        LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
        "account",
        LinuxSecretServiceOpenAiCredentialStore.AccountName);

    private static ProcessStartInfo CreateCommand(
        bool redirectInput,
        params string[] arguments)
    {
        ProcessStartInfo startInfo = new(
            LinuxSecretServiceOpenAiCredentialStore.SecretToolCommandPath)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = redirectInput,
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
