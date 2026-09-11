using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;

namespace Allrpcs.Application.Diseases;

/// <summary>Projects eligible patient assessment into qualitative observable conditions.</summary>
public static class PatientAssessmentProjection
{
    /// <summary>Returns observable patient conditions, or null when assessment is ineligible.</summary>
    public static CharacterConditionSet? Project(
        PatientAssessmentRequest? request,
        CharacterPatientPerception? perception,
        Character? assessor,
        DiseaseTruthSnapshot diseaseTruth)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);

        if (!PatientAssessmentEligibilityPolicy.CanAssess(request, perception, assessor))
        {
            return null;
        }

        List<CharacterCondition> conditions = new();
        HashSet<string> conditionKeys = new(StringComparer.Ordinal);
        foreach (CharacterDiseaseState infection in diseaseTruth.Infections.Where(infection =>
                     infection.CharacterId == request!.PatientCharacterId))
        {
            CharacterCondition? condition = DiseaseConditionProjection.Project(
                infection,
                diseaseTruth);
            if (condition is not null && conditionKeys.Add(condition.Key))
            {
                conditions.Add(condition);
            }
        }

        return new CharacterConditionSet(conditions);
    }
}
