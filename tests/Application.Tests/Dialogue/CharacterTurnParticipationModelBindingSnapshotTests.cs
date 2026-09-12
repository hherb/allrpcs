using System.Reflection;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnParticipationModelBindingSnapshotTests
{
    [Fact]
    public void CopiesAndRetainsExactBindingsInStableOrder()
    {
        CharacterTurnParticipationModelBinding openAi = Binding(
            CharacterModelProvider.OpenAi, "gpt-test");
        CharacterTurnParticipationModelBinding anthropic = Binding(
            CharacterModelProvider.Anthropic, "claude-test");
        CharacterTurnParticipationModelBinding[] source = [openAi, anthropic];

        CharacterTurnParticipationModelBindingSnapshot snapshot = new(source);
        source[0] = anthropic;

        Assert.Collection(
            snapshot.Bindings,
            binding => Assert.Same(openAi, binding),
            binding => Assert.Same(anthropic, binding));
        Assert.Equal(0, Model(openAi).CallCount);
        Assert.Equal(0, Model(anthropic).CallCount);
    }

    [Fact]
    public void AcceptsTheThreeCanonicalProvidersAtTheExactBoundary()
    {
        CharacterTurnParticipationModelBindingSnapshot snapshot = new(
            [
                Binding(CharacterModelProvider.OpenAi, "gpt-test"),
                Binding(CharacterModelProvider.Anthropic, "claude-test"),
                Binding(CharacterModelProvider.Ollama, "granite-test"),
            ]);

        Assert.Equal(
            CharacterTurnParticipationModelBindingSnapshot.MaximumBindingCount,
            snapshot.Bindings.Count);
        Assert.All(snapshot.Bindings, binding => Assert.Equal(0, Model(binding).CallCount));
    }

    [Fact]
    public void RejectsNullEmptyExcessiveAndDuplicateBindings()
    {
        CharacterTurnParticipationModelBinding openAi = Binding(
            CharacterModelProvider.OpenAi, "gpt-test");

        Assert.Throws<ArgumentNullException>(
            () => new CharacterTurnParticipationModelBindingSnapshot(null!));
        Assert.Throws<ArgumentException>(
            () => new CharacterTurnParticipationModelBindingSnapshot([]));
        Assert.Throws<ArgumentException>(
            () => new CharacterTurnParticipationModelBindingSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new CharacterTurnParticipationModelBindingSnapshot(
            [openAi, Binding(CharacterModelProvider.OpenAi, "other-gpt-test")]));
        Assert.Throws<ArgumentException>(() => new CharacterTurnParticipationModelBindingSnapshot(
            [
                openAi,
                Binding(CharacterModelProvider.Anthropic, "claude-test"),
                Binding(CharacterModelProvider.Ollama, "granite-test"),
                Binding(CharacterModelProvider.OpenAi, "other-gpt-test"),
            ]));
        Assert.Equal(0, Model(openAi).CallCount);
    }

    [Fact]
    public void RejectsNonCanonicalAndSettingsMismatchedProviders()
    {
        CharacterModelProvider nonCanonical = CreateNonCanonicalProvider("other");
        CharacterTurnParticipationModelBinding unknown = Binding(nonCanonical, "model-test");
        CharacterTurnParticipationModelBinding mismatched = new(
            CharacterModelProvider.Anthropic,
            new CharacterModelSettings(CharacterModelProvider.OpenAi, "gpt-test"),
            new CharacterTurnParticipationModelBindingTests.RecordingParticipationModel());

        Assert.Throws<ArgumentException>(
            () => new CharacterTurnParticipationModelBindingSnapshot([unknown]));
        Assert.Throws<ArgumentException>(
            () => new CharacterTurnParticipationModelBindingSnapshot([mismatched]));
        Assert.Equal(0, Model(unknown).CallCount);
        Assert.Equal(0, Model(mismatched).CallCount);
    }

    [Fact]
    public void ExposesOnlyTheClosedImmutableSnapshotSurface()
    {
        Type snapshotType = typeof(CharacterTurnParticipationModelBindingSnapshot);
        ConstructorInfo constructor = Assert.Single(snapshotType.GetConstructors());

        Assert.Equal(
            [typeof(IEnumerable<CharacterTurnParticipationModelBinding>)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Collection(
            snapshotType.GetProperties(),
            property =>
            {
                Assert.Equal("Bindings", property.Name);
                Assert.False(property.CanWrite);
            });
        Assert.Empty(snapshotType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly));
    }

    internal static CharacterTurnParticipationModelBinding Binding(
        CharacterModelProvider provider,
        string modelName) =>
        new(
            provider,
            new CharacterModelSettings(provider, modelName),
            new CharacterTurnParticipationModelBindingTests.RecordingParticipationModel());

    internal static CharacterTurnParticipationModelBindingTests.RecordingParticipationModel Model(
        CharacterTurnParticipationModelBinding binding) =>
        Assert.IsType<CharacterTurnParticipationModelBindingTests.RecordingParticipationModel>(
            binding.Model);

    private static CharacterModelProvider CreateNonCanonicalProvider(string value)
    {
        ConstructorInfo constructor = Assert.Single(typeof(CharacterModelProvider).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic));
        return (CharacterModelProvider)constructor.Invoke([value]);
    }
}
