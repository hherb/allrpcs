using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationContinuationPolicyTests
{
    [Theory]
    [InlineData(0, 1, false, true)]
    [InlineData(15, 16, false, true)]
    [InlineData(1, 1, false, false)]
    [InlineData(0, 1, true, false)]
    [InlineData(1, 1, true, false)]
    public void AllowsAnotherTurnOnlyBelowTheMaximumAndBeforeInterruption(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted,
        bool expected)
    {
        AutonomousConversationBudget budget = new(
            completedTurnCount,
            maximumTurnCount,
            hasPlayerInterrupted);

        bool result = AutonomousConversationContinuationPolicy.CanContinue(budget);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePurePolicyOperation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationContinuationPolicy.CanContinue(null!));

        Type policyType = typeof(AutonomousConversationContinuationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanContinue", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(AutonomousConversationBudget)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
