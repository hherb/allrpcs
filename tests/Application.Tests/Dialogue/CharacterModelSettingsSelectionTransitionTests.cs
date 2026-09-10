using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsSelectionTransitionTests
{
    [Fact]
    public void SelectsConfiguredProviderWithoutChangingSettingsOrderOrInstances()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [anthropic, openAi],
            CharacterModelProvider.OpenAi);

        CharacterModelSettingsSelectionSnapshot selected =
            CharacterModelSettingsSelectionTransition.Select(
                CharacterModelProvider.Anthropic,
                current);

        Assert.NotSame(current, selected);
        Assert.Same(CharacterModelProvider.Anthropic, selected.SelectedProvider);
        Assert.Collection(
            selected.Settings,
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(openAi, settings));
        Assert.Same(CharacterModelProvider.OpenAi, current.SelectedProvider);
    }

    [Fact]
    public void RepeatedSelectionRetainsTheExactSnapshot()
    {
        CharacterModelSettings settings = Settings(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [settings],
            CharacterModelProvider.Ollama);

        CharacterModelSettingsSelectionSnapshot selected =
            CharacterModelSettingsSelectionTransition.Select(
                CharacterModelProvider.Ollama,
                current);

        Assert.Same(current, selected);
    }

    [Fact]
    public void RejectsAbsentNonCanonicalAndNullSelection()
    {
        CharacterModelSettings settings = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [settings],
            CharacterModelProvider.OpenAi);
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        CharacterModelProvider nonCanonical = (CharacterModelProvider)constructor.Invoke(["other"]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterModelSettingsSelectionTransition.Select(
                CharacterModelProvider.Anthropic,
                current));
        Assert.Throws<ArgumentException>(() => CharacterModelSettingsSelectionTransition.Select(
            nonCanonical,
            current));
        Assert.Throws<ArgumentNullException>(() => CharacterModelSettingsSelectionTransition.Select(
            null!,
            current));
        Assert.Throws<ArgumentNullException>(() => CharacterModelSettingsSelectionTransition.Select(
            CharacterModelProvider.OpenAi,
            null!));
    }

    [Fact]
    public void ExposesOnlyThePureSelectionTransition()
    {
        Type transitionType = typeof(CharacterModelSettingsSelectionTransition);

        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
        Assert.Empty(transitionType.GetConstructors());
        MethodInfo method = Assert.Single(transitionType.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("Select", method.Name);
        Assert.Equal(typeof(CharacterModelSettingsSelectionSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterModelProvider), typeof(CharacterModelSettingsSelectionSnapshot)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static CharacterModelSettings Settings(
        CharacterModelProvider provider,
        string modelName) => new(provider, modelName);
}
