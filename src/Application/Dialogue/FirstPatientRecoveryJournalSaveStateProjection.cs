using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Dialogue;

/// <summary>Adds exact heard recovery dialogue to one content-safe journal save state.</summary>
public static class FirstPatientRecoveryJournalSaveStateProjection
{
    /// <summary>Returns a fresh save state retaining existing entries plus missing heard recovery.</summary>
    public static JournalSaveState Project(
        JournalSaveState journal,
        DialogueEvent? patientTestimony,
        DialogueEvent? sageConsultation)
    {
        ArgumentNullException.ThrowIfNull(journal);
        if (sageConsultation is not null && patientTestimony is null)
        {
            throw new ArgumentException(
                "Sage consultation cannot be retained without patient testimony.",
                nameof(sageConsultation));
        }

        List<JournalTestimonySaveEntry> testimony = [.. journal.Testimony];
        AddDistinct(testimony, patientTestimony, journal.SeekerId);
        AddDistinct(testimony, sageConsultation, journal.SeekerId);
        return new JournalSaveState(
            journal.SeekerId,
            journal.Observations,
            testimony,
            journal.Hypotheses,
            journal.Experiments,
            journal.OpenQuestions,
            journal.SelectedSectionKey);
    }

    private static void AddDistinct(
        List<JournalTestimonySaveEntry> entries,
        DialogueEvent? candidate,
        CharacterId journalOwner)
    {
        if (candidate is null)
        {
            return;
        }

        if (!candidate.Audience.ListenerCharacterIds.Any(listener =>
                listener == journalOwner))
        {
            throw new ArgumentException(
                "Recovery dialogue must have been heard by the journal owner.",
                nameof(candidate));
        }

        if (entries.Any(entry =>
                entry.SpeakerCharacterId == candidate.Utterance.SpeakerCharacterId
                && string.Equals(entry.Text, candidate.Utterance.Text, StringComparison.Ordinal)
                && entry.DeliveryMode == candidate.Delivery.Mode))
        {
            return;
        }

        entries.Add(new JournalTestimonySaveEntry(
            candidate.Utterance.SpeakerCharacterId,
            candidate.Utterance.Text,
            candidate.Delivery.Mode));
    }
}
