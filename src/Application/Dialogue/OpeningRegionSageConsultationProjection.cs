using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>Projects fixed sage guidance from exact heard stabilized-patient testimony.</summary>
public static class OpeningRegionSageConsultationProjection
{
    private const string SageCharacterId = "village-sage";

    /// <summary>Gets the fixed authored consultation text.</summary>
    public const string ConsultationText =
        "Keep the patient's account beside the stream, shrine, and wagon evidence. More than one "
        + "route can support the remedy, so a lost clue need not end the investigation.";

    /// <summary>Returns one ordinary sage delivery without invoking a provider.</summary>
    public static DialogueDelivery Project(
        Character sage,
        FirstPatientTreatmentContext context,
        DialogueEvent patientTestimony,
        CharacterId seekerId)
    {
        ArgumentNullException.ThrowIfNull(sage);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(patientTestimony);
        ArgumentNullException.ThrowIfNull(seekerId);

        if (sage.Identity.Id.Value != SageCharacterId
            || !ReferenceEquals(
                patientTestimony.Utterance.SpeakerCharacterId,
                context.Scenario.PatientId)
            || !ReferenceEquals(
                patientTestimony.Delivery.Mode,
                DialogueDeliveryMode.OrdinarySpeech)
            || patientTestimony.Utterance.Text
                != FirstPatientStabilizationTestimonyProjection.TestimonyText
            || !patientTestimony.Audience.ListenerCharacterIds.Any(
                listenerId => ReferenceEquals(listenerId, seekerId)))
        {
            throw new InvalidOperationException(
                "Sage consultation requires exact heard stabilized-patient testimony.");
        }

        return new DialogueDelivery(
            new DialogueUtterance(sage.Identity.Id, ConsultationText),
            DialogueDeliveryMode.OrdinarySpeech);
    }
}
