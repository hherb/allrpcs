using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class ICharacterObjectSabotageSubstitutionDecisionModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareDecisionMethod()
    {
        Type modelType = typeof(ICharacterObjectSabotageSubstitutionDecisionModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterObjectSabotageSubstitutionDecisionModel.DecideAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterObjectSabotageSubstitutionDecision>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterObjectSabotageSubstitutionConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
