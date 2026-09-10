using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsUpdateTransitionTests
{
    [Fact]
    public void ReplacesProviderSettingsAtTheirStablePosition()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-before");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings replacement = Settings(
            CharacterModelProvider.OpenAi,
            "gpt-after");
        CharacterModelSettingsSelectionSnapshot current = new(
            [anthropic, openAi],
            CharacterModelProvider.Anthropic);

        CharacterModelSettingsSelectionSnapshot updated =
            CharacterModelSettingsUpdateTransition.Update(replacement, current);

        Assert.NotSame(current, updated);
        Assert.Same(CharacterModelProvider.Anthropic, updated.SelectedProvider);
        Assert.Collection(
            updated.Settings,
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(replacement, settings));
        Assert.Same(openAi, current.Settings[1]);
    }

    [Fact]
    public void AppendsPreviouslyUnconfiguredProviderWithinTheBound()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings ollama = Settings(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [openAi, anthropic],
            CharacterModelProvider.OpenAi);

        CharacterModelSettingsSelectionSnapshot updated =
            CharacterModelSettingsUpdateTransition.Update(ollama, current);

        Assert.Same(CharacterModelProvider.OpenAi, updated.SelectedProvider);
        Assert.Collection(
            updated.Settings,
            settings => Assert.Same(openAi, settings),
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(ollama, settings));
    }

    [Fact]
    public void RetainsSnapshotWhenTheExactSettingsAreAlreadyPresent()
    {
        CharacterModelSettings settings = Settings(CharacterModelProvider.Ollama, "granite-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [settings],
            CharacterModelProvider.Ollama);

        CharacterModelSettingsSelectionSnapshot updated =
            CharacterModelSettingsUpdateTransition.Update(settings, current);

        Assert.Same(current, updated);
    }

    [Fact]
    public void RejectsNullAndNonCanonicalInputsWithoutChangingInput()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettingsSelectionSnapshot current = new(
            [openAi],
            CharacterModelProvider.OpenAi);
        CharacterModelSettings nonCanonical = Settings(CreateNonCanonicalProvider(), "other-test");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsUpdateTransition.Update(null!, current));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelSettingsUpdateTransition.Update(openAi, null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterModelSettingsUpdateTransition.Update(nonCanonical, current));
        Assert.Collection(current.Settings, settings => Assert.Same(openAi, settings));
        Assert.Same(CharacterModelProvider.OpenAi, current.SelectedProvider);
    }

    [Fact]
    public void ExposesOnlyThePureUpdateTransition()
    {
        Type transitionType = typeof(CharacterModelSettingsUpdateTransition);

        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
        Assert.Empty(transitionType.GetConstructors());
        MethodInfo method = Assert.Single(transitionType.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal("Update", method.Name);
        Assert.Equal(typeof(CharacterModelSettingsSelectionSnapshot), method.ReturnType);
        Assert.Equal(
            [typeof(CharacterModelSettings), typeof(CharacterModelSettingsSelectionSnapshot)],
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
