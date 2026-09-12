using System.Reflection;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterTurnParticipationModelBindingTests
{
    [Fact]
    public void RetainsTheExactProviderSettingsAndModelWithoutInvokingIt()
    {
        CharacterModelProvider provider = CharacterModelProvider.OpenAi;
        CharacterModelSettings settings = new(provider, "gpt-test");
        RecordingParticipationModel model = new();

        CharacterTurnParticipationModelBinding binding = new(provider, settings, model);

        Assert.Same(provider, binding.Provider);
        Assert.Same(settings, binding.Settings);
        Assert.Same(model, binding.Model);
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void RejectsNullComponents()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        RecordingParticipationModel model = new();

        Assert.Throws<ArgumentNullException>(() => new CharacterTurnParticipationModelBinding(
            null!, settings, model));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnParticipationModelBinding(
            CharacterModelProvider.OpenAi, null!, model));
        Assert.Throws<ArgumentNullException>(() => new CharacterTurnParticipationModelBinding(
            CharacterModelProvider.OpenAi, settings, null!));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void ExposesOnlyTheClosedImmutableBindingSurface()
    {
        Type bindingType = typeof(CharacterTurnParticipationModelBinding);
        ConstructorInfo constructor = Assert.Single(bindingType.GetConstructors());

        Assert.Equal(
            [
                typeof(CharacterModelProvider),
                typeof(CharacterModelSettings),
                typeof(ICharacterTurnParticipationModel),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Model", "Provider", "Settings"],
            bindingType.GetProperties().Select(property => property.Name).Order());
        Assert.All(bindingType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(bindingType.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly));
    }

    internal sealed class RecordingParticipationModel : ICharacterTurnParticipationModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("Binding tests must not invoke the model.");
        }
    }
}
