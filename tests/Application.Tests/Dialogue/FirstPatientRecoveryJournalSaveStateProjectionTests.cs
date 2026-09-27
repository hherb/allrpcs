using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class FirstPatientRecoveryJournalSaveStateProjectionTests
{
    [Fact]
    public void AppendsMissingRecoveryCopiesInOrderWhilePreservingExistingState()
    {
        JournalSaveState source = Journal();
        DialogueEvent patient = Event("patient", "I am steadier.");
        DialogueEvent sage = Event("village-sage", "Keep investigating.");

        JournalSaveState result = FirstPatientRecoveryJournalSaveStateProjection.Project(
            source,
            patient,
            sage);

        Assert.NotSame(source, result);
        Assert.Same(source.SeekerId, result.SeekerId);
        Assert.Equal(source.Observations, result.Observations);
        Assert.Equal(source.Hypotheses, result.Hypotheses);
        Assert.Equal(source.Experiments, result.Experiments);
        Assert.Equal(source.OpenQuestions, result.OpenQuestions);
        Assert.Equal(source.SelectedSectionKey, result.SelectedSectionKey);
        Assert.Collection(
            result.Testimony,
            entry => Assert.Equal("Existing testimony.", entry.Text),
            entry => Assert.Equal(patient.Utterance.Text, entry.Text),
            entry => Assert.Equal(sage.Utterance.Text, entry.Text));
    }

    [Fact]
    public void DoesNotDuplicateExistingValueCopies()
    {
        DialogueEvent patient = Event("patient", "I am steadier.");
        JournalSaveState source = new(
            Journal().SeekerId,
            [],
            [
                new JournalTestimonySaveEntry(
                    new CharacterId(patient.Utterance.SpeakerCharacterId.Value),
                    patient.Utterance.Text,
                    patient.Delivery.Mode),
            ],
            [],
            [],
            [],
            null);

        JournalSaveState result = FirstPatientRecoveryJournalSaveStateProjection.Project(
            source,
            patient,
            null);

        Assert.Single(result.Testimony);
        Assert.Same(source.Testimony[0], result.Testimony[0]);
    }

    [Fact]
    public void RejectsConsultationWithoutPatientTestimonyAndNullJournal()
    {
        DialogueEvent sage = Event("village-sage", "Keep investigating.");

        Assert.Throws<ArgumentException>(() =>
            FirstPatientRecoveryJournalSaveStateProjection.Project(Journal(), null, sage));
        Assert.Throws<ArgumentNullException>(() =>
            FirstPatientRecoveryJournalSaveStateProjection.Project(null!, null, null));
    }

    [Fact]
    public void RejectsRecoveryDialogueNotHeardByTheJournalOwner()
    {
        CharacterId speaker = new("patient");
        DialogueEvent unheard = new(
            new DialogueDelivery(
                new DialogueUtterance(speaker, "I am steadier."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speaker, [new CharacterId("someone-else")]));

        Assert.Throws<ArgumentException>(() =>
            FirstPatientRecoveryJournalSaveStateProjection.Project(Journal(), unheard, null));
    }

    private static JournalSaveState Journal() => new(
        new CharacterId("seeker"),
        [],
        [
            new JournalTestimonySaveEntry(
                new CharacterId("healer"),
                "Existing testimony.",
                DialogueDeliveryMode.Whispering),
        ],
        [],
        [],
        [],
        "testimony");

    private static DialogueEvent Event(string speakerId, string text)
    {
        CharacterId speaker = new(speakerId);
        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speaker, text),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speaker, [new CharacterId("seeker")]));
    }
}
