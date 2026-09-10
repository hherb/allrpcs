using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsRemovalTransitionTests
{
    [Fact]
    public void RemovesConfiguredUnselectedProviderInStableOrder()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings ollama = Settings(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [openAi, anthropic, ollama],
            CharacterModelProvider.Anthropic);

        CharacterModelSettingsSelectionSnapshot updated =
            CharacterModelSettingsRemovalTransition.Remove(
                CharacterModelProvider.OpenAi,
                current);

        Assert.NotSame(current, updated);
        Assert.Same(CharacterModelProvider.Anthropic, updated.SelectedProvider);
        Assert.Collection(
            updated.Settings,
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(ollama, settings));
        Assert.Collection(
            current.Settings,
            settings => Assert.Same(openAi, settings),
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(ollama, settings));
    }

    [Fact]
    public void RejectsSelectedAbsentNonCanonicalAndNullProviderWithoutChangingInput()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [openAi, anthropic],
            CharacterModelProvider.OpenAi);
        CharacterModelProvider nonCanonical = CreateNonCanonicalProvider();

        Assert.Throws<InvalidOperationException>(() =>
            CharacterModelSettingsRemovalTransition.Remove(
                CharacterModelProvider.OpenAi,
                current));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterModelSettingsRemovalTransition.Remove(
                CharacterModelProvider.Ollama,
                current));
        Assert.Throws<ArgumentException>(() =>
            CharacterModelSettingsRemovalTransition.Remove(nonCanonical, current));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsRemovalTransition.Remove(null!, current));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsRemovalTransition.Remove(
                CharacterModelProvider.Anthropic,
                null!));
        Assert.Collection(
            current.Settings,
            settings => Assert.Same(openAi, settings),
            settings => Assert.Same(anthropic, settings));
        Assert.Same(CharacterModelProvider.OpenAi, current.SelectedProvider);
    }

    [Fact]
    public void ExposesOnlyThePureRemovalTransition()
    {
        Type transitionType = typeof(CharacterModelSettingsRemovalTransition);

        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
        Assert.Empty(transitionType.GetConstructors());
        MethodInfo method = Assert.Single(transitionType.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("Remove", method.Name);
        Assert.Equal(typeof(CharacterModelSettingsSelectionSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterModelProvider), typeof(CharacterModelSettingsSelectionSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static CharacterModelSettings Settings(
        CharacterModelProvider provider,
        string modelName) => new(provider, modelName);

    private static CharacterModelProvider CreateNonCanonicalProvider()
    {
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        return (CharacterModelProvider)constructor.Invoke(["other"]);
    }
}
