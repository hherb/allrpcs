using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelBindingTests
{
    [Fact]
    public void RetainsTheExactProviderSettingsModelAndConnectionTester()
    {
        CharacterModelProvider provider = CharacterModelProvider.OpenAi;
        CharacterModelSettings settings = new(provider, "gpt-test");
        StubCharacterModel model = new();
        StubConnectionTester tester = new();

        CharacterModelBinding binding = new(provider, settings, model, tester);

        Assert.Same(provider, binding.Provider);
        Assert.Same(settings, binding.Settings);
        Assert.Same(model, binding.Model);
        Assert.Same(tester, binding.ConnectionTester);
    }

    [Fact]
    public void RejectsNullComponents()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        StubCharacterModel model = new();
        StubConnectionTester tester = new();

        Assert.Throws<ArgumentNullException>(() => new CharacterModelBinding(
            null!,
            settings,
            model,
            tester));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelBinding(
            CharacterModelProvider.OpenAi,
            null!,
            model,
            tester));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelBinding(
            CharacterModelProvider.OpenAi,
            settings,
            null!,
            tester));
        Assert.Throws<ArgumentNullException>(() => new CharacterModelBinding(
            CharacterModelProvider.OpenAi,
            settings,
            model,
            null!));
    }

    internal sealed class StubCharacterModel : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    internal sealed class StubConnectionTester : ICharacterModelConnectionTester
    {
        public ValueTask<CharacterModelConnectionTestResult> TestConnectionAsync(
            CharacterModelSettings settings,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(CharacterModelConnectionTestResult.Connected());
    }
}
