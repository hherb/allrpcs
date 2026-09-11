using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Diseases;

/// <summary>Decides whether one character may assess a currently perceived patient.</summary>
public static class PatientAssessmentEligibilityPolicy
{
    /// <summary>Gets the authored capability key that enables patient assessment.</summary>
    public const string RequiredCapabilityKey = "assess-patients";

    /// <summary>Gets the maximum capability entries inspected by one assessment decision.</summary>
    public const int MaximumCapabilityCount = 32;

    /// <summary>Returns true only for a distinct perceived patient and a capable exact assessor.</summary>
    public static bool CanAssess(
        PatientAssessmentRequest? request,
        CharacterPatientPerception? perception,
        Character? assessor)
    {
        if (request is null || perception is null || assessor is null)
        {
            return false;
        }

        return request.AssessorCharacterId != request.PatientCharacterId
            && assessor.Identity.Id == request.AssessorCharacterId
            && perception.AssessorCharacterId == request.AssessorCharacterId
            && perception.PatientCharacterId == request.PatientCharacterId
            && assessor.Capabilities.Entries.Count <= MaximumCapabilityCount
            && assessor.Capabilities.Entries.Any(capability =>
                StringComparer.Ordinal.Equals(capability.Key, RequiredCapabilityKey));
    }
}
