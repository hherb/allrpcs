using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationBudgetTests
{
    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(0, CharacterRecentConversationWindow.MaximumTurnCount, true)]
    [InlineData(16, CharacterRecentConversationWindow.MaximumTurnCount, false)]
    public void RetainsValidBoundedState(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        AutonomousConversationBudget budget = new(
            completedTurnCount,
            maximumTurnCount,
            hasPlayerInterrupted);

        Assert.Equal(completedTurnCount, budget.CompletedTurnCount);
        Assert.Equal(maximumTurnCount, budget.MaximumTurnCount);
        Assert.Equal(hasPlayerInterrupted, budget.HasPlayerInterrupted);
        Assert.Equal(
            CharacterRecentConversationWindow.MaximumTurnCount,
            AutonomousConversationBudget.MaximumAllowedTurnCount);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(2, 1)]
    [InlineData(0, 0)]
    [InlineData(0, CharacterRecentConversationWindow.MaximumTurnCount + 1)]
    public void RejectsInvalidCounts(int completedTurnCount, int maximumTurnCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AutonomousConversationBudget(
            completedTurnCount,
            maximumTurnCount,
            false));
    }

    [Fact]
    public void ExposesOnlyTheBoundedImmutableState()
    {
        Type budgetType = typeof(AutonomousConversationBudget);
        (string Name, Type Type)[] expectedProperties =
        [
            ("CompletedTurnCount", typeof(int)),
            ("HasPlayerInterrupted", typeof(bool)),
            ("MaximumTurnCount", typeof(int)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = budgetType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(budgetType.GetMethods().Where(
            method => method.DeclaringType == budgetType && !method.IsSpecialName));
        Assert.Empty(budgetType.GetEvents());
        Assert.True(budgetType.IsSealed);
    }
}
