using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseProtectionDefinitionTests
{
    [Fact]
    public void RetainsExactDiseaseAndDiscoverableEvidenceIdentity()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        EvidenceId evidenceId = new("covered-water-prevents-exposure");

        DiseaseProtectionDefinition definition = new(disease, evidenceId);

        Assert.Same(disease, definition.Disease);
        Assert.Same(evidenceId, definition.EvidenceId);
    }

    [Fact]
    public void RejectsNullValuesAndExposesOnlyImmutableProperties()
    {
        DiseaseDefinition disease = DiseaseProgressionPolicyTests.Definition();
        EvidenceId evidenceId = new("covered-water-prevents-exposure");

        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionDefinition(null!, evidenceId));
        Assert.Throws<ArgumentNullException>(() => new DiseaseProtectionDefinition(disease, null!));

        Type definitionType = typeof(DiseaseProtectionDefinition);
        Assert.Equal(2, definitionType.GetProperties().Length);
        Assert.All(definitionType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(definitionType.GetEvents());
        Assert.True(definitionType.IsSealed);
    }
}
