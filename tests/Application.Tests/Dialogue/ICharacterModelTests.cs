using Allrpcs.Application.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class ICharacterModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareStreamMethod()
    {
        Type modelType = typeof(ICharacterModel);

        Assert.True(modelType.IsInterface);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());
        Assert.Equal(nameof(ICharacterModel.StreamResponseAsync), method.Name);
        Assert.Equal(
            typeof(IAsyncEnumerable<CharacterModelStreamUpdate>),
            method.ReturnType);
        Assert.Equal(
            [typeof(CharacterModelGenerationRequest), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }
}
