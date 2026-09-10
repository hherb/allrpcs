using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class WindowsCredentialManagerTests
{
    [Fact]
    public async Task RejectsAlteredIdentitiesAndBoundsBeforeCheckingAvailability()
    {
        WindowsCredentialManager manager = new();

        await Assert.ThrowsAsync<ArgumentException>(() => manager.WriteAsync(
            "other-target",
            WindowsOpenAiCredentialStore.UserName,
            Encoding.UTF8.GetBytes("secret"),
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ReadAsync(
            WindowsOpenAiCredentialStore.TargetName,
            "other-user",
            OpenAiCredentialPolicy.MaximumApiKeyLength,
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentException>(() => manager.WriteAsync(
            WindowsOpenAiCredentialStore.TargetName,
            WindowsOpenAiCredentialStore.UserName,
            new byte[OpenAiCredentialPolicy.MaximumApiKeyLength + 1],
            default).AsTask());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => manager.ReadAsync(
            WindowsOpenAiCredentialStore.TargetName,
            WindowsOpenAiCredentialStore.UserName,
            OpenAiCredentialPolicy.MaximumApiKeyLength + 1,
            default).AsTask());
    }

    [Fact]
    public async Task HonorsPreCancellationBeforeCheckingAvailability()
    {
        WindowsCredentialManager manager = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => manager.DeleteAsync(
            WindowsOpenAiCredentialStore.TargetName,
            cancellation.Token).AsTask());
    }

    [Fact]
    public async Task ReportsUnavailableOnNonWindowsHostsWithoutCallingNativeApis()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        WindowsCredentialManager manager = new();

        WindowsCredentialManagerResult result = await manager.ReadAsync(
            WindowsOpenAiCredentialStore.TargetName,
            WindowsOpenAiCredentialStore.UserName,
            OpenAiCredentialPolicy.MaximumApiKeyLength,
            default);

        Assert.False(manager.IsAvailable);
        Assert.Equal(WindowsCredentialManagerStatus.Unavailable, result.Status);
    }

    [Fact]
    public void ResultsCopySecretBytesAndNeverRenderThem()
    {
        byte[] secret = Encoding.UTF8.GetBytes("secret");

        WindowsCredentialManagerResult result =
            WindowsCredentialManagerResult.Succeeded(secret);
        secret[0] = (byte)'X';

        Assert.Equal("secret", Encoding.UTF8.GetString(result.SecretBytes.Span));
        Assert.Equal("Succeeded", result.ToString());
        Assert.DoesNotContain("secret", result.ToString(), StringComparison.Ordinal);
    }
}
