using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsBindingSelectorTests
{
    [Fact]
    public void SelectsOnlyTheExactBindingForTheExactSelectedSettings()
    {
        CharacterModelSettings openAiSettings = Settings(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings anthropicSettings = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelBinding openAi = Binding(openAiSettings);
        CharacterModelBinding anthropic = Binding(anthropicSettings);
        CharacterModelSettingsSelectionSnapshot selection = new(
            [openAiSettings, anthropicSettings],
            CharacterModelProvider.Anthropic);
        CharacterModelBindingSnapshot bindings = new([openAi, anthropic]);

        CharacterModelBinding? selected = CharacterModelSettingsBindingSelector.Select(
            selection,
            bindings);

        Assert.Same(anthropic, selected);
    }

    [Fact]
    public void MissingBindingIsUnavailableWithoutSubstitution()
    {
        CharacterModelSettings openAiSettings = Settings(
            CharacterModelProvider.OpenAi,
            "gpt-test");
        CharacterModelSettings anthropicSettings = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelBinding anthropic = Binding(anthropicSettings);

        CharacterModelBinding? selected = CharacterModelSettingsBindingSelector.Select(
            new CharacterModelSettingsSelectionSnapshot(
                [openAiSettings, anthropicSettings],
                CharacterModelProvider.OpenAi),
            new CharacterModelBindingSnapshot([anthropic]));

        Assert.Null(selected);
        Assert.Same(anthropic, CharacterModelBindingSelector.Select(
            CharacterModelProvider.Anthropic,
            new CharacterModelBindingSnapshot([anthropic])));
    }

    [Fact]
    public void ValueEquivalentSettingsDoNotAuthorizeADifferentBinding()
    {
        CharacterModelSettings configured = Settings(
            CharacterModelProvider.Ollama,
            "granite-test");
        CharacterModelSettings separatelyConstructed = Settings(
            CharacterModelProvider.Ollama,
            "granite-test");
        CharacterModelBinding binding = Binding(separatelyConstructed);

        CharacterModelBinding? selected = CharacterModelSettingsBindingSelector.Select(
            new CharacterModelSettingsSelectionSnapshot(
                [configured],
                CharacterModelProvider.Ollama),
            new CharacterModelBindingSnapshot([binding]));

        Assert.Null(selected);
        Assert.NotSame(configured, binding.Settings);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterModelSettings settings = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettingsSelectionSnapshot selection = new(
            [settings],
            CharacterModelProvider.OpenAi);
        CharacterModelBindingSnapshot bindings = new([Binding(settings)]);

        Assert.Throws<ArgumentNullException>(() => CharacterModelSettingsBindingSelector.Select(
            null!,
            bindings));
        Assert.Throws<ArgumentNullException>(() => CharacterModelSettingsBindingSelector.Select(
            selection,
            null!));
    }

    private static CharacterModelSettings Settings(
        CharacterModelProvider provider,
        string modelName) => new(provider, modelName);

    private static CharacterModelBinding Binding(CharacterModelSettings settings) =>
        new(
            settings.Provider,
            settings,
            new CharacterModelBindingTests.StubCharacterModel(),
            new CharacterModelBindingTests.StubConnectionTester());
}
