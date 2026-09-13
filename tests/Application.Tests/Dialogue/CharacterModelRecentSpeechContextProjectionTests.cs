using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterModelRecentSpeechContextProjectionTests
{
    [Fact]
    public void ProjectsOnlyLocalSpeechInStableOrderWithExactText()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));
        string heardText = "  Can you examine this?  ";
        string spokenText = "I can examine it.";
        DialogueEvent heard = CreateEvent(
            "seeker",
            heardText,
            DialogueDeliveryMode.Whispering,
            "healer");
        DialogueEvent unrelated = CreateEvent("villager", "Keep this private.", "seeker");
        DialogueEvent spoken = CreateEvent("healer", spokenText, "seeker");
        DialogueEvent[] source = [heard, unrelated, spoken];

        CharacterModelContextEntry[] entries = CharacterModelRecentSpeechContextProjection
            .Project(character, source)
            .ToArray();

        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RecentSpeech, entry.Kind);
                Assert.Same(heardText, entry.Text);
                Assert.Same(heard.Delivery.Mode, entry.DeliveryMode);
            },
            entry =>
            {
                Assert.Equal(CharacterModelContextEntryKind.RecentSpeech, entry.Kind);
                Assert.Same(spokenText, entry.Text);
                Assert.Same(spoken.Delivery.Mode, entry.DeliveryMode);
            });
        Assert.Collection(
            source,
            entry => Assert.Same(heard, entry),
            entry => Assert.Same(unrelated, entry),
            entry => Assert.Same(spoken, entry));
    }

    [Fact]
    public void AcceptsNoRecentSpeech()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        IReadOnlyList<CharacterModelContextEntry> entries =
            CharacterModelRecentSpeechContextProjection.Project(character, []);

        Assert.Empty(entries);
    }

    [Fact]
    public void RejectsNullInputsAndEntries()
    {
        Character character = CharacterModelGoalContextProjectionTests.CreateCharacter(
            new CharacterGoalSet([]));

        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelRecentSpeechContextProjection.Project(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterModelRecentSpeechContextProjection.Project(character, null!));
        Assert.Throws<ArgumentException>(() =>
            CharacterModelRecentSpeechContextProjection.Project(character, [null!]));
    }

    internal static DialogueEvent CreateEvent(string speaker, string text, params string[] listeners) =>
        CreateEvent(speaker, text, DialogueDeliveryMode.OrdinarySpeech, listeners);

    internal static DialogueEvent CreateEvent(
        string speaker,
        string text,
        DialogueDeliveryMode mode,
        params string[] listeners)
    {
        DialogueUtterance utterance = new(new CharacterId(speaker), text);
        CharacterId[] listenerIds = listeners.Select(listener => new CharacterId(listener)).ToArray();
        return new DialogueEvent(
            new DialogueDelivery(utterance, mode),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, listenerIds));
    }
}
