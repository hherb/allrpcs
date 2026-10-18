using System.Reflection;
using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterTargetSabotageDelayModelBindingTests
{
    [Fact]
    public void RetainsTheExactOpenAiSettingsAndModelWithoutInvokingIt()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        RecordingModel model = new();

        CharacterTargetSabotageDelayModelBinding binding = new(settings, model);

        Assert.Same(settings, binding.Settings);
        Assert.Same(model, binding.Model);
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void RejectsNullsAndAcceptsOtherProvidersWithoutInvokingTheModel()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        RecordingModel model = new();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayModelBinding(null!, model));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayModelBinding(settings, null!));
        Assert.Same(model, new CharacterTargetSabotageDelayModelBinding(anthropic, model).Model);
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void ExposesOnlyTheClosedImmutableBindingSurface()
    {
        Type bindingType = typeof(CharacterTargetSabotageDelayModelBinding);
        ConstructorInfo constructor = Assert.Single(bindingType.GetConstructors());

        Assert.Equal(
            [
                typeof(CharacterModelSettings),
                typeof(ICharacterTargetSabotageDelayDecisionModel),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Model", "Settings"],
            bindingType.GetProperties().Select(property => property.Name).Order());
        Assert.All(bindingType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(bindingType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly));
        Assert.True(bindingType.IsSealed);
    }

    private sealed class RecordingModel : ICharacterTargetSabotageDelayDecisionModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
            CharacterTargetSabotageDelayConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("Binding tests must not invoke the model.");
        }
    }
}
