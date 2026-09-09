using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelValidAffordanceContextProjectionTests
{
    [Fact]
    public void ProjectsFreshBoundedEntriesInStableSnapshotOrder()
    {
        CharacterIntentionAffordance examine = Affordance(
            CharacterIntentionType.ExamineObject,
            "herb-patch");
        CharacterIntentionAffordance collect = Affordance(
            CharacterIntentionType.CollectObject,
            "stone-marker");
        CharacterIntentionAffordanceSnapshot snapshot = new([examine, collect]);

        IReadOnlyList<CharacterModelContextEntry> first =
            CharacterModelValidAffordanceContextProjection.Project(snapshot);
        IReadOnlyList<CharacterModelContextEntry> second =
            CharacterModelValidAffordanceContextProjection.Project(snapshot);

        Assert.Collection(
            first,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.ValidAffordance, entry.Kind);
                Assert.Equal("type=examine-object;target=herb-patch", entry.Text);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.ValidAffordance, entry.Kind);
                Assert.Equal("type=collect-object;target=stone-marker", entry.Text);
            });
        Assert.NotSame(first, second);
        Assert.NotSame(first[0], second[0]);
        Assert.Same(examine, snapshot.Entries[0]);
        Assert.Same(collect, snapshot.Entries[1]);
    }

    [Fact]
    public void DistinguishesTypeAndTargetPairsWithoutDisclosingObjectState()
    {
        CharacterIntentionAffordanceSnapshot snapshot = new(
            [
                Affordance(CharacterIntentionType.ExamineObject, "herb-patch"),
                Affordance(CharacterIntentionType.CollectObject, "herb-patch"),
                Affordance(CharacterIntentionType.ExamineObject, "stone-marker"),
            ]);

        IReadOnlyList<CharacterModelContextEntry> result =
            CharacterModelValidAffordanceContextProjection.Project(snapshot);

        Assert.Equal(
            [
                "type=examine-object;target=herb-patch",
                "type=collect-object;target=herb-patch",
                "type=examine-object;target=stone-marker",
            ],
            result.Select(entry => entry.Text));
        Assert.All(result, entry =>
        {
            Assert.DoesNotContain("Herb patch", entry.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("Description", entry.Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void AcceptsEmptyAndRejectsNullWithoutChangingInput()
    {
        CharacterIntentionAffordanceSnapshot empty = new([]);

        IReadOnlyList<CharacterModelContextEntry> result =
            CharacterModelValidAffordanceContextProjection.Project(empty);

        Assert.Empty(result);
        Assert.Empty(empty.Entries);
        Assert.Throws<ArgumentNullException>(
            () => CharacterModelValidAffordanceContextProjection.Project(null!));
    }

    private static CharacterIntentionAffordance Affordance(
        CharacterIntentionType type,
        string target) =>
        new(type, new ObjectId(target));
}
