using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelBindingConnectionTestControllerTests
{
    [Fact]
    public async Task TestsTheExactBindingAuthorizedByTheSettingsSelection()
    {
        CharacterModelConnectionTestResult expected = CharacterModelConnectionTestResult.Connected();
        CharacterModelSettings openAiSettings = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic,
            "claude-test");
        RecordingConnectionTester openAiTester = new(expected);
        RecordingConnectionTester anthropicTester = new(expected);
        CharacterModelBinding openAi = Binding(openAiSettings, openAiTester);
        CharacterModelBinding anthropic = Binding(anthropicSettings, anthropicTester);
        CharacterModelSettingsSelectionSnapshot selection = new(
            [openAiSettings, anthropicSettings],
            CharacterModelProvider.Anthropic);
        using CancellationTokenSource cancellation = new();

        CharacterModelConnectionTestResult result =
            await CharacterModelBindingConnectionTestController.TestConfiguredAsync(
                selection,
                new CharacterModelBindingSnapshot([openAi, anthropic]),
                cancellation.Token);

        Assert.Same(expected, result);
        Assert.Equal(0, openAiTester.CallCount);
        Assert.Equal(1, anthropicTester.CallCount);
        Assert.Same(anthropicSettings, anthropicTester.Settings);
        Assert.Equal(cancellation.Token, anthropicTester.CancellationToken);
    }

    [Fact]
    public async Task SettingsMismatchReturnsUnavailableWithoutTesting()
    {
        CharacterModelSettings configured = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings separatelyConstructed = new(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        RecordingConnectionTester tester = new(CharacterModelConnectionTestResult.Connected());

        CharacterModelConnectionTestResult result =
            await CharacterModelBindingConnectionTestController.TestConfiguredAsync(
                new CharacterModelSettingsSelectionSnapshot(
                    [configured],
                    CharacterModelProvider.OpenAi),
                new CharacterModelBindingSnapshot(
                    [Binding(separatelyConstructed, tester)]),
                CancellationToken.None);

        Assert.False(result.IsConnected);
        Assert.Equal(CharacterModelFailureCode.Unavailable, result.Failure?.Code);
        Assert.Equal(0, tester.CallCount);
    }

    [Fact]
    public async Task TestsTheExactSelectedBindingWithItsExactSettingsAndCancellation()
    {
        CharacterModelConnectionTestResult expected = CharacterModelConnectionTestResult.Connected();
        RecordingConnectionTester openAiTester = new(expected);
        RecordingConnectionTester anthropicTester = new(expected);
        CharacterModelBinding openAi = Binding(
            CharacterModelProvider.OpenAi,
            "gpt-test",
            openAiTester);
        CharacterModelBinding anthropic = Binding(
            CharacterModelProvider.Anthropic,
            "claude-test",
            anthropicTester);
        CharacterModelBindingSnapshot snapshot = new([openAi, anthropic]);
        using CancellationTokenSource cancellation = new();

        CharacterModelConnectionTestResult result =
            await CharacterModelBindingConnectionTestController.TestAsync(
                CharacterModelProvider.Anthropic,
                snapshot,
                cancellation.Token);

        Assert.Same(expected, result);
        Assert.Equal(0, openAiTester.CallCount);
        Assert.Equal(1, anthropicTester.CallCount);
        Assert.Same(anthropic.Settings, anthropicTester.Settings);
        Assert.Equal(cancellation.Token, anthropicTester.CancellationToken);
    }

    [Fact]
    public async Task MissingProviderReturnsRedactedUnavailableWithoutTestingAnotherBinding()
    {
        RecordingConnectionTester tester = new(CharacterModelConnectionTestResult.Connected());
        CharacterModelBinding onlyBinding = Binding(
            CharacterModelProvider.Ollama,
            "granite-test",
            tester);
        CharacterModelBindingSnapshot snapshot = new([onlyBinding]);

        CharacterModelConnectionTestResult result =
            await CharacterModelBindingConnectionTestController.TestAsync(
                CharacterModelProvider.OpenAi,
                snapshot,
                CancellationToken.None);

        Assert.False(result.IsConnected);
        Assert.Equal(CharacterModelFailureCode.Unavailable, result.Failure?.Code);
        Assert.Equal(0, tester.CallCount);
        Assert.Same(onlyBinding, snapshot.Bindings[0]);
    }

    [Fact]
    public async Task PropagatesExactFailedResultWithoutProviderDetails()
    {
        CharacterModelConnectionTestResult expected = CharacterModelConnectionTestResult.Failed(
            CharacterModelFailureCode.Authentication);
        RecordingConnectionTester tester = new(expected);
        CharacterModelBinding binding = Binding(
            CharacterModelProvider.OpenAi,
            "gpt-test",
            tester);

        CharacterModelConnectionTestResult result =
            await CharacterModelBindingConnectionTestController.TestAsync(
                CharacterModelProvider.OpenAi,
                new CharacterModelBindingSnapshot([binding]),
                CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal("Failed:Authentication", result.ToString());
    }

    [Fact]
    public async Task RejectsNullInputs()
    {
        CharacterModelBinding binding = Binding(
            CharacterModelProvider.OpenAi,
            "gpt-test",
            new RecordingConnectionTester(CharacterModelConnectionTestResult.Connected()));
        CharacterModelBindingSnapshot snapshot = new([binding]);
        CharacterModelSettingsSelectionSnapshot selection = new(
            [binding.Settings],
            CharacterModelProvider.OpenAi);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelBindingConnectionTestController.TestAsync(
                null!,
                snapshot,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelBindingConnectionTestController.TestAsync(
                CharacterModelProvider.OpenAi,
                null!,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelBindingConnectionTestController.TestConfiguredAsync(
                null!,
                snapshot,
                CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await CharacterModelBindingConnectionTestController.TestConfiguredAsync(
                selection,
                null!,
                CancellationToken.None));
    }

    private static CharacterModelBinding Binding(
        CharacterModelProvider provider,
        string modelName,
        ICharacterModelConnectionTester tester) => Binding(
            new CharacterModelSettings(provider, modelName),
            tester);

    private static CharacterModelBinding Binding(
        CharacterModelSettings settings,
        ICharacterModelConnectionTester tester) =>
        new(
            settings.Provider,
            settings,
            new CharacterModelBindingTests.StubCharacterModel(),
            tester);

    private sealed class RecordingConnectionTester(CharacterModelConnectionTestResult result)
        : ICharacterModelConnectionTester
    {
        public int CallCount { get; private set; }

        public CharacterModelSettings? Settings { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
            CharacterModelSettings settings,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Settings = settings;
            CancellationToken = cancellationToken;
            return ValueTask.FromResult(result);
        }
    }
}
