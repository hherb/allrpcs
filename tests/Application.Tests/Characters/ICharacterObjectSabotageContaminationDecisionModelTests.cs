using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class ICharacterObjectSabotageContaminationDecisionModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareDecisionMethod()
    {
        Type modelType = typeof(ICharacterObjectSabotageContaminationDecisionModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterObjectSabotageContaminationDecisionModel.DecideAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterObjectSabotageContaminationDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterObjectSabotageContaminationConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
