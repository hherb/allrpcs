using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationTurnCompletionTransitionTests
{
    [Fact]
    public void CompletesTheExactSpeakingTurnAndRecordsOneTurnAtomically()
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Speaking);
        AutonomousConversationBudget budget = new(0, 2, false);

        AutonomousConversationTurnCompletionResult result =
            AutonomousConversationTurnCompletionTransition.Complete(budget, activity);

        Assert.NotSame(activity, result.Activity);
        Assert.Same(activity.Request, result.Activity.Request);
        Assert.Same(activity.Response, result.Activity.Response);
        Assert.Same(CharacterTurnActivityState.Completed, result.Activity.State);
        Assert.NotSame(budget, result.Budget);
        Assert.Equal(1, result.Budget.CompletedTurnCount);
        Assert.Equal(2, result.Budget.MaximumTurnCount);
        Assert.False(result.Budget.HasPlayerInterrupted);
        Assert.Same(CharacterTurnActivityState.Speaking, activity.State);
        Assert.Equal(0, budget.CompletedTurnCount);
    }

    [Fact]
    public void RecordsTheFinalAllowedTurnAndClosesTheResultingBudget()
    {
        AutonomousConversationTurnCompletionResult result =
            AutonomousConversationTurnCompletionTransition.Complete(
                new AutonomousConversationBudget(1, 2, false),
                CharacterTurnCompletionPolicyTests.CreateActivity(
                    CharacterTurnActivityState.Speaking));

        Assert.Equal(2, result.Budget.CompletedTurnCount);
        Assert.False(AutonomousConversationContinuationPolicy.CanContinue(result.Budget));
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("completed")]
    [InlineData("resolved")]
    [InlineData("cancelled")]
    public void RejectsAnIneligibleActivityWithoutAdvancingTheBudget(string stateValue)
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));
        AutonomousConversationBudget budget = new(0, 2, false);

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnCompletionTransition.Complete(budget, activity));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
        Assert.Equal(0, budget.CompletedTurnCount);
    }

    [Theory]
    [InlineData(2, 2, false)]
    [InlineData(0, 2, true)]
    public void RejectsAClosedBudgetWithoutCompletingTheActivity(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Speaking);
        AutonomousConversationBudget budget = new(
            completedTurnCount,
            maximumTurnCount,
            hasPlayerInterrupted);

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnCompletionTransition.Complete(budget, activity));
        Assert.Same(CharacterTurnActivityState.Speaking, activity.State);
        Assert.Equal(completedTurnCount, budget.CompletedTurnCount);
        Assert.Equal(hasPlayerInterrupted, budget.HasPlayerInterrupted);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureAtomicTransition()
    {
        CharacterTurnActivity activity = CharacterTurnCompletionPolicyTests.CreateActivity(
            CharacterTurnActivityState.Speaking);
        AutonomousConversationBudget budget = new(0, 2, false);

        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnCompletionTransition.Complete(null!, activity));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnCompletionTransition.Complete(budget, null!));

        Type transitionType = typeof(AutonomousConversationTurnCompletionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Complete", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(AutonomousConversationTurnCompletionResult), method.ReturnType);
        Assert.Equal(
            [typeof(AutonomousConversationBudget), typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
