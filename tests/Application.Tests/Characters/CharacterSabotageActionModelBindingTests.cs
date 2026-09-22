using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class CharacterSabotageActionModelBindingTests
{
    [Fact]
    public void RetainsExactOpenAiSettingsAndModelWithoutInvokingIt()
    {
        CharacterModelSettings settings = new(CharacterModelProvider.OpenAi, "gpt-test");
        ThrowingModel model = new();

        CharacterSabotageActionModelBinding binding = new(settings, model);

        Assert.Same(settings, binding.Settings);
        Assert.Same(model, binding.Model);
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void RejectsNullsAndNonOpenAiSettings()
    {
        CharacterModelSettings openAi = new(CharacterModelProvider.OpenAi, "gpt-test");
        CharacterModelSettings anthropic = new(CharacterModelProvider.Anthropic, "claude-test");
        ThrowingModel model = new();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionModelBinding(null!, model));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterSabotageActionModelBinding(openAi, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterSabotageActionModelBinding(anthropic, model));
        Assert.Equal(0, model.CallCount);
    }

    [Fact]
    public void ExposesOnlyExactReadOnlyBindingState()
    {
        Type bindingType = typeof(CharacterSabotageActionModelBinding);
        System.Reflection.ConstructorInfo constructor = Assert.Single(bindingType.GetConstructors());
        System.Reflection.PropertyInfo[] properties = bindingType.GetProperties();

        Assert.Equal(
            [typeof(CharacterModelSettings), typeof(ICharacterSabotageActionDecisionModel)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            ["Model", "Settings"],
            properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        System.Reflection.MethodInfo[] methods = bindingType.GetMethods()
            .Where(method => method.DeclaringType == bindingType)
            .ToArray();
        Assert.Equal(
            ["get_Model", "get_Settings"],
            methods.Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(methods, method => Assert.True(method.IsSpecialName));
        Assert.Empty(bindingType.GetFields(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        Assert.Empty(bindingType.GetEvents());
        Assert.True(bindingType.IsSealed);
    }

    private sealed class ThrowingModel : ICharacterSabotageActionDecisionModel
    {
        internal int CallCount { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("Binding construction invoked the model.");
        }
    }
}
