using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelBindingSelectorTests
{
    [Fact]
    public void SelectsTheExactBindingForEachCanonicalProvider()
    {
        CharacterModelBinding openAi = Binding(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelBinding anthropic = Binding(CharacterModelProvider.Anthropic, "claude-test");
        CharacterModelBinding ollama = Binding(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelBindingSnapshot snapshot = new([anthropic, ollama, openAi]);

        Assert.Same(
            openAi,
            CharacterModelBindingSelector.Select(CharacterModelProvider.OpenAi, snapshot));
        Assert.Same(
            anthropic,
            CharacterModelBindingSelector.Select(CharacterModelProvider.Anthropic, snapshot));
        Assert.Same(
            ollama,
            CharacterModelBindingSelector.Select(CharacterModelProvider.Ollama, snapshot));
    }

    [Fact]
    public void MissingProviderIsExplicitlyUnavailableWithoutSubstitution()
    {
        CharacterModelBinding anthropic = Binding(CharacterModelProvider.Anthropic, "claude-test");
        CharacterModelBindingSnapshot snapshot = new([anthropic]);

        CharacterModelBinding? selection = CharacterModelBindingSelector.Select(
            CharacterModelProvider.OpenAi,
            snapshot);

        Assert.Null(selection);
        Assert.Same(anthropic, snapshot.Bindings[0]);
        Assert.Same(CharacterModelProvider.Anthropic, anthropic.Settings.Provider);
        Assert.Equal("claude-test", anthropic.Settings.ModelName);
    }

    [Fact]
    public void RejectsNullInputsAndNonCanonicalProvider()
    {
        CharacterModelBinding binding = Binding(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelBindingSnapshot snapshot = new([binding]);

        Assert.Throws<ArgumentNullException>(() => CharacterModelBindingSelector.Select(
            null!,
            snapshot));
        Assert.Throws<ArgumentNullException>(() => CharacterModelBindingSelector.Select(
            CharacterModelProvider.OpenAi,
            null!));
        Assert.Throws<ArgumentException>(() => CharacterModelBindingSelector.Select(
            CreateNonCanonicalProvider("other"),
            snapshot));
    }

    private static CharacterModelBinding Binding(
        CharacterModelProvider provider,
        string modelName) =>
        new(
            provider,
            new CharacterModelSettings(provider, modelName),
            new CharacterModelBindingTests.StubCharacterModel(),
            new CharacterModelBindingTests.StubConnectionTester());

    private static CharacterModelProvider CreateNonCanonicalProvider(string value)
    {
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        return (CharacterModelProvider)constructor.Invoke([value]);
    }
}
