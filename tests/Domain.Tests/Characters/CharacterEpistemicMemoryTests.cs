using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterEpistemicMemoryTests
{
    [Fact]
    public void RetainsExactConfirmedFactInputs()
    {
        EpistemicMemoryId memoryId = new("moonwort-location");
        CharacterId characterId = new("healer");
        CharacterTurnId sourceTurnId = new("healer-turn-1");

        CharacterEpistemicMemory memory = new(
            memoryId,
            characterId,
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            sourceTurnId,
            "  Moonwort grows beside the stream.\r\n",
            null);

        Assert.Same(memoryId, memory.MemoryId);
        Assert.Same(characterId, memory.CharacterId);
        Assert.Same(CharacterEpistemicMemoryCategory.ConfirmedFact, memory.Category);
        Assert.Same(sourceTurnId, memory.SourceTurnId);
        Assert.Equal("  Moonwort grows beside the stream.\r\n", memory.Statement);
        Assert.Null(memory.AttributedSpeakerCharacterId);
    }

    [Fact]
    public void RetainsExactAttributedClaimInputs()
    {
        CharacterId speakerId = new("seeker");
        CharacterEpistemicMemory memory = Memory(
            CharacterEpistemicMemoryCategory.AttributedClaim,
            speakerId);

        Assert.Same(CharacterEpistemicMemoryCategory.AttributedClaim, memory.Category);
        Assert.Same(speakerId, memory.AttributedSpeakerCharacterId);
    }

    [Fact]
    public void RetainsExactUncertainBeliefInputs()
    {
        CharacterEpistemicMemory memory = Memory(
            CharacterEpistemicMemoryCategory.UncertainBelief,
            null);

        Assert.Same(CharacterEpistemicMemoryCategory.UncertainBelief, memory.Category);
        Assert.Null(memory.AttributedSpeakerCharacterId);
    }

    [Fact]
    public void RequiresSpeakerOnlyForAttributedClaims()
    {
        CharacterId speakerId = new("seeker");

        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.AttributedClaim,
            null));
        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            speakerId));
        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.UncertainBelief,
            speakerId));
    }

    [Fact]
    public void AcceptsExactStatementBoundAndRejectsInvalidText()
    {
        string maximum = new('e', CharacterEpistemicMemory.MaximumStatementLength);

        Assert.Equal(
            maximum,
            Memory(CharacterEpistemicMemoryCategory.ConfirmedFact, null, maximum).Statement);
        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            string.Empty));
        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            " \t\r\n"));
        Assert.Throws<ArgumentException>(() => Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            new string('e', CharacterEpistemicMemory.MaximumStatementLength + 1)));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        EpistemicMemoryId memoryId = new("moonwort-location");
        CharacterId characterId = new("healer");
        CharacterTurnId sourceTurnId = new("healer-turn-1");

        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemory(
            null!, characterId, CharacterEpistemicMemoryCategory.ConfirmedFact,
            sourceTurnId, "Statement.", null));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemory(
            memoryId, null!, CharacterEpistemicMemoryCategory.ConfirmedFact,
            sourceTurnId, "Statement.", null));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemory(
            memoryId, characterId, null!, sourceTurnId, "Statement.", null));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemory(
            memoryId, characterId, CharacterEpistemicMemoryCategory.ConfirmedFact,
            null!, "Statement.", null));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemory(
            memoryId, characterId, CharacterEpistemicMemoryCategory.ConfirmedFact,
            sourceTurnId, null!, null));
    }

    internal static CharacterEpistemicMemory Memory(
        CharacterEpistemicMemoryCategory category,
        CharacterId? speakerId,
        string statement = "Moonwort may cool a fever.",
        string memoryId = "moonwort-cooling",
        CharacterId? characterId = null) =>
        new(
            new EpistemicMemoryId(memoryId),
            characterId ?? new CharacterId("healer"),
            category,
            new CharacterTurnId("healer-turn-1"),
            statement,
            speakerId);
}
