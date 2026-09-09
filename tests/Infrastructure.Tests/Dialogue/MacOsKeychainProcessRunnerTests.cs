using System.Diagnostics;
using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class MacOsKeychainProcessRunnerTests
{
    [Fact]
    public async Task RejectsShellOrNonNativeCommandsBeforeStartingAProcess()
    {
        MacOsKeychainProcessRunner runner = new();
        ProcessStartInfo shell = ValidLoadCommand();
        shell.UseShellExecute = true;
        ProcessStartInfo wrongExecutable = ValidLoadCommand();
        wrongExecutable.FileName = "/bin/sh";

        await Assert.ThrowsAsync<ArgumentException>(
            () => RunAsync(runner, shell));
        await Assert.ThrowsAsync<ArgumentException>(
            () => RunAsync(runner, wrongExecutable));
    }

    [Fact]
    public async Task RejectsAlteredIdentityArgumentsAndUnexpectedSecretInput()
    {
        MacOsKeychainProcessRunner runner = new();
        ProcessStartInfo alteredIdentity = ValidLoadCommand();
        alteredIdentity.ArgumentList[4] = "other-service";

        await Assert.ThrowsAsync<ArgumentException>(
            () => RunAsync(runner, alteredIdentity));
        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(
            ValidLoadCommand(),
            Encoding.UTF8.GetBytes("secret"),
            OpenAiCredentialPolicy.MaximumApiKeyLength + 2,
            default).AsTask());
    }

    [Fact]
    public async Task RejectsExcessiveInputAndHonorsPreCancellationWithoutStarting()
    {
        MacOsKeychainProcessRunner runner = new();
        ProcessStartInfo save = ValidSaveCommand();
        byte[] excessive = new byte[OpenAiCredentialPolicy.MaximumApiKeyLength + 1];
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(
            save,
            excessive,
            maximumStandardOutputBytes: 0,
            default).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.RunAsync(
            ValidSaveCommand(),
            Encoding.UTF8.GetBytes("secret"),
            maximumStandardOutputBytes: 0,
            cancellation.Token).AsTask());
    }

    [Fact]
    public void ProcessResultsCopyOutputAndNeverRenderIt()
    {
        byte[] output = Encoding.UTF8.GetBytes("secret");

        MacOsKeychainProcessResult result = MacOsKeychainProcessResult.Succeeded(output);
        output[0] = (byte)'X';

        Assert.Equal("secret", Encoding.UTF8.GetString(result.StandardOutput.Span));
        Assert.Equal("Succeeded", result.ToString());
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }

    private static Task<MacOsKeychainProcessResult> RunAsync(
        MacOsKeychainProcessRunner runner,
        ProcessStartInfo startInfo) => runner.RunAsync(
            startInfo,
            ReadOnlyMemory<byte>.Empty,
            OpenAiCredentialPolicy.MaximumApiKeyLength + 2,
            default).AsTask();

    private static ProcessStartInfo ValidLoadCommand() => CreateCommand(
        redirectInput: false,
        "find-generic-password",
        "-a",
        MacOsOpenAiCredentialStore.AccountName,
        "-s",
        MacOsOpenAiCredentialStore.ServiceName,
        "-w");

    private static ProcessStartInfo ValidSaveCommand() => CreateCommand(
        redirectInput: true,
        "add-generic-password",
        "-U",
        "-a",
        MacOsOpenAiCredentialStore.AccountName,
        "-s",
        MacOsOpenAiCredentialStore.ServiceName,
        "-w");

    private static ProcessStartInfo CreateCommand(
        bool redirectInput,
        params string[] arguments)
    {
        ProcessStartInfo startInfo = new(MacOsOpenAiCredentialStore.SecurityCommandPath)
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
