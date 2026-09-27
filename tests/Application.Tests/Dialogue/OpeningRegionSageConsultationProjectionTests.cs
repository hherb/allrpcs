using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class OpeningRegionSageConsultationProjectionTests
{
    [Fact]
    public void ProjectsFixedOrdinarySageGuidanceFromExactHeardTestimony()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        CharacterId seekerId = new("seeker");
        DialogueEvent testimony = Testimony(context.Scenario.PatientId, seekerId);

        DialogueDelivery delivery = OpeningRegionSageConsultationProjection.Project(
            sage,
            context,
            testimony,
            seekerId);

        Assert.Same(sage.Identity.Id, delivery.Utterance.SpeakerCharacterId);
        Assert.Equal(OpeningRegionSageConsultationProjection.ConsultationText, delivery.Utterance.Text);
        Assert.Same(DialogueDeliveryMode.OrdinarySpeech, delivery.Mode);
    }

    [Fact]
    public void RejectsRebuiltSageUnheardOrUnrelatedTestimony()
    {
        Character sage = RepresentativeSageCharacterSource.Create();
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        CharacterId seekerId = new("seeker");
        DialogueEvent testimony = Testimony(context.Scenario.PatientId, seekerId);
        DialogueEvent unheard = Testimony(
            context.Scenario.PatientId,
            new CharacterId("someone-else"));
        DialogueEvent unrelated = new(
            new DialogueDelivery(
                new DialogueUtterance(
                    new CharacterId("first-patient"),
                    "An unrelated statement."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(new CharacterId("first-patient"), [seekerId]));

        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionSageConsultationProjection.Project(
                RepresentativeInfectedCharacterSource.Create()[0],
                context,
                testimony,
                seekerId));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionSageConsultationProjection.Project(sage, context, unheard, seekerId));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionSageConsultationProjection.Project(sage, context, unrelated, seekerId));
        Assert.Throws<InvalidOperationException>(() =>
            OpeningRegionSageConsultationProjection.Project(
                sage,
                RepresentativeFirstPatientTreatmentContextSource.Create(),
                testimony,
                seekerId));
    }

    private static DialogueEvent Testimony(CharacterId patientId, CharacterId listenerId)
    {
        DialogueDelivery delivery = new(
            new DialogueUtterance(
                patientId,
                FirstPatientStabilizationTestimonyProjection.TestimonyText),
            DialogueDeliveryMode.OrdinarySpeech);
        return new DialogueEvent(
            delivery,
            new DialogueAudienceSnapshot(patientId, [listenerId]));
    }
}
