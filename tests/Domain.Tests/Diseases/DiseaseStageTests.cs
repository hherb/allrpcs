using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseStageTests
{
    [Fact]
    public void DefinesTheCompleteCanonicalStageSetInProgressionOrder()
    {
        Assert.Collection(
            DiseaseStage.All,
            stage => Assert.Same(DiseaseStage.Exposed, stage),
            stage => Assert.Same(DiseaseStage.EarlyIllness, stage),
            stage => Assert.Same(DiseaseStage.Drowsing, stage),
            stage => Assert.Same(DiseaseStage.DeepSleep, stage),
            stage => Assert.Same(DiseaseStage.Wasting, stage),
            stage => Assert.Same(DiseaseStage.FinalDecline, stage));
        Assert.Equal(
            ["exposed", "early-illness", "drowsing", "deep-sleep", "wasting", "final-decline"],
            DiseaseStage.All.Select(stage => stage.Value));
    }

    [Fact]
    public void CanonicalTextRoundTripsToTheDefinedInstances()
    {
        foreach (DiseaseStage stage in DiseaseStage.All)
        {
            Assert.Equal(stage.Value, stage.ToString());
            Assert.Same(stage, DiseaseStage.Parse(stage.Value));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Exposed")]
    [InlineData("early_illness")]
    [InlineData("deep-sleep ")]
    [InlineData("recovered")]
    [InlineData("dead")]
    public void RejectsUnknownOrNonCanonicalText(string value)
    {
        Assert.Throws<ArgumentException>(() => DiseaseStage.Parse(value));
    }

    [Fact]
    public void RejectsNullTextAndExposesOnlyTheClosedSurface()
    {
        Assert.Throws<ArgumentNullException>(() => DiseaseStage.Parse(null!));

        string[] publicProperties = typeof(DiseaseStage)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["All", "DeepSleep", "Drowsing", "EarlyIllness", "Exposed", "FinalDecline", "Value", "Wasting"],
            publicProperties);
        Assert.Empty(typeof(DiseaseStage).GetConstructors());
    }
}
