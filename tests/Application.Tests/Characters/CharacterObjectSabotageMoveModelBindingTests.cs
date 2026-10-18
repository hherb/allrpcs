using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterObjectSabotageMoveModelBindingTests
{
    [Fact]
    public void RetainsTheExactOpenAiSettingsAndModelWithoutInvokingIt()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        RecordingModel model = new();

        CharacterObjectSabotageMoveModelBinding binding = new(settings, model);

        Assert.Same(settings, binding.Settings);
        Assert.Same(model, binding.Model);
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void RejectsNullsAndNonOpenAiSettingsWithoutInvokingTheModel()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        RecordingModel model = new();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveModelBinding(null!, model));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageMoveModelBinding(settings, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterObjectSabotageMoveModelBinding(anthropic, model));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void ExposesOnlyTheClosedImmutableBindingSurface()
    {
        Type bindingType = typeof(CharacterObjectSabotageMoveModelBinding);
        ConstructorInfo constructor = Assert.Single(bindingType.GetConstructors());

        Assert.Equal(
            [
                typeof(CharacterModelSettings),
                typeof(ICharacterObjectSabotageMoveDecisionModel),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Model", "Settings"],
            bindingType.GetProperties().Select(property => property.Name).Order());
        Assert.All(bindingType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(bindingType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly));
        Assert.True(bindingType.IsSealed);
    }

    private sealed class RecordingModel : ICharacterObjectSabotageMoveDecisionModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
            CharacterObjectSabotageMoveConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("Binding tests must not invoke the model.");
        }
    }
}
