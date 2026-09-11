using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Records that one character can currently perceive another character.</summary>
public sealed record CharacterPatientPerception
{
    /// <summary>Creates a character-to-character perception input without deriving observations.</summary>
    public CharacterPatientPerception(
        CharacterId assessorCharacterId,
        CharacterId patientCharacterId)
    {
        ArgumentNullException.ThrowIfNull(assessorCharacterId);
        ArgumentNullException.ThrowIfNull(patientCharacterId);

        AssessorCharacterId = assessorCharacterId;
        PatientCharacterId = patientCharacterId;
    }

    /// <summary>Gets the exact identity of the perceiving assessor.</summary>
    public CharacterId AssessorCharacterId { get; }

    /// <summary>Gets the exact identity of the perceived patient.</summary>
    public CharacterId PatientCharacterId { get; }
}
