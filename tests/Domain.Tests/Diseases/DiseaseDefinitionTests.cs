using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseDefinitionTests
{
    [Fact]
    public void RetainsExactDiseaseIdentity()
    {
        DiseaseId diseaseId = new("grey-sleep");

        DiseaseDefinition definition = new(diseaseId);

        Assert.Same(diseaseId, definition.DiseaseId);
    }

    [Fact]
    public void RejectsNullIdentity()
    {
        Assert.Throws<ArgumentNullException>(() => new DiseaseDefinition(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableDefinitionSurface()
    {
        Type definitionType = typeof(DiseaseDefinition);
        System.Reflection.PropertyInfo property = Assert.Single(definitionType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(definitionType.GetConstructors());

        Assert.Equal("DiseaseId", property.Name);
        Assert.Equal(typeof(DiseaseId), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(DiseaseId), parameter.ParameterType));
        Assert.Empty(definitionType.GetEvents());
        Assert.True(definitionType.IsSealed);
    }
}
