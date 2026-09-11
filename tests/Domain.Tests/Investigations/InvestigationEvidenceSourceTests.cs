using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Tests.Investigations;

public sealed class InvestigationEvidenceSourceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RetainsExactSourceAndEvidenceIdentitiesAndCooperationRequirement(
        bool requiresCooperation)
    {
        InvestigationEvidenceSourceId sourceId = new("healer-testimony");
        EvidenceId evidenceId = new("healer-remedy-note");

        InvestigationEvidenceSource source = new(
            sourceId,
            evidenceId,
            requiresCooperation);

        Assert.Same(sourceId, source.SourceId);
        Assert.Same(evidenceId, source.EvidenceId);
        Assert.Equal(requiresCooperation, source.RequiresCooperation);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheImmutableSourceSurface()
    {
        InvestigationEvidenceSourceId sourceId = new("healer-testimony");
        EvidenceId evidenceId = new("healer-remedy-note");

        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationEvidenceSource(null!, evidenceId, false));
        Assert.Throws<ArgumentNullException>(() =>
            new InvestigationEvidenceSource(sourceId, null!, false));

        Type sourceType = typeof(InvestigationEvidenceSource);
        System.Reflection.PropertyInfo[] properties = sourceType.GetProperties();
        System.Reflection.ConstructorInfo constructor = Assert.Single(sourceType.GetConstructors());

        Assert.Equal(
            ["EvidenceId", "RequiresCooperation", "SourceId"],
            properties.Select(property => property.Name).Order());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Equal(
            [typeof(InvestigationEvidenceSourceId), typeof(EvidenceId), typeof(bool)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Empty(sourceType.GetEvents());
        Assert.Empty(sourceType.GetMethods().Where(
            method => method.DeclaringType == sourceType && !method.IsSpecialName));
        Assert.True(sourceType.IsSealed);
    }
}
