using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterConversationSummarySnapshotTests
{
    [Fact]
    public void PreservesExactEntriesAndCopiesStableOrder()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummary first = Summary(characterId, "first");
        CharacterConversationSummary second = Summary(characterId, "second");
        CharacterConversationSummary[] summaries = [first, second];

        CharacterConversationSummarySnapshot snapshot = new(characterId, summaries);
        summaries[0] = second;

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Collection(
            snapshot.Summaries,
            summary => Assert.Same(first, summary),
            summary => Assert.Same(second, summary));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterConversationSummary>)snapshot.Summaries).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummary[] maximum = Enumerable
            .Range(0, CharacterConversationSummarySnapshot.MaximumSummaryCount)
            .Select(index => Summary(characterId, $"summary-{index}"))
            .ToArray();

        Assert.Empty(new CharacterConversationSummarySnapshot(characterId, []).Summaries);
        Assert.Equal(
            CharacterConversationSummarySnapshot.MaximumSummaryCount,
            new CharacterConversationSummarySnapshot(characterId, maximum).Summaries.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterConversationSummarySnapshot(
            characterId,
            maximum.Append(Summary(characterId, "extra"))));
    }

    [Fact]
    public void RejectsDuplicateMismatchedAndNullEntries()
    {
        CharacterId characterId = new("healer");
        CharacterConversationSummary first = Summary(characterId, "summary");
        CharacterConversationSummary duplicate = Summary(characterId, "summary");
        CharacterConversationSummary other = Summary(new CharacterId("hermit"), "other");

        Assert.Throws<ArgumentException>(() => new CharacterConversationSummarySnapshot(
            characterId,
            [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterConversationSummarySnapshot(
            characterId,
            [other]));
        Assert.Throws<ArgumentException>(() => new CharacterConversationSummarySnapshot(
            characterId,
            [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummarySnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummarySnapshot(
            characterId,
            null!));
    }

    private static CharacterConversationSummary Summary(CharacterId characterId, string summaryId) =>
        CharacterConversationSummaryTests.Summary(characterId: characterId, summaryId: summaryId);
}
