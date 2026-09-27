using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Dialogue;

/// <summary>Rebuilds recovery dialogue tokens only from closed progress and authored provenance.</summary>
public static class FirstPatientRecoveryRestorationProjection
{
    /// <summary>Returns exact canonical tokens for one durable recovery stage.</summary>
    public static FirstPatientRecoveryRestoration Restore(
        FirstPatientRecoveryStage stage,
        FirstPatientTreatmentContext context,
        DiseaseTruthSnapshot diseaseTruth,
        IngredientExperimentJournalSnapshot administrationExperiments,
        WorldEvent? administrationWorldEvent,
        WorldEventLogSnapshot worldEventLog,
        Character sage,
        CharacterId seekerId)
    {
        ArgumentNullException.ThrowIfNull(stage);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(worldEventLog);
        ArgumentNullException.ThrowIfNull(sage);
        ArgumentNullException.ThrowIfNull(seekerId);

        if (ReferenceEquals(stage, FirstPatientRecoveryStage.NotHeard))
        {
            return new FirstPatientRecoveryRestoration(null, null);
        }

        ArgumentNullException.ThrowIfNull(administrationWorldEvent);
        if (!worldEventLog.Entries.Any(eventId =>
                eventId == administrationWorldEvent.EventId))
        {
            throw new InvalidOperationException(
                "Recovery dialogue requires the authoritative administration occurrence.");
        }

        DialogueDelivery testimonyDelivery = FirstPatientStabilizationTestimonyProjection.Project(
            context,
            diseaseTruth,
            administrationExperiments,
            administrationWorldEvent);
        DialogueEvent testimony = new(
            testimonyDelivery,
            new DialogueAudienceSnapshot(
                testimonyDelivery.Utterance.SpeakerCharacterId,
                [seekerId]));
        if (ReferenceEquals(stage, FirstPatientRecoveryStage.PatientTestimonyHeard))
        {
            return new FirstPatientRecoveryRestoration(testimony, null);
        }

        if (!ReferenceEquals(stage, FirstPatientRecoveryStage.SageConsulted))
        {
            throw new ArgumentException("Recovery stage must be a canonical value.", nameof(stage));
        }

        DialogueDelivery consultationDelivery = OpeningRegionSageConsultationProjection.Project(
            sage,
            context,
            testimony,
            seekerId);
        DialogueEvent consultation = new(
            consultationDelivery,
            new DialogueAudienceSnapshot(
                consultationDelivery.Utterance.SpeakerCharacterId,
                [seekerId]));
        return new FirstPatientRecoveryRestoration(testimony, consultation);
    }
}
