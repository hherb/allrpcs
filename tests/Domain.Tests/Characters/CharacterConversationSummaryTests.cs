using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterConversationSummaryTests
{
    [Fact]
    public void RetainsExactInputsAndCopiesSourceTurnsInStableOrder()
    {
        CharacterConversationSummaryId summaryId = new("moonwort-talk");
        CharacterId characterId = new("healer");
        CharacterTurnId firstTurnId = new("healer-turn-1");
        CharacterTurnId secondTurnId = new("healer-turn-2");
        CharacterTurnId[] sourceTurnIds = [firstTurnId, secondTurnId];
        const string Text = "  We discussed examining moonwort before collecting it.\r\n";

        CharacterConversationSummary summary = new(
            summaryId,
            characterId,
            sourceTurnIds,
            Text);
        sourceTurnIds[0] = secondTurnId;

        Assert.Same(summaryId, summary.SummaryId);
        Assert.Same(characterId, summary.CharacterId);
        Assert.Collection(
            summary.SourceTurnIds,
            turnId => Assert.Same(firstTurnId, turnId),
            turnId => Assert.Same(secondTurnId, turnId));
        Assert.Equal(Text, summary.Text);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterTurnId>)summary.SourceTurnIds).Add(firstTurnId));
    }

    [Fact]
    public void AcceptsExactSourceTurnAndTextBounds()
    {
        CharacterTurnId[] maximumTurns = Enumerable
            .Range(0, CharacterConversationSummary.MaximumSourceTurnCount)
            .Select(index => new CharacterTurnId($"turn-{index}"))
            .ToArray();
        string maximumText = new('s', CharacterConversationSummary.MaximumTextLength);

        CharacterConversationSummary summary = Summary(maximumTurns, maximumText);

        Assert.Equal(CharacterConversationSummary.MaximumSourceTurnCount, summary.SourceTurnIds.Count);
        Assert.Equal(CharacterConversationSummary.MaximumTextLength, summary.Text.Length);
    }

    [Fact]
    public void RejectsInvalidSourceTurnsAndText()
    {
        CharacterTurnId first = new("turn-one");
        CharacterTurnId[] tooMany = Enumerable
            .Range(0, CharacterConversationSummary.MaximumSourceTurnCount + 1)
            .Select(index => new CharacterTurnId($"turn-{index}"))
            .ToArray();

        Assert.Throws<ArgumentException>(() => Summary([], "Summary."));
        Assert.Throws<ArgumentException>(() => Summary([first, new CharacterTurnId("turn-one")], "Summary."));
        Assert.Throws<ArgumentException>(() => Summary([null!], "Summary."));
        Assert.Throws<ArgumentOutOfRangeException>(() => Summary(tooMany, "Summary."));
        Assert.Throws<ArgumentException>(() => Summary([first], " "));
        Assert.Throws<ArgumentException>(() => Summary(
            [first],
            new string('s', CharacterConversationSummary.MaximumTextLength + 1)));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterConversationSummaryId summaryId = new("moonwort-talk");
        CharacterId characterId = new("healer");
        CharacterTurnId turnId = new("turn-one");

        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummary(
            null!, characterId, [turnId], "Summary."));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummary(
            summaryId, null!, [turnId], "Summary."));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummary(
            summaryId, characterId, null!, "Summary."));
        Assert.Throws<ArgumentNullException>(() => new CharacterConversationSummary(
            summaryId, characterId, [turnId], null!));
    }

    internal static CharacterConversationSummary Summary(
        IEnumerable<CharacterTurnId>? sourceTurnIds = null,
        string text = "Earlier conversation summary.",
        CharacterId? characterId = null,
        string summaryId = "summary") =>
        new(
            new CharacterConversationSummaryId(summaryId),
            characterId ?? new CharacterId("healer"),
            sourceTurnIds ?? [new CharacterTurnId("turn-one")],
            text);
}
