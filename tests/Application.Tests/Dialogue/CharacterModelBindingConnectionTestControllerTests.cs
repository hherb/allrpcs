using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelBindingConnectionTestControllerTests
{
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
    }

    private static CharacterModelBinding Binding(
        CharacterModelProvider provider,
        string modelName,
        ICharacterModelConnectionTester tester) =>
        new(
            provider,
            new CharacterModelSettings(provider, modelName),
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
