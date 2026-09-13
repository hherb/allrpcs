using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskStartTransitionTests
{
    [Fact]
    public void StartsFirstEligibleExactActivityAtStablePosition()
    {
        CharacterIndependentTaskActivity other =
            CharacterIndependentTaskSelectionPolicyTests.Activity("other", "sage", "assigned");
        CharacterIndependentTaskActivity selected =
            CharacterIndependentTaskSelectionPolicyTests.Activity("first", "healer", "assigned");
        CharacterIndependentTaskActivity later =
            CharacterIndependentTaskSelectionPolicyTests.Activity("later", "healer", "assigned");
        CharacterIndependentTaskActivitySnapshot snapshot = new([other, selected, later]);

        CharacterIndependentTaskActivitySnapshot result =
            CharacterIndependentTaskStartTransition.StartNext(
                new CharacterId("healer"),
                snapshot);

        Assert.Collection(
            result.Activities,
            activity => Assert.Same(other, activity),
            activity =>
            {
                Assert.NotSame(selected, activity);
                Assert.Same(selected.Assignment, activity.Assignment);
                Assert.Same(CharacterIndependentTaskActivityState.Active, activity.State);
            },
            activity => Assert.Same(later, activity));
        Assert.Same(selected, snapshot.Activities[1]);
    }

    [Theory]
    [InlineData("active")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RejectsWhenCharacterHasNoEligibleAssignedTask(string stateValue)
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskSelectionPolicyTests.Activity("task", "healer", stateValue);
        CharacterIndependentTaskActivitySnapshot snapshot = new([activity]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskStartTransition.StartNext(
                new CharacterId("healer"),
                snapshot));
        Assert.Same(activity, snapshot.Activities[0]);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureTransition()
    {
        CharacterIndependentTaskActivitySnapshot snapshot = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskStartTransition.StartNext(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskStartTransition.StartNext(
                new CharacterId("healer"),
                null!));

        Type transitionType = typeof(CharacterIndependentTaskStartTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("StartNext", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskActivitySnapshot), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
