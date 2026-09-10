using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelSettingsSelectionSnapshotTests
{
    [Fact]
    public void CopiesExactSettingsInStableOrderAndRetainsExactSelection()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = Settings(
            CharacterModelProvider.Anthropic,
            "claude-test");
        CharacterModelSettings[] source = [anthropic, openAi];

        CharacterModelSettingsSelectionSnapshot snapshot = new(
            source,
            CharacterModelProvider.OpenAi);
        source[0] = openAi;

        Assert.Collection(
            snapshot.Settings,
            settings => Assert.Same(anthropic, settings),
            settings => Assert.Same(openAi, settings));
        Assert.Same(CharacterModelProvider.OpenAi, snapshot.SelectedProvider);
    }

    [Fact]
    public void AcceptsEveryCanonicalProviderAtTheExactBoundary()
    {
        CharacterModelSettingsSelectionSnapshot snapshot = new(
            [
                Settings(CharacterModelProvider.OpenAi, "gpt-test"),
                Settings(CharacterModelProvider.Anthropic, "claude-test"),
                Settings(CharacterModelProvider.Ollama, "granite-test"),
            ],
            CharacterModelProvider.Ollama);

        Assert.Equal(
            CharacterModelSettingsSelectionSnapshot.MaximumSettingsCount,
            snapshot.Settings.Count);
        Assert.Same(CharacterModelProvider.Ollama, snapshot.Settings[2].Provider);
    }

    [Fact]
    public void RejectsNullEmptyExcessiveAndDuplicateSettings()
    {
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterModelSettingsSelectionSnapshot(null!, CharacterModelProvider.OpenAi));
        Assert.Throws<ArgumentException>(() =>
            new CharacterModelSettingsSelectionSnapshot([], CharacterModelProvider.OpenAi));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [null!],
            CharacterModelProvider.OpenAi));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [openAi, Settings(CharacterModelProvider.OpenAi, "other-gpt-test")],
            CharacterModelProvider.OpenAi));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [
                openAi,
                Settings(CharacterModelProvider.Anthropic, "claude-test"),
                Settings(CharacterModelProvider.Ollama, "granite-test"),
                Settings(CharacterModelProvider.OpenAi, "other-gpt-test"),
            ],
            CharacterModelProvider.OpenAi));
    }

    [Fact]
    public void RejectsNonCanonicalOrAbsentSelectionAndNonCanonicalSettings()
    {
        CharacterModelProvider nonCanonical = CreateNonCanonicalProvider("other");
        CharacterModelSettings openAi = Settings(CharacterModelProvider.OpenAi, "gpt-test");

        Assert.Throws<ArgumentNullException>(() => new CharacterModelSettingsSelectionSnapshot(
            [openAi],
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [openAi],
            nonCanonical));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [Settings(nonCanonical, "model-test")],
            nonCanonical));
        Assert.Throws<ArgumentException>(() => new CharacterModelSettingsSelectionSnapshot(
            [openAi],
            CharacterModelProvider.Anthropic));
    }

    [Fact]
    public void ExposesOnlyTheClosedImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterModelSettingsSelectionSnapshot);
        ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal(
            [typeof(IEnumerable<CharacterModelSettings>), typeof(CharacterModelProvider)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["SelectedProvider", "Settings"],
            snapshotType.GetProperties().Select(property => property.Name).Order());
        Assert.All(snapshotType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Equal(
            ["get_SelectedProvider", "get_Settings"],
            snapshotType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.DeclaringType == snapshotType)
                .Select(method => method.Name)
                .Order());
    }

    private static CharacterModelSettings Settings(
        CharacterModelProvider provider,
        string modelName) => new(provider, modelName);

    private static CharacterModelProvider CreateNonCanonicalProvider(string value)
    {
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        return (CharacterModelProvider)constructor.Invoke([value]);
    }
}
