using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnTranscriptEntryTests
{
    [Fact]
    public void RetainsAnExactSpokenDeliveryWithoutAPhysicalAction()
    {
        DialogueUtterance speech = new(new CharacterId("healer"), "I will examine it.");
        DialogueDelivery delivery = new(speech, DialogueDeliveryMode.Whispering);

        CharacterTurnTranscriptEntry entry = new(delivery);

        Assert.Same(CharacterTurnTranscriptEntryKind.SpokenStatement, entry.Kind);
        Assert.Same(speech.SpeakerCharacterId, entry.CharacterId);
        Assert.Same(delivery, entry.Delivery);
        Assert.Same(speech, entry.SpokenStatement);
        Assert.Null(entry.PhysicalAction);
    }

    [Fact]
    public void RetainsAnExactPhysicalActionWithoutASpokenStatement()
    {
        CharacterId characterId = new("healer");
        CharacterIntention action = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort"));

        CharacterTurnTranscriptEntry entry = new(characterId, action);

        Assert.Same(CharacterTurnTranscriptEntryKind.PhysicalAction, entry.Kind);
        Assert.Same(characterId, entry.CharacterId);
        Assert.Same(action, entry.PhysicalAction);
        Assert.Null(entry.Delivery);
        Assert.Null(entry.SpokenStatement);
    }

    [Fact]
    public void AcceptsADistinctValueEqualPhysicalActorIdentifier()
    {
        CharacterId characterId = new("healer");
        CharacterIntention action = new(
            CharacterIntentionType.CollectObject,
            new ObjectId("moonwort"));

        CharacterTurnTranscriptEntry entry = new(
            new CharacterId(characterId.Value),
            action);

        Assert.Equal(characterId, entry.CharacterId);
        Assert.NotSame(characterId, entry.CharacterId);
    }

    [Fact]
    public void RejectsWaitAsAPhysicalAction()
    {
        Assert.Throws<ArgumentException>(() => new CharacterTurnTranscriptEntry(
            new CharacterId("healer"),
            new CharacterIntention(CharacterIntentionType.Wait, null)));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterIntention action = new(
            CharacterIntentionType.ExamineObject,
            new ObjectId("moonwort"));

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnTranscriptEntry((DialogueDelivery)null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnTranscriptEntry(null!, action));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTurnTranscriptEntry(new CharacterId("healer"), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableEntrySurface()
    {
        Type entryType = typeof(CharacterTurnTranscriptEntry);
        string[] publicProperties = entryType
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["CharacterId", "Delivery", "Kind", "PhysicalAction", "SpokenStatement"],
            publicProperties);
        Assert.All(
            entryType.GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.Equal(2, entryType.GetConstructors().Length);
    }
}
