using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterIntentionAffordanceSnapshotTests
{
    [Fact]
    public void RetainsExactAffordancesInStableOrderAndCopiesCallerCollection()
    {
        CharacterIntentionAffordance examine = Affordance(
            CharacterIntentionType.ExamineObject,
            "herb-patch");
        CharacterIntentionAffordance collect = Affordance(
            CharacterIntentionType.CollectObject,
            "herb-patch");
        CharacterIntentionAffordance[] source = [examine, collect];

        CharacterIntentionAffordanceSnapshot snapshot = new(source);
        source[0] = collect;

        Assert.Collection(
            snapshot.Entries,
            entry => Assert.Same(examine, entry),
            entry => Assert.Same(collect, entry));
        Assert.Same(CharacterIntentionType.ExamineObject, examine.Type);
        Assert.Equal(new ObjectId("herb-patch"), examine.TargetObjectId);
    }

    [Fact]
    public void AcceptsEmptyAndDistinctTypeTargetPairs()
    {
        CharacterIntentionAffordanceSnapshot empty = new([]);
        CharacterIntentionAffordanceSnapshot distinct = new(
            [
                Affordance(CharacterIntentionType.ExamineObject, "herb-patch"),
                Affordance(CharacterIntentionType.CollectObject, "herb-patch"),
                Affordance(CharacterIntentionType.ExamineObject, "stone-marker"),
            ]);

        Assert.Empty(empty.Entries);
        Assert.Equal(3, distinct.Entries.Count);
    }

    [Fact]
    public void RejectsWaitNullsAndDuplicateValueEqualPairs()
    {
        CharacterIntentionAffordance first = Affordance(
            CharacterIntentionType.ExamineObject,
            "herb-patch");
        CharacterIntentionAffordance duplicate = Affordance(
            CharacterIntentionType.ExamineObject,
            "herb-patch");

        Assert.Throws<ArgumentException>(() => new CharacterIntentionAffordance(
            CharacterIntentionType.Wait,
            new ObjectId("herb-patch")));
        Assert.Throws<ArgumentNullException>(() => new CharacterIntentionAffordance(
            null!,
            new ObjectId("herb-patch")));
        Assert.Throws<ArgumentNullException>(() => new CharacterIntentionAffordance(
            CharacterIntentionType.ExamineObject,
            null!));
        Assert.Throws<ArgumentNullException>(() => new CharacterIntentionAffordanceSnapshot(null!));
        Assert.Throws<ArgumentException>(() => new CharacterIntentionAffordanceSnapshot([null!]));
        Assert.Throws<ArgumentException>(() => new CharacterIntentionAffordanceSnapshot(
            [first, duplicate]));
    }

    private static CharacterIntentionAffordance Affordance(
        CharacterIntentionType type,
        string target) =>
        new(type, new ObjectId(target));
}
