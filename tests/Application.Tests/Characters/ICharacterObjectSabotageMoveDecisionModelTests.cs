using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class ICharacterObjectSabotageMoveDecisionModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareDecisionMethod()
    {
        Type modelType = typeof(ICharacterObjectSabotageMoveDecisionModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterObjectSabotageMoveDecisionModel.DecideAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterObjectSabotageMoveDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterObjectSabotageMoveConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
