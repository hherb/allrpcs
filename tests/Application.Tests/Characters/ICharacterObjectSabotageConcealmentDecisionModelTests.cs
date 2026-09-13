using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class ICharacterObjectSabotageConcealmentDecisionModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareDecisionMethod()
    {
        Type modelType = typeof(ICharacterObjectSabotageConcealmentDecisionModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterObjectSabotageConcealmentDecisionModel.DecideAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterObjectSabotageConcealmentDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterObjectSabotageConcealmentConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
