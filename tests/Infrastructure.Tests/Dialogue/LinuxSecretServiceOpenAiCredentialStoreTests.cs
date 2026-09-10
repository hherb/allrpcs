using System.Diagnostics;
using System.Text;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Infrastructure.Tests.Dialogue;

public sealed class LinuxSecretServiceOpenAiCredentialStoreTests
{
    [Fact]
    public async Task SavesLoadsReplacesAndDeletesOnlyTheExactCredential()
    {
        FakeSecretServiceProcessRunner runner = new();
        runner.Set("unrelated-application", "unrelated-account", "leave-me-alone");
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);

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
        Assert.Equal("leave-me-alone", runner.Get("unrelated-application", "unrelated-account"));
        Assert.All(runner.Invocations, AssertSafeInvocation);
    }

    [Fact]
    public async Task ReportsMissingDeleteAndMapsNativeFailuresWithoutDetails()
    {
        FakeSecretServiceProcessRunner runner = new();
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);

        Assert.Equal(
            OpenAiCredentialDeleteStatus.Missing,
            await store.DeleteApiKeyAsync(default));

        runner.ForcedResult = LinuxSecretServiceProcessResult.Failed();
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

    [Fact]
    public async Task FailsClosedWithoutInvokingAnUnavailableFacility()
    {
        FakeSecretServiceProcessRunner runner = new() { IsAvailable = false };
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);

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
        Assert.Empty(runner.Invocations);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" key with spaces ")]
    [InlineData("key\0value")]
    public async Task RejectsInvalidKeysBeforeCallingNativeBoundary(string apiKey)
    {
        FakeSecretServiceProcessRunner runner = new();
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);

        OpenAiCredentialWriteStatus result = await store.SaveApiKeyAsync(apiKey, default);

        Assert.Equal(OpenAiCredentialWriteStatus.Failed, result);
        Assert.Empty(runner.Invocations);
    }

    [Fact]
    public async Task RejectsExcessiveSaveAndLoadValuesAndClearsNativeOutput()
    {
        FakeSecretServiceProcessRunner runner = new();
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);
        string excessive = new('k', OpenAiCredentialPolicy.MaximumApiKeyLength + 1);

        Assert.Equal(
            OpenAiCredentialWriteStatus.Failed,
            await store.SaveApiKeyAsync(excessive, default));
        LinuxSecretServiceProcessResult nativeResult =
            LinuxSecretServiceProcessResult.Succeeded(Encoding.UTF8.GetBytes(excessive));
        runner.ForcedResult = nativeResult;

        Assert.Equal(
            OpenAiCredentialLoadStatus.Failed,
            (await store.LoadApiKeyAsync(default)).Status);
        Assert.All(nativeResult.StandardOutput.ToArray(), value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task RejectsMalformedNativeOutputAndClearsIt()
    {
        LinuxSecretServiceProcessResult nativeResult =
            LinuxSecretServiceProcessResult.Succeeded(new byte[] { 0xc3, 0x28 });
        FakeSecretServiceProcessRunner runner = new() { ForcedResult = nativeResult };
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);

        OpenAiCredentialLoadResult result = await store.LoadApiKeyAsync(default);

        Assert.Equal(OpenAiCredentialLoadStatus.Failed, result.Status);
        Assert.Null(result.ApiKey);
        Assert.All(nativeResult.StandardOutput.ToArray(), value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task PropagatesCancellationBeforeAvailabilityOrNativeOperations()
    {
        FakeSecretServiceProcessRunner runner = new() { IsAvailable = false };
        LinuxSecretServiceOpenAiCredentialStore store = new(runner);
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
        Assert.Throws<ArgumentNullException>(() => new LinuxSecretServiceOpenAiCredentialStore(null!));

        LinuxSecretServiceOpenAiCredentialStore store = new(new FakeSecretServiceProcessRunner());
        Assert.Throws<ArgumentNullException>(
            () => store.SaveApiKeyAsync(null!, default).AsTask().GetAwaiter().GetResult());
    }

    private static void AssertSafeInvocation(FakeInvocation invocation)
    {
        Assert.Equal(
            LinuxSecretServiceOpenAiCredentialStore.SecretToolCommandPath,
            invocation.StartInfo.FileName);
        Assert.False(invocation.StartInfo.UseShellExecute);
        Assert.True(invocation.StartInfo.CreateNoWindow);
        Assert.Contains(
            LinuxSecretServiceOpenAiCredentialStore.ApplicationName,
            invocation.StartInfo.ArgumentList);
        Assert.Contains(
            LinuxSecretServiceOpenAiCredentialStore.AccountName,
            invocation.StartInfo.ArgumentList);
        string secret = Encoding.UTF8.GetString(invocation.StandardInput);
        if (secret.Length > 0)
        {
            Assert.DoesNotContain(secret, invocation.StartInfo.ArgumentList);
            Assert.DoesNotContain(secret, invocation.StartInfo.Environment.Values);
            Assert.DoesNotContain(secret, invocation.StartInfo.Arguments, StringComparison.Ordinal);
        }
    }

    private sealed class FakeSecretServiceProcessRunner : ILinuxSecretServiceProcessRunner
    {
        private readonly Dictionary<(string Application, string Account), string> entries = [];

        public bool IsAvailable { get; set; } = true;

        public List<FakeInvocation> Invocations { get; } = [];

        public LinuxSecretServiceProcessResult? ForcedResult { get; set; }

        public void Set(string application, string account, string apiKey) =>
            entries[(application, account)] = apiKey;

        public string? Get(string application, string account) =>
            entries.GetValueOrDefault((application, account));

        public ValueTask<LinuxSecretServiceProcessResult> RunAsync(
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
            (string Application, string Account) key = (
                ValueAfter(startInfo, "application"),
                ValueAfter(startInfo, "account"));
            return operation switch
            {
                "store" => Save(key, copiedInput),
                "lookup" => Load(key, maximumStandardOutputBytes),
                "clear" => Delete(key),
                _ => throw new InvalidOperationException("Unexpected fake command."),
            };
        }

        private ValueTask<LinuxSecretServiceProcessResult> Save(
            (string Application, string Account) key,
            byte[] standardInput)
        {
            entries[key] = Encoding.UTF8.GetString(standardInput);
            return ValueTask.FromResult(
                LinuxSecretServiceProcessResult.Succeeded(ReadOnlyMemory<byte>.Empty));
        }

        private ValueTask<LinuxSecretServiceProcessResult> Load(
            (string Application, string Account) key,
            int maximumOutputBytes)
        {
            if (!entries.TryGetValue(key, out string? value))
            {
                return ValueTask.FromResult(LinuxSecretServiceProcessResult.Missing());
            }

            byte[] output = Encoding.UTF8.GetBytes(value);
            return ValueTask.FromResult(output.Length <= maximumOutputBytes
                ? LinuxSecretServiceProcessResult.Succeeded(output)
                : LinuxSecretServiceProcessResult.Failed());
        }

        private ValueTask<LinuxSecretServiceProcessResult> Delete(
            (string Application, string Account) key)
        {
            entries.Remove(key);
            return ValueTask.FromResult(
                LinuxSecretServiceProcessResult.Succeeded(ReadOnlyMemory<byte>.Empty));
        }

        private static string ValueAfter(ProcessStartInfo startInfo, string argument)
        {
            int index = startInfo.ArgumentList.IndexOf(argument);
            return startInfo.ArgumentList[index + 1];
        }
    }

    private sealed record FakeInvocation(ProcessStartInfo StartInfo, byte[] StandardInput);
}
