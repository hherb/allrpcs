using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>Retains exact rebuilt one-shot recovery dialogue tokens for scene restoration.</summary>
public sealed class FirstPatientRecoveryRestoration
{
    internal FirstPatientRecoveryRestoration(
        DialogueEvent? patientTestimony,
        DialogueEvent? sageConsultation)
    {
        PatientTestimony = patientTestimony;
        SageConsultation = sageConsultation;
    }

    /// <summary>Gets the rebuilt heard patient testimony, if already completed.</summary>
    public DialogueEvent? PatientTestimony { get; }

    /// <summary>Gets the rebuilt heard sage consultation, if already completed.</summary>
    public DialogueEvent? SageConsultation { get; }
}
