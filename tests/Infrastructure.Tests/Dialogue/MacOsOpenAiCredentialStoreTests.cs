using System.Diagnostics;
using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class MacOsOpenAiCredentialStoreTests
{
    [Fact]
    public async Task SavesLoadsReplacesAndDeletesOnlyTheExactCredential()
    {
        FakeKeychainProcessRunner runner = new();
        runner.Set("unrelated-service", "unrelated-account", "leave-me-alone");
        MacOsOpenAiCredentialStore store = new(runner);

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
        Assert.Equal("leave-me-alone", runner.Get("unrelated-service", "unrelated-account"));
        Assert.All(runner.Invocations, AssertSafeInvocation);
    }

    [Fact]
    public async Task ReportsMissingDeleteAndMapsNativeFailuresWithoutDetails()
    {
        FakeKeychainProcessRunner runner = new();
        MacOsOpenAiCredentialStore store = new(runner);

        Assert.Equal(
            OpenAiCredentialDeleteStatus.Missing,
            await store.DeleteApiKeyAsync(default));

        runner.ForcedResult = MacOsKeychainProcessResult.Failed();
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
        FakeKeychainProcessRunner runner = new();
        MacOsOpenAiCredentialStore store = new(runner);

        OpenAiCredentialWriteStatus result = await store.SaveApiKeyAsync(apiKey, default);

        Assert.Equal(OpenAiCredentialWriteStatus.Failed, result);
        Assert.Empty(runner.Invocations);
    }

    [Fact]
    public async Task RejectsExcessiveSaveAndLoadValues()
    {
        FakeKeychainProcessRunner runner = new();
        MacOsOpenAiCredentialStore store = new(runner);
        string excessive = new('k', OpenAiCredentialPolicy.MaximumApiKeyLength + 1);

        Assert.Equal(
            OpenAiCredentialWriteStatus.Failed,
            await store.SaveApiKeyAsync(excessive, default));
        runner.Set(
            MacOsOpenAiCredentialStore.ServiceName,
            MacOsOpenAiCredentialStore.AccountName,
            excessive);
        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
    }

    [Fact]
    public async Task RejectsMalformedNativeOutput()
    {
        FakeKeychainProcessRunner runner = new()
        {
            ForcedResult = MacOsKeychainProcessResult.Succeeded(new byte[] { 0xc3, 0x28 }),
        };
        MacOsOpenAiCredentialStore store = new(runner);

        OpenAiCredentialLoadResult result = await store.LoadApiKeyAsync(default);

        Assert.Equal(OpenAiCredentialLoadStatus.Failed, result.Status);
        Assert.Null(result.ApiKey);
    }

    [Fact]
    public async Task PropagatesCancellationBeforeAnyNativeOperation()
    {
        FakeKeychainProcessRunner runner = new();
        MacOsOpenAiCredentialStore store = new(runner);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.SaveApiKeyAsync("secret", cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.LoadApiKeyAsync(cancellation.Token).AsTask());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.DeleteApiKeyAsync(cancellation.Token).AsTask());
        Assert.Empty(runner.Invocations);
    }

    [Fact]
    public void RejectsNullDependenciesAndApiKeys()
    {
        Assert.Throws<ArgumentNullException>(() => new MacOsOpenAiCredentialStore(null!));

        MacOsOpenAiCredentialStore store = new(new FakeKeychainProcessRunner());
        Assert.Throws<ArgumentNullException>(
            () => store.SaveApiKeyAsync(null!, default).AsTask().GetAwaiter().GetResult());
    }

    private static void AssertSafeInvocation(FakeInvocation invocation)
    {
        Assert.Equal("/usr/bin/security", invocation.StartInfo.FileName);
        Assert.False(invocation.StartInfo.UseShellExecute);
        Assert.True(invocation.StartInfo.CreateNoWindow);
        Assert.Contains(MacOsOpenAiCredentialStore.ServiceName, invocation.StartInfo.ArgumentList);
        Assert.Contains(MacOsOpenAiCredentialStore.AccountName, invocation.StartInfo.ArgumentList);
        string secret = Encoding.UTF8.GetString(invocation.StandardInput);
        if (secret.Length > 0)
        {
            Assert.DoesNotContain(secret, invocation.StartInfo.ArgumentList);
            Assert.DoesNotContain(secret, invocation.StartInfo.Environment.Values);
            Assert.DoesNotContain(secret, invocation.StartInfo.Arguments, StringComparison.Ordinal);
        }
    }

    private sealed class FakeKeychainProcessRunner : IMacOsKeychainProcessRunner
    {
        private readonly Dictionary<(string Service, string Account), string> entries = [];

        public List<FakeInvocation> Invocations { get; } = [];

        public MacOsKeychainProcessResult? ForcedResult { get; set; }

        public void Set(string service, string account, string apiKey) =>
            entries[(service, account)] = apiKey;

        public string? Get(string service, string account) =>
            entries.GetValueOrDefault((service, account));

        public ValueTask<MacOsKeychainProcessResult> RunAsync(
            ProcessStartInfo startInfo,
            ReadOnlyMemory<byte> standardInput,
            int maximumStandardOutputBytes,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] copiedInput = standardInput.ToArray();
            Invocations.Add(new FakeInvocation(startInfo, copiedInput));
            if (ForcedResult is not null)
            {
                return ValueTask.FromResult(ForcedResult);
            }

            string operation = startInfo.ArgumentList[0];
            string account = ValueAfter(startInfo, "-a");
            string service = ValueAfter(startInfo, "-s");
            (string Service, string Account) key = (service, account);
            return operation switch
            {
                "add-generic-password" => Save(key, copiedInput),
                "find-generic-password" => Load(key, maximumStandardOutputBytes),
                "delete-generic-password" => Delete(key),
                _ => throw new InvalidOperationException("Unexpected fake command."),
            };
        }

        private ValueTask<MacOsKeychainProcessResult> Save(
            (string Service, string Account) key,
            byte[] standardInput)
        {
            entries[key] = Encoding.UTF8.GetString(standardInput);
            return ValueTask.FromResult(
                MacOsKeychainProcessResult.Succeeded(ReadOnlyMemory<byte>.Empty));
        }

        private ValueTask<MacOsKeychainProcessResult> Load(
            (string Service, string Account) key,
            int maximumOutputBytes)
        {
            if (!entries.TryGetValue(key, out string? value))
            {
                return ValueTask.FromResult(MacOsKeychainProcessResult.Missing());
            }

            byte[] output = Encoding.UTF8.GetBytes(value + "\n");
            return ValueTask.FromResult(output.Length <= maximumOutputBytes
                ? MacOsKeychainProcessResult.Succeeded(output)
                : MacOsKeychainProcessResult.Failed());
        }

        private ValueTask<MacOsKeychainProcessResult> Delete(
            (string Service, string Account) key) =>
            ValueTask.FromResult(entries.Remove(key)
                ? MacOsKeychainProcessResult.Succeeded(ReadOnlyMemory<byte>.Empty)
                : MacOsKeychainProcessResult.Missing());

        private static string ValueAfter(ProcessStartInfo startInfo, string argument)
        {
            int index = startInfo.ArgumentList.IndexOf(argument);
            return startInfo.ArgumentList[index + 1];
        }
    }

    private sealed record FakeInvocation(ProcessStartInfo StartInfo, byte[] StandardInput);
}
