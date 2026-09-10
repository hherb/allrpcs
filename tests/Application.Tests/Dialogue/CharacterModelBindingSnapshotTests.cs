using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelBindingSnapshotTests
{
    [Fact]
    public void CopiesAndRetainsExactBindingsInStableOrder()
    {
        CharacterModelBinding openAi = Binding(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelBinding anthropic = Binding(CharacterModelProvider.Anthropic, "claude-test");
        CharacterModelBinding[] source = [openAi, anthropic];

        CharacterModelBindingSnapshot snapshot = new(source);
        source[0] = anthropic;

        Assert.Collection(
            snapshot.Bindings,
            binding => Assert.Same(openAi, binding),
            binding => Assert.Same(anthropic, binding));
    }

    [Fact]
    public void AcceptsTheThreeCanonicalProvidersAtTheExactBoundary()
    {
        CharacterModelBindingSnapshot snapshot = new(
            [
                Binding(CharacterModelProvider.OpenAi, "gpt-test"),
                Binding(CharacterModelProvider.Anthropic, "claude-test"),
                Binding(CharacterModelProvider.Ollama, "granite-test"),
            ]);

        Assert.Equal(CharacterModelBindingSnapshot.MaximumBindingCount, snapshot.Bindings.Count);
    }

    [Fact]
    public void RejectsNullEmptyExcessiveAndDuplicateBindings()
    {
        CharacterModelBinding openAi = Binding(CharacterModelProvider.OpenAi, "gpt-test");

        Assert.Throws<ArgumentNullException>(() => new CharacterModelBindingSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot([]));
        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot(
            [openAi, Binding(CharacterModelProvider.OpenAi, "other-gpt-test")]));
        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot(
            [
                openAi,
                Binding(CharacterModelProvider.Anthropic, "claude-test"),
                Binding(CharacterModelProvider.Ollama, "granite-test"),
                Binding(CharacterModelProvider.OpenAi, "other-gpt-test"),
            ]));
    }

    [Fact]
    public void RejectsNonCanonicalAndSettingsMismatchedProviders()
    {
        CharacterModelProvider nonCanonical = CreateNonCanonicalProvider("other");
        CharacterModelBinding unknown = Binding(nonCanonical, "model-test");
        CharacterModelBinding mismatched = new(
            CharacterModelProvider.Anthropic,
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "gpt-test"),
            new CharacterModelBindingTests.StubCharacterModel(),
            new CharacterModelBindingTests.StubConnectionTester());

        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot([unknown]));
        Assert.Throws<ArgumentException>(() => new CharacterModelBindingSnapshot([mismatched]));
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
