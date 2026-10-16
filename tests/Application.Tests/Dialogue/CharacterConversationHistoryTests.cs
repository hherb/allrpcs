using System.Globalization;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class CharacterConversationHistoryTests
{
    private static readonly string[] ExpectedSpeakers = ["seeker", "healer", "seeker"];

    private static DialogueEvent Speech(string speaker, string text, params string[] listeners) =>
        CharacterModelRecentSpeechContextProjectionTests.CreateEvent(speaker, text,
            DialogueDeliveryMode.Whispering, listeners);

    [Fact]
    public void KeepsExactHeardSpeechOwnRepliesAndRepeatedWordsButExcludesCurrentAndUnheardSpeech()
    {
        CharacterConversationHistory history = new();
        DialogueEvent first = Speech("seeker", "Remember blue fern.", "healer");
        DialogueEvent answer = Speech("healer", "I remember blue fern.", "seeker");
        DialogueEvent secret = Speech("sage", "Private secret.", "seeker");
        DialogueEvent repeated = Speech("seeker", first.Utterance.Text, "healer");
        DialogueEvent current = Speech("seeker", "What did I say?", "healer");
        foreach (DialogueEvent entry in new[] { first, answer, secret, repeated, current })
        {
            history.Record(entry);
        }

        var recent = history.RecentFor(new("healer"), current);
        Assert.Equal(new[] { first, answer, repeated }, recent);
        Assert.Same(first, recent[0]);
        Assert.Equal(new[] { secret }, history.RecentFor(new("sage"), current));
        var character = CharacterModelGoalContextProjectionTests.CreateCharacter(new([]));
        var projected = CharacterModelRecentSpeechContextProjection.Project(character, recent);
        Assert.Equal(ExpectedSpeakers,
            projected.Select(entry => entry.SpeakerCharacterId!.Value));
        Assert.All(projected, entry => Assert.Same(DialogueDeliveryMode.Whispering, entry.DeliveryMode));
        history.Clear();
        Assert.Empty(history.RecentFor(new("healer"), current));
        Assert.Equal(3, recent.Count); // Previously assembled requests remain immutable.
    }

    [Theory]
    [InlineData(10, 24)]
    [InlineData(2000, 6)]
    public void BoundsWorldStorageAndSelectsNewestWholeUtterances(int length, int count)
    {
        CharacterConversationHistory history = new();
        List<DialogueEvent> all = new();
        for (int i = 0; i < 300; i++)
        {
            DialogueEvent entry = Speech("seeker", i.ToString(CultureInfo.InvariantCulture).PadRight(length, 'x'), "healer");
            all.Add(entry);
            history.Record(entry);
        }

        var recent = history.RecentFor(new("healer"), Speech("seeker", "Next?", "healer"));
        Assert.Equal(count, recent.Count);
        Assert.Equal(all.TakeLast(count), recent);
        Assert.True(recent.Sum(entry => entry.Utterance.Text.Length) <= 12000);
    }

    [Fact]
    public void FiltersBeforeBudgetingAndDoesNotSpendBudgetOnUnheardSecrets()
    {
        CharacterConversationHistory history = new();
        var heard = Speech("seeker", "The herb is blue fern.", "healer");
        history.Record(heard);
        for (int i = 0; i < 25; i++) { history.Record(Speech("sage", "Secret", "seeker")); }
        Assert.Same(heard, Assert.Single(history.RecentFor(new("healer"), Speech("seeker", "Which herb?", "healer"))));
    }

    [Theory]
    [InlineData(128, 10)]
    [InlineData(32, 2000)]
    public void EvictsOldWorldHistoryAtBothStorageLimits(int newerEvents, int length)
    {
        CharacterConversationHistory history = new();
        var old = Speech("seeker", "Old healer conversation.", "healer");
        history.Record(old);
        for (int i = 0; i < newerEvents - 1; i++)
        {
            history.Record(Speech("sage", new string('x', length), "seeker"));
        }

        var current = Speech("seeker", "Now?", "healer");
        Assert.Same(old, Assert.Single(history.RecentFor(new("healer"), current)));
        history.Record(Speech("sage", new string('x', length), "seeker"));
        Assert.Empty(history.RecentFor(new("healer"), current));
    }

    [Fact]
    public void ReservesCurrentInputAndAuthoritativeContextWhenHistoryExceedsTotalBudget()
    {
        var request = CharacterModelPlayerDialogueContextProjectionTests.CreateRequest();
        var character = CharacterModelGoalContextProjectionTests.CreateCharacter(new([]));
        var memories = Enumerable.Range(0, 7)
            .Select(_ => new CharacterModelContextEntry(CharacterModelContextEntryKind.Memory, new string('m', 4000)));
        var recent = Enumerable.Range(0, 24).Select(i => Speech("seeker", i.ToString(CultureInfo.InvariantCulture).PadRight(2000, 's'), "healer"));
        var result = CharacterModelContextAssembler.Assemble(request, character,
            new CharacterPerceptionSnapshot(character.Identity.Id, []), new([]),
            new([new(new("herbal-remedies"), "Known herb.")]), memories, recent, new([]));
        Assert.True(result.Context.Entries.Sum(entry => entry.Text.Length) <= 32000);
        Assert.Equal(request.DialogueEvent.Utterance.Text, result.Context.Entries[^1].Text);
        Assert.Equal(7, result.Context.Entries.Count(entry => entry.Kind == CharacterModelContextEntryKind.Memory));
        Assert.Equal('2', Assert.Single(result.Context.Entries.Where(entry => entry.Kind == CharacterModelContextEntryKind.RecentSpeech)).Text[0]);
        Assert.StartsWith("23", Assert.Single(result.Context.Entries.Where(entry => entry.Kind == CharacterModelContextEntryKind.RecentSpeech)).Text);
    }

    [Fact]
    public void ExaminationContextRequiresExactCommittedOccurrenceAndMatchingActor()
    {
        CharacterId healer = new("healer");
        var entry = new CharacterTurnTranscriptEntry(healer, new(CharacterIntentionType.ExamineObject, new("herb-patch")));
        var occurrence = new CharacterTurnWorldEventOccurrence(new(new("examination-1"), WorldEventTimeEffect.Advance), entry);
        var result = new CharacterExaminationResult(occurrence, CharacterExaminationSafety.SafeToCollect);
        var transcript = new CharacterTurnTranscriptSnapshot([entry]);
        var projected = Assert.Single(CharacterModelExaminationContextProjection.Project(new("healer"), result, transcript));
        Assert.Equal(CharacterModelContextEntryKind.ActionOutcome, projected.Kind);
        Assert.Contains("safety=safe-to-collect", projected.Text);
        Assert.Contains("target=herb-patch", projected.Text);
        Assert.Empty(CharacterModelExaminationContextProjection.Project(new("sage"), result, transcript));
        Assert.Empty(CharacterModelExaminationContextProjection.Project(healer, result, new([])));
        Assert.Empty(CharacterModelExaminationContextProjection.Project(healer, null, transcript));
        Assert.Empty(CharacterModelExaminationContextProjection.Project(healer, result,
            new([new CharacterTurnTranscriptEntry(healer, entry.PhysicalAction!)])));
    }
}
