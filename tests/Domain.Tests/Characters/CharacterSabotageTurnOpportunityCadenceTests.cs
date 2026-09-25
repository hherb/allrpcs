using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnOpportunityCadenceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void RejectsOutOfRangeMeaningfulEventIntervals(int interval)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CharacterSabotageTurnOpportunityCadence(interval));
    }

    [Fact]
    public void RetainsOneBoundedMeaningfulEventInterval()
    {
        CharacterSabotageTurnOpportunityCadence cadence = new(3);

        Assert.Equal(3, cadence.MeaningfulEventInterval);
    }
}
