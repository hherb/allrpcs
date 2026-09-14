using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityResponseKindTests
{
    private static readonly string[] ExpectedValues =
    [
        "maintain-position",
        "reconsider-belief",
        "reconsider-goal",
        "seek-reconciliation",
    ];

    private static readonly string[] ExpectedPropertyNames =
    [
        "All",
        "MaintainPosition",
        "ReconsiderBelief",
        "ReconsiderGoal",
        "SeekReconciliation",
        "Value",
    ];

    [Fact]
    public void DefinesTheClosedResponseKindsInStableOrder()
    {
        Assert.Collection(
            CharacterBeliefGoalIncompatibilityResponseKind.All,
            kind => Assert.Same(
                CharacterBeliefGoalIncompatibilityResponseKind.MaintainPosition,
                kind),
            kind => Assert.Same(
                CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderBelief,
                kind),
            kind => Assert.Same(
                CharacterBeliefGoalIncompatibilityResponseKind.ReconsiderGoal,
                kind),
            kind => Assert.Same(
                CharacterBeliefGoalIncompatibilityResponseKind.SeekReconciliation,
                kind));
    }

    [Fact]
    public void CanonicalTextRoundTripsToExactDefinedInstances()
    {
        Assert.Equal(
            ExpectedValues,
            CharacterBeliefGoalIncompatibilityResponseKind.All.Select(kind => kind.Value));

        foreach (CharacterBeliefGoalIncompatibilityResponseKind kind in
                 CharacterBeliefGoalIncompatibilityResponseKind.All)
        {
            Assert.Equal(kind.Value, kind.ToString());
            Assert.Same(kind, CharacterBeliefGoalIncompatibilityResponseKind.Parse(kind.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Maintain-position")]
    [InlineData("reconsider")]
    [InlineData("reconsider_belief")]
    [InlineData("seek-reconciliation ")]
    [InlineData("change-allegiance")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() =>
            CharacterBeliefGoalIncompatibilityResponseKind.Parse(value));
    }

    [Fact]
    public void RejectsNullAndExposesOnlyTheClosedKindSurface()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityResponseKind.Parse(null!));

        string[] publicProperties = typeof(CharacterBeliefGoalIncompatibilityResponseKind)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedPropertyNames, publicProperties);
        Assert.Empty(typeof(CharacterBeliefGoalIncompatibilityResponseKind).GetConstructors());
    }
}
