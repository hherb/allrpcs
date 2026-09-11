using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Requests one character's bounded observation of another character.</summary>
public sealed record PatientAssessmentRequest
{
    /// <summary>Creates an assessment request without granting assessment authority.</summary>
    public PatientAssessmentRequest(
        CharacterId assessorCharacterId,
        CharacterId patientCharacterId)
    {
        ArgumentNullException.ThrowIfNull(assessorCharacterId);
        ArgumentNullException.ThrowIfNull(patientCharacterId);

        AssessorCharacterId = assessorCharacterId;
        PatientCharacterId = patientCharacterId;
    }

    /// <summary>Gets the exact identity of the character requesting the assessment.</summary>
    public CharacterId AssessorCharacterId { get; }

    /// <summary>Gets the exact identity of the character to be assessed.</summary>
    public CharacterId PatientCharacterId { get; }
}
