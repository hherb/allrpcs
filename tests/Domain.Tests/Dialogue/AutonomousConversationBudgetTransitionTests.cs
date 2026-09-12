using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationBudgetTransitionTests
{
    [Fact]
    public void RecordsExactlyOneCompletedTurnWithoutChangingTheSource()
    {
        AutonomousConversationBudget original = new(0, 2, false);

        AutonomousConversationBudget result =
            AutonomousConversationBudgetTransition.RecordCompletedTurn(original);

        Assert.NotSame(original, result);
        Assert.Equal(0, original.CompletedTurnCount);
        Assert.False(original.HasPlayerInterrupted);
        Assert.Equal(1, result.CompletedTurnCount);
        Assert.Equal(2, result.MaximumTurnCount);
        Assert.False(result.HasPlayerInterrupted);
    }

    [Fact]
    public void RecordsTheFinalAllowedTurnAndRejectsFurtherCompletion()
    {
        AutonomousConversationBudget penultimate = new(15, 16, false);

        AutonomousConversationBudget exhausted =
            AutonomousConversationBudgetTransition.RecordCompletedTurn(penultimate);

        Assert.Equal(16, exhausted.CompletedTurnCount);
        Assert.False(AutonomousConversationContinuationPolicy.CanContinue(exhausted));
        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationBudgetTransition.RecordCompletedTurn(exhausted));
    }

    [Fact]
    public void InterruptionIsImmutableAndMonotonic()
    {
        AutonomousConversationBudget original = new(0, 4, false);

        AutonomousConversationBudget interrupted =
            AutonomousConversationBudgetTransition.Interrupt(original);
        AutonomousConversationBudget repeated =
            AutonomousConversationBudgetTransition.Interrupt(interrupted);

        Assert.NotSame(original, interrupted);
        Assert.False(original.HasPlayerInterrupted);
        Assert.Equal(original.CompletedTurnCount, interrupted.CompletedTurnCount);
        Assert.Equal(original.MaximumTurnCount, interrupted.MaximumTurnCount);
        Assert.True(interrupted.HasPlayerInterrupted);
        Assert.Same(interrupted, repeated);
        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationBudgetTransition.RecordCompletedTurn(interrupted));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheTwoPureTransitions()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationBudgetTransition.RecordCompletedTurn(null!));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationBudgetTransition.Interrupt(null!));

        Type transitionType = typeof(AutonomousConversationBudgetTransition);
        (string Name, Type ReturnType, Type[] Parameters)[] methods = transitionType
            .GetMethods()
            .Where(method => method.DeclaringType == transitionType)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .Select(method => (
                method.Name,
                method.ReturnType,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray()))
            .ToArray();

        Assert.Equal(2, methods.Length);
        Assert.Equal("Interrupt", methods[0].Name);
        Assert.Equal("RecordCompletedTurn", methods[1].Name);
        Assert.All(methods, method =>
        {
            Assert.Equal(typeof(AutonomousConversationBudget), method.ReturnType);
            Assert.Equal([typeof(AutonomousConversationBudget)], method.Parameters);
        });
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
