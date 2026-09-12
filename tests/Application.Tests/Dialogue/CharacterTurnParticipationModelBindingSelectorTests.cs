using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnParticipationModelBindingSelectorTests
{
    [Fact]
    public void SelectsTheExactBindingForEachCanonicalProviderWithoutInvokingModels()
    {
        CharacterTurnParticipationModelBinding openAi = Binding(
            CharacterModelProvider.OpenAi, "gpt-test");
        CharacterTurnParticipationModelBinding anthropic = Binding(
            CharacterModelProvider.Anthropic, "claude-test");
        CharacterTurnParticipationModelBinding ollama = Binding(
            CharacterModelProvider.Ollama, "granite-test");
        CharacterTurnParticipationModelBindingSnapshot snapshot = new([anthropic, ollama, openAi]);

        Assert.Same(openAi, CharacterTurnParticipationModelBindingSelector.Select(
            CharacterModelProvider.OpenAi, snapshot));
        Assert.Same(anthropic, CharacterTurnParticipationModelBindingSelector.Select(
            CharacterModelProvider.Anthropic, snapshot));
        Assert.Same(ollama, CharacterTurnParticipationModelBindingSelector.Select(
            CharacterModelProvider.Ollama, snapshot));
        Assert.All(snapshot.Bindings, binding => Assert.Equal(0, Model(binding).CallCount));
    }

    [Fact]
    public void MissingProviderIsUnavailableWithoutSubstitution()
    {
        CharacterTurnParticipationModelBinding anthropic = Binding(
            CharacterModelProvider.Anthropic, "claude-test");
        CharacterTurnParticipationModelBindingSnapshot snapshot = new([anthropic]);

        CharacterTurnParticipationModelBinding? selected =
            CharacterTurnParticipationModelBindingSelector.Select(
                CharacterModelProvider.OpenAi,
                snapshot);

        Assert.Null(selected);
        Assert.Same(anthropic, snapshot.Bindings[0]);
        Assert.Equal(0, Model(anthropic).CallCount);
    }

    [Fact]
    public void RejectsNullInputsAndAValueEqualButNonCanonicalProvider()
    {
        CharacterTurnParticipationModelBinding binding = Binding(
            CharacterModelProvider.OpenAi, "gpt-test");
        CharacterTurnParticipationModelBindingSnapshot snapshot = new([binding]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationModelBindingSelector.Select(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnParticipationModelBindingSelector.Select(
                CharacterModelProvider.OpenAi, null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterTurnParticipationModelBindingSelector.Select(
                CreateNonCanonicalProvider(CharacterModelProvider.OpenAi.Value), snapshot));
        Assert.Equal(0, Model(binding).CallCount);
    }

    private static CharacterTurnParticipationModelBinding Binding(
        CharacterModelProvider provider,
        string modelName) =>
        CharacterTurnParticipationModelBindingSnapshotTests.Binding(provider, modelName);

    private static CharacterTurnParticipationModelBindingTests.RecordingParticipationModel Model(
        CharacterTurnParticipationModelBinding binding) =>
        CharacterTurnParticipationModelBindingSnapshotTests.Model(binding);

    private static CharacterModelProvider CreateNonCanonicalProvider(string value)
    {
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        return (CharacterModelProvider)constructor.Invoke([value]);
    }
}
