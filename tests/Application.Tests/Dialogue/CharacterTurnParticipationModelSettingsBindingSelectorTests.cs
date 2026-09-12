using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnParticipationModelSettingsBindingSelectorTests
{
    [Fact]
    public void SelectsOnlyTheExactBindingForTheExactSelectedSettings()
    {
        CharacterModelSettings openAiSettings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic, "claude-test");
        CharacterTurnParticipationModelBinding openAi = Binding(openAiSettings);
        CharacterTurnParticipationModelBinding anthropic = Binding(anthropicSettings);
        CharacterModelSettingsSelectionSnapshot selection = new(
            [openAiSettings, anthropicSettings],
            CharacterModelProvider.Anthropic);
        CharacterTurnParticipationModelBindingSnapshot bindings = new([openAi, anthropic]);

        CharacterTurnParticipationModelBinding? selected =
            CharacterTurnParticipationModelSettingsBindingSelector.Select(selection, bindings);

        Assert.Same(anthropic, selected);
        Assert.All(bindings.Bindings, binding => Assert.Equal(0, Model(binding).CallCount));
    }

    [Fact]
    public void MissingBindingIsUnavailableWithoutSubstitution()
    {
        CharacterModelSettings openAiSettings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropicSettings = new(
            CharacterModelProvider.Anthropic, "claude-test");
        CharacterTurnParticipationModelBinding anthropic = Binding(anthropicSettings);

        CharacterTurnParticipationModelBinding? selected =
            CharacterTurnParticipationModelSettingsBindingSelector.Select(
                new CharacterModelSettingsSelectionSnapshot(
                    [openAiSettings, anthropicSettings],
                    CharacterModelProvider.OpenAi),
                new CharacterTurnParticipationModelBindingSnapshot([anthropic]));

        Assert.Null(selected);
        Assert.Equal(0, Model(anthropic).CallCount);
    }

    [Fact]
    public void ValueEquivalentSettingsDoNotAuthorizeADifferentBinding()
    {
        CharacterModelSettings configured = new(
            CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettings separatelyConstructed = new(
            CharacterModelProvider.Ollama, "granite-test");
        CharacterTurnParticipationModelBinding binding = Binding(separatelyConstructed);

        CharacterTurnParticipationModelBinding? selected =
            CharacterTurnParticipationModelSettingsBindingSelector.Select(
                new CharacterModelSettingsSelectionSnapshot(
                    [configured], CharacterModelProvider.Ollama),
                new CharacterTurnParticipationModelBindingSnapshot([binding]));

        Assert.Null(selected);
        Assert.NotSame(configured, binding.Settings);
        Assert.Equal(0, Model(binding).CallCount);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettingsSelectionSnapshot selection = new(
            [settings], CharacterModelProvider.OpenAi);
        CharacterTurnParticipationModelBindingSnapshot bindings = new([Binding(settings)]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationModelSettingsBindingSelector.Select(null!, bindings));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationModelSettingsBindingSelector.Select(selection, null!));
        Assert.Equal(0, Model(bindings.Bindings[0]).CallCount);
    }

    private static CharacterTurnParticipationModelBinding Binding(
        CharacterModelSettings settings) =>
        new(
            settings.Provider,
            settings,
            new CharacterTurnParticipationModelBindingTests.RecordingParticipationModel());

    private static CharacterTurnParticipationModelBindingTests.RecordingParticipationModel Model(
        CharacterTurnParticipationModelBinding binding) =>
        CharacterTurnParticipationModelBindingSnapshotTests.Model(binding);
}
