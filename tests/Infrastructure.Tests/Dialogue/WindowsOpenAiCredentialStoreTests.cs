using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class WindowsOpenAiCredentialStoreTests
{
    [Fact]
    public async Task SavesLoadsReplacesAndDeletesOnlyTheExactCredential()
    {
        FakeCredentialManager manager = new();
        manager.Set("unrelated-target", "unrelated-user", "leave-me-alone");
        WindowsOpenAiCredentialStore store = new(manager);

        Assert.True(store.IsAvailable);
        Assert.Equal(
            OpenAiCredentialWriteStatus.Saved,
            await store.SaveApiKeyAsync("first-secret", default));
        Assert.Equal(
            OpenAiCredentialWriteStatus.Saved,
            await store.SaveApiKeyAsync("replacement-secret", default));
        OpenAiCredentialLoadResult loaded = await store.LoadApiKeyAsync(default);
        OpenAiCredentialDeleteStatus deleted = await store.DeleteApiKeyAsync(default);
        OpenAiCredentialLoadResult missing = await store.LoadApiKeyAsync(default);

        Assert.Equal(OpenAiCredentialLoadStatus.Loaded, loaded.Status);
        Assert.Equal("replacement-secret", loaded.ApiKey);
        Assert.Equal(OpenAiCredentialDeleteStatus.Deleted, deleted);
        Assert.Equal(OpenAiCredentialLoadStatus.Missing, missing.Status);
        Assert.Equal("leave-me-alone", manager.Get("unrelated-target", "unrelated-user"));
        Assert.All(manager.Invocations, invocation =>
        {
            Assert.Equal(WindowsOpenAiCredentialStore.TargetName, invocation.TargetName);
            Assert.Equal(WindowsOpenAiCredentialStore.UserName, invocation.UserName);
        });
    }

    [Fact]
    public async Task MapsUnavailableMissingAndNativeFailureWithoutDetails()
    {
        FakeCredentialManager manager = new() { IsAvailable = false };
        WindowsOpenAiCredentialStore store = new(manager);

        Assert.False(store.IsAvailable);
        Assert.Equal(
            OpenAiCredentialWriteStatus.Failed,
            await store.SaveApiKeyAsync("secret", default));
        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
        Assert.Equal(
            OpenAiCredentialDeleteStatus.Failed,
            await store.DeleteApiKeyAsync(default));
        Assert.Empty(manager.Invocations);

        manager.IsAvailable = true;
        Assert.Equal(
            OpenAiCredentialDeleteStatus.Missing,
            await store.DeleteApiKeyAsync(default));
        manager.ForcedResult = WindowsCredentialManagerResult.Failed();
        Assert.Equal(
            OpenAiCredentialWriteStatus.Failed,
            await store.SaveApiKeyAsync("secret", default));
        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
        Assert.Equal(
            OpenAiCredentialDeleteStatus.Failed,
            await store.DeleteApiKeyAsync(default));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" key with spaces ")]
    [InlineData("key\0value")]
    public async Task RejectsInvalidKeysBeforeCallingNativeBoundary(string apiKey)
    {
        FakeCredentialManager manager = new();
        WindowsOpenAiCredentialStore store = new(manager);

        OpenAiCredentialWriteStatus result = await store.SaveApiKeyAsync(apiKey, default);

        Assert.Equal(OpenAiCredentialWriteStatus.Failed, result);
        Assert.Empty(manager.Invocations);
    }

    [Fact]
    public async Task RejectsMalformedAndExcessiveNativeValuesAndClearsOutput()
    {
        WindowsCredentialManagerResult malformed =
            WindowsCredentialManagerResult.Succeeded(new byte[] { 0xc3, 0x28 });
        FakeCredentialManager manager = new() { ForcedResult = malformed };
        WindowsOpenAiCredentialStore store = new(manager);

        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
        Assert.All(malformed.SecretBytes.ToArray(), value => Assert.Equal(0, value));

        WindowsCredentialManagerResult excessive = WindowsCredentialManagerResult.Succeeded(
            Encoding.UTF8.GetBytes(new string('k', OpenAiCredentialPolicy.MaximumApiKeyLength + 1)));
        manager.ForcedResult = excessive;
        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
        Assert.All(excessive.SecretBytes.ToArray(), value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task PropagatesCancellationBeforeAvailabilityOrNativeOperations()
    {
        FakeCredentialManager manager = new() { IsAvailable = false };
        WindowsOpenAiCredentialStore store = new(manager);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.SaveApiKeyAsync("secret", cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.LoadApiKeyAsync(cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.DeleteApiKeyAsync(cancellation.Token).AsTask());
        Assert.Empty(manager.Invocations);
    }

    [Fact]
    public void RejectsNullDependenciesAndApiKeys()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowsOpenAiCredentialStore(null!));

        WindowsOpenAiCredentialStore store = new(new FakeCredentialManager());
        Assert.Throws<ArgumentNullException>(
            () => store.SaveApiKeyAsync(null!, default).AsTask().GetAwaiter().GetResult());
    }

    private sealed class FakeCredentialManager : IWindowsCredentialManager
    {
        private readonly Dictionary<(string Target, string User), byte[]> entries = [];

        public bool IsAvailable { get; set; } = true;

        public List<FakeInvocation> Invocations { get; } = [];

        public WindowsCredentialManagerResult? ForcedResult { get; set; }

        public void Set(string target, string userName, string secret) =>
            entries[(target, userName)] = Encoding.UTF8.GetBytes(secret);

        public string? Get(string target, string userName) =>
            entries.TryGetValue((target, userName), out byte[]? secret)
                ? Encoding.UTF8.GetString(secret)
                : null;

        public ValueTask<WindowsCredentialManagerResult> WriteAsync(
            string targetName,
            string userName,
            ReadOnlyMemory<byte> secretBytes,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Invocations.Add(new("write", targetName, userName));
            if (ForcedResult is not null)
            {
                return ValueTask.FromResult(ForcedResult);
            }

            entries[(targetName, userName)] = secretBytes.ToArray();
            return ValueTask.FromResult(
                WindowsCredentialManagerResult.Succeeded(ReadOnlyMemory<byte>.Empty));
        }

        public ValueTask<WindowsCredentialManagerResult> ReadAsync(
            string targetName,
            string expectedUserName,
            int maximumSecretBytes,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Invocations.Add(new("read", targetName, expectedUserName));
            if (ForcedResult is not null)
            {
                return ValueTask.FromResult(ForcedResult);
            }

            return ValueTask.FromResult(
                entries.TryGetValue((targetName, expectedUserName), out byte[]? secret)
                    && secret.Length <= maximumSecretBytes
                    ? WindowsCredentialManagerResult.Succeeded(secret)
                    : WindowsCredentialManagerResult.Missing());
        }

        public ValueTask<WindowsCredentialManagerResult> DeleteAsync(
            string targetName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Invocations.Add(new("delete", targetName, WindowsOpenAiCredentialStore.UserName));
            if (ForcedResult is not null)
            {
                return ValueTask.FromResult(ForcedResult);
            }

            bool removed = entries.Remove((targetName, WindowsOpenAiCredentialStore.UserName));
            return ValueTask.FromResult(removed
                ? WindowsCredentialManagerResult.Succeeded(ReadOnlyMemory<byte>.Empty)
                : WindowsCredentialManagerResult.Missing());
        }
    }

    private sealed record FakeInvocation(string Operation, string TargetName, string UserName);
}
