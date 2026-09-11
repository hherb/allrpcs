using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class CharacterPatientPerceptionTests
{
    [Fact]
    public void RetainsExactAssessorAndPatientIdentities()
    {
        CharacterId assessor = new("travelling-healer");
        CharacterId patient = new("patient");

        CharacterPatientPerception perception = new(assessor, patient);

        Assert.Same(assessor, perception.AssessorCharacterId);
        Assert.Same(patient, perception.PatientCharacterId);
    }

    [Fact]
    public void RejectsNullIdentitiesAndExposesOnlyTheIdentityContract()
    {
        CharacterId assessor = new("travelling-healer");
        CharacterId patient = new("patient");

        Assert.Throws<ArgumentNullException>(() => new CharacterPatientPerception(null!, patient));
        Assert.Throws<ArgumentNullException>(() => new CharacterPatientPerception(assessor, null!));

        Type perceptionType = typeof(CharacterPatientPerception);
        (string Name, Type Type, bool CanWrite)[] properties = perceptionType
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
        Assert.Empty(perceptionType.GetEvents());
        Assert.True(perceptionType.IsSealed);
    }
}
