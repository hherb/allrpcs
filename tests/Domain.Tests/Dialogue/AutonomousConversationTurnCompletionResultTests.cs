using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationTurnCompletionResultTests
{
    [Fact]
    public void RetainsTheExactCompletedActivityAndAdvancedBudget()
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Completed);
        AutonomousConversationBudget budget = new(1, 2, false);

        AutonomousConversationTurnCompletionResult result = new(activity, budget);

        Assert.Same(activity, result.Activity);
        Assert.Same(budget, result.Budget);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableResultSurface()
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Completed);
        AutonomousConversationBudget budget = new(1, 2, false);

        Assert.Throws<ArgumentNullException>(() =>
            new AutonomousConversationTurnCompletionResult(null!, budget));
        Assert.Throws<ArgumentNullException>(() =>
            new AutonomousConversationTurnCompletionResult(activity, null!));

        Type resultType = typeof(AutonomousConversationTurnCompletionResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        System.Reflection.PropertyInfo[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [typeof(CharacterTurnActivity), typeof(AutonomousConversationBudget)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(["Activity", "Budget"], properties.Select(property => property.Name));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.Empty(resultType.GetMethods().Where(
            method => method.DeclaringType == resultType && !method.IsSpecialName));
        Assert.True(resultType.IsSealed);
    }
}
