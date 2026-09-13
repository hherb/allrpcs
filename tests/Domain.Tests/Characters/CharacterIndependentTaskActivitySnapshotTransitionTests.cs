using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskActivitySnapshotTransitionTests
{
    [Fact]
    public void ReplacesOnlyExactContainedActivityAtStablePosition()
    {
        CharacterIndependentTaskActivity first =
            CharacterIndependentTaskActivitySnapshotTests.Activity("first", "healer");
        CharacterIndependentTaskActivity second =
            CharacterIndependentTaskActivitySnapshotTests.Activity("second", "sage");
        CharacterIndependentTaskActivitySnapshot snapshot = new([first, second]);

        CharacterIndependentTaskActivitySnapshot result =
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                snapshot,
                first,
                CharacterIndependentTaskActivityState.Active);

        Assert.Collection(
            result.Activities,
            activity =>
            {
                Assert.NotSame(first, activity);
                Assert.Same(first.Assignment, activity.Assignment);
                Assert.Same(CharacterIndependentTaskActivityState.Active, activity.State);
            },
            activity => Assert.Same(second, activity));
        Assert.Same(CharacterIndependentTaskActivityState.Assigned, first.State);
        Assert.Same(first, snapshot.Activities[0]);
    }

    [Fact]
    public void RejectsValueEqualButDistinctActivityAndIllegalTransition()
    {
        CharacterIndependentTaskActivity contained =
            CharacterIndependentTaskActivitySnapshotTests.Activity("task", "healer");
        CharacterIndependentTaskActivity equal =
            CharacterIndependentTaskActivitySnapshotTests.Activity("task", "healer");
        CharacterIndependentTaskActivitySnapshot snapshot = new([contained]);

        Assert.Equal(contained, equal);
        Assert.NotSame(contained, equal);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                snapshot,
                equal,
                CharacterIndependentTaskActivityState.Active));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                snapshot,
                contained,
                CharacterIndependentTaskActivityState.Completed));
        Assert.Same(contained, snapshot.Activities[0]);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureTransition()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskActivitySnapshotTests.Activity("task", "healer");
        CharacterIndependentTaskActivitySnapshot snapshot = new([activity]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                null!,
                activity,
                CharacterIndependentTaskActivityState.Active));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                snapshot,
                null!,
                CharacterIndependentTaskActivityState.Active));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskActivitySnapshotTransition.Transition(
                snapshot,
                activity,
                null!));

        Type transitionType = typeof(CharacterIndependentTaskActivitySnapshotTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Transition", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskActivitySnapshot), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
