using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Tests.Characters;

public sealed class ICharacterBeliefGoalIncompatibilityResponseModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareResponseMethod()
    {
        Type modelType = typeof(ICharacterBeliefGoalIncompatibilityResponseModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterBeliefGoalIncompatibilityResponseModel.RespondAsync), method.Name);
        Assert.Equal(typeof(ValueTask<CharacterBeliefGoalIncompatibilityResponse>), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterBeliefGoalIncompatibilityConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
