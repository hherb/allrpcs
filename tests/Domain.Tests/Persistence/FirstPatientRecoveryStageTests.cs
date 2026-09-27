using Allrpcs.Domain.Persistence;

namespace Allrpcs.Domain.Tests.Persistence;

public sealed class FirstPatientRecoveryStageTests
{
    [Theory]
    [InlineData("not-heard")]
    [InlineData("patient-testimony-heard")]
    [InlineData("sage-consulted")]
    public void ParsesCanonicalStagesAsExactSingletons(string value)
    {
        FirstPatientRecoveryStage expected = value switch
        {
            "not-heard" => FirstPatientRecoveryStage.NotHeard,
            "patient-testimony-heard" => FirstPatientRecoveryStage.PatientTestimonyHeard,
            _ => FirstPatientRecoveryStage.SageConsulted,
        };

        FirstPatientRecoveryStage result = FirstPatientRecoveryStage.Parse(value);

        Assert.Same(expected, result);
        Assert.Equal(value, result.Value);
        Assert.Equal(value, result.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("NotHeard")]
    [InlineData("patient-testimony")]
    [InlineData("sage-consulted ")]
    public void RejectsUnknownOrNonCanonicalStages(string value)
    {
        Assert.Throws<ArgumentException>(() => FirstPatientRecoveryStage.Parse(value));
    }

    [Fact]
    public void RejectsNullStageText()
    {
        Assert.Throws<ArgumentNullException>(() => FirstPatientRecoveryStage.Parse(null!));
    }
}
