using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelStreamSnapshotTests
{
    [Fact]
    public void RetainsExactUpdatesInStableOrder()
    {
        CharacterModelTextDelta first = new("Heal");
        CharacterModelTextDelta second = new("er");
        CharacterModelStreamTermination termination = Completed();
        CharacterModelStreamUpdate[] source = [first, second, termination];

        CharacterModelStreamSnapshot snapshot = new(source);
        source[0] = new CharacterModelTextDelta("Changed");

        Assert.Collection(
            snapshot.Updates,
            update => Assert.Same(first, update),
            update => Assert.Same(second, update),
            update => Assert.Same(termination, update));
    }

    [Fact]
    public void AcceptsEmptyAndNonTerminalProgressSnapshots()
    {
        CharacterModelTextDelta delta = new("In progress");

        CharacterModelStreamSnapshot empty = new([]);
        CharacterModelStreamSnapshot progress = new([delta]);

        Assert.Empty(empty.Updates);
        Assert.Same(delta, Assert.Single(progress.Updates));
    }

    [Fact]
    public void AcceptsCombinedDeltaTextAtTheUtteranceBound()
    {
        CharacterModelTextDelta first = new(new string('a', 1_000));
        CharacterModelTextDelta second = new(new string('b', 1_000));

        CharacterModelStreamSnapshot snapshot = new([first, second, Completed()]);

        Assert.Equal(3, snapshot.Updates.Count);
    }

    [Fact]
    public void RejectsCombinedDeltaTextAboveTheUtteranceBound()
    {
        CharacterModelTextDelta first = new(new string('a', DialogueUtterance.MaximumTextLength));
        CharacterModelTextDelta second = new("b");

        Assert.Throws<ArgumentException>(() => new CharacterModelStreamSnapshot([first, second]));
    }

    [Fact]
    public void RejectsUpdatesAfterATermination()
    {
        CharacterModelStreamTermination termination = Completed();

        Assert.Throws<ArgumentException>(() => new CharacterModelStreamSnapshot(
            [termination, new CharacterModelTextDelta("late")]));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamSnapshot(
            [termination, Completed()]));
    }

    [Fact]
    public void RejectsNullInputAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterModelStreamSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new CharacterModelStreamSnapshot([null!]));
    }

    [Fact]
    public void AppendReturnsAFreshSnapshotWithTheExactUpdate()
    {
        CharacterModelTextDelta first = new("first");
        CharacterModelStreamSnapshot snapshot = new([first]);
        CharacterModelTextDelta second = new(" second");

        CharacterModelStreamSnapshot result = CharacterModelStreamAppendTransition.Append(
            second,
            snapshot);

        Assert.NotSame(snapshot, result);
        Assert.Same(first, Assert.Single(snapshot.Updates));
        Assert.Collection(
            result.Updates,
            update => Assert.Same(first, update),
            update => Assert.Same(second, update));
    }

    [Fact]
    public void AppendRejectsNullInputsAndInvalidSequencesWithoutChangingTheSnapshot()
    {
        CharacterModelStreamTermination termination = Completed();
        CharacterModelStreamSnapshot snapshot = new([termination]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelStreamAppendTransition.Append(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelStreamAppendTransition.Append(termination, null!));
        Assert.Throws<ArgumentException>(() => CharacterModelStreamAppendTransition.Append(
            new CharacterModelTextDelta("late"),
            snapshot));
        Assert.Same(termination, Assert.Single(snapshot.Updates));
    }

    private static CharacterModelStreamTermination Completed() => new(
        CharacterModelStreamTerminationStatus.Completed,
        null);
}
