using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class ICharacterTurnParticipationModelTests
{
    [Fact]
    public void DefinesOneProviderNeutralCancellationAwareDecisionMethod()
    {
        Type modelType = typeof(ICharacterTurnParticipationModel);
        System.Reflection.MethodInfo method = Assert.Single(modelType.GetMethods());

        Assert.True(modelType.IsInterface);
        Assert.Equal(nameof(ICharacterTurnParticipationModel.DecideAsync), method.Name);
        Assert.Equal(
            typeof(ValueTask<CharacterTurnParticipationDecision>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTurnParticipationConsiderationRequest),
                typeof(CancellationToken),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(modelType.GetProperties());
        Assert.Empty(modelType.GetEvents());
    }
}
