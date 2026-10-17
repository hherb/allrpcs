using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterEpisodicMemoryTests
{
    [Fact]
    public void RetainsExactInputsAndCopiesParticipants()
    {
        EpisodicMemoryId memoryId = new("moonwort-warning");
        CharacterId healerId = new("healer");
        CharacterId seekerId = new("seeker");
        CharacterId[] participants = [healerId, seekerId];
        LocationId locationId = new("herb-patch");
        WorldTimeSnapshot worldTime = new(new WorldEventId("herb-identified"));
        CharacterTurnId sourceTurnId = new("healer-turn-1");

        CharacterEpisodicMemory memory = new(
            memoryId,
            healerId,
            participants,
            locationId,
            worldTime,
            sourceTurnId,
            "The Seeker asked me to examine the moonwort.");
        ListIdentityContractTests.AssertIdentity(memory);
        participants[0] = new CharacterId("hermit");

        Assert.Same(memoryId, memory.MemoryId);
        Assert.Same(healerId, memory.CharacterId);
        Assert.Collection(
            memory.ParticipantCharacterIds,
            participant => Assert.Same(healerId, participant),
            participant => Assert.Same(seekerId, participant));
        Assert.Same(locationId, memory.LocationId);
        Assert.Same(worldTime, memory.WorldTime);
        Assert.Same(sourceTurnId, memory.SourceTurnId);
        Assert.Equal("The Seeker asked me to examine the moonwort.", memory.Summary);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterId>)memory.ParticipantCharacterIds).Add(new CharacterId("hermit")));
    }

    [Fact]
    public void AcceptsExactParticipantAndSummaryBounds()
    {
        CharacterId healerId = new("healer");
        CharacterId[] participants = Enumerable
            .Range(0, CharacterEpisodicMemory.MaximumParticipantCount - 1)
            .Select(index => new CharacterId($"character-{index}"))
            .Prepend(healerId)
            .ToArray();

        CharacterEpisodicMemory memory = Create(
            healerId,
            participants,
            new string('a', CharacterEpisodicMemory.MaximumSummaryLength));

        Assert.Equal(CharacterEpisodicMemory.MaximumParticipantCount, memory.ParticipantCharacterIds.Count);
        Assert.Equal(CharacterEpisodicMemory.MaximumSummaryLength, memory.Summary.Length);
    }

    [Fact]
    public void RejectsInvalidParticipantsAndSummary()
    {
        CharacterId healerId = new("healer");
        CharacterId distinctHealerId = new("healer");
        CharacterId[] tooMany = Enumerable
            .Range(0, CharacterEpisodicMemory.MaximumParticipantCount)
            .Select(index => new CharacterId($"character-{index}"))
            .Prepend(healerId)
            .ToArray();

        Assert.Throws<ArgumentException>(() => Create(healerId, [], "Memory."));
        Assert.Throws<ArgumentException>(() => Create(
            healerId,
            [healerId, distinctHealerId],
            "Memory."));
        Assert.Throws<ArgumentException>(() => Create(healerId, [healerId, null!], "Memory."));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(healerId, tooMany, "Memory."));
        Assert.Throws<ArgumentException>(() => Create(healerId, [healerId], " "));
        Assert.Throws<ArgumentException>(() => Create(
            healerId,
            [healerId],
            new string('a', CharacterEpisodicMemory.MaximumSummaryLength + 1)));
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterId healerId = new("healer");
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            null!, healerId, [healerId], new LocationId("garden"),
            new WorldTimeSnapshot(null), new CharacterTurnId("turn-one"), "Memory."));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            new EpisodicMemoryId("memory"), null!, [healerId], new LocationId("garden"),
            new WorldTimeSnapshot(null), new CharacterTurnId("turn-one"), "Memory."));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            new EpisodicMemoryId("memory"), healerId, null!, new LocationId("garden"),
            new WorldTimeSnapshot(null), new CharacterTurnId("turn-one"), "Memory."));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            new EpisodicMemoryId("memory"), healerId, [healerId], null!,
            new WorldTimeSnapshot(null), new CharacterTurnId("turn-one"), "Memory."));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            new EpisodicMemoryId("memory"), healerId, [healerId], new LocationId("garden"),
            null!, new CharacterTurnId("turn-one"), "Memory."));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemory(
            new EpisodicMemoryId("memory"), healerId, [healerId], new LocationId("garden"),
            new WorldTimeSnapshot(null), null!, "Memory."));
        Assert.Throws<ArgumentNullException>(() => Create(healerId, [healerId], null!));
    }

    internal static CharacterEpisodicMemory Create(
        CharacterId characterId,
        IEnumerable<CharacterId> participants,
        string summary,
        string memoryId = "memory") =>
        new(
            new EpisodicMemoryId(memoryId),
            characterId,
            participants,
            new LocationId("garden"),
            new WorldTimeSnapshot(new WorldEventId("journey-began")),
            new CharacterTurnId("turn-one"),
            summary);
}
