using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class PatientAssessmentRequestTests
{
    [Fact]
    public void RetainsExactAssessorAndPatientIdentities()
    {
        CharacterId assessor = new("travelling-healer");
        CharacterId patient = new("patient");

        PatientAssessmentRequest request = new(assessor, patient);

        Assert.Same(assessor, request.AssessorCharacterId);
        Assert.Same(patient, request.PatientCharacterId);
    }

    [Fact]
    public void AllowsSelfAssessmentForTheEligibilityPolicyToReject()
    {
        CharacterId characterId = new("patient");

        PatientAssessmentRequest request = new(characterId, characterId);

        Assert.Same(characterId, request.AssessorCharacterId);
        Assert.Same(characterId, request.PatientCharacterId);
    }

    [Fact]
    public void RejectsNullIdentitiesAndExposesOnlyTheIdentityContract()
    {
        CharacterId assessor = new("travelling-healer");
        CharacterId patient = new("patient");

        Assert.Throws<ArgumentNullException>(() => new PatientAssessmentRequest(null!, patient));
        Assert.Throws<ArgumentNullException>(() => new PatientAssessmentRequest(assessor, null!));

        Type requestType = typeof(PatientAssessmentRequest);
        (string Name, Type Type, bool CanWrite)[] properties = requestType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                ("AssessorCharacterId", typeof(CharacterId), false),
                ("PatientCharacterId", typeof(CharacterId), false),
            ],
            properties);
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }
}
