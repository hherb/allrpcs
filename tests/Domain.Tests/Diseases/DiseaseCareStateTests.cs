using Allrpcs.Domain.Diseases;

namespace Allrpcs.Domain.Tests.Diseases;

public sealed class DiseaseCareStateTests
{
    [Fact]
    public void DefinesClosedCanonicalStatesInEffectOrder()
    {
        Assert.Collection(
            DiseaseCareState.All,
            state => Assert.Same(DiseaseCareState.Unassisted, state),
            state => Assert.Same(DiseaseCareState.Comforted, state),
            state => Assert.Same(DiseaseCareState.Stabilized, state));

        foreach (DiseaseCareState state in DiseaseCareState.All)
        {
            Assert.Same(state, DiseaseCareState.Parse(state.Value));
            Assert.Equal(state.Value, state.ToString());
        }
    }

    [Fact]
    public void UsesCanonicalSingletonEqualityAndRejectsUnknownText()
    {
        Assert.Equal(DiseaseCareState.Stabilized, DiseaseCareState.Parse("stabilized"));
        Assert.NotEqual(DiseaseCareState.Comforted, DiseaseCareState.Stabilized);
        Assert.Throws<ArgumentException>(() => DiseaseCareState.Parse("cured"));
        Assert.Throws<ArgumentException>(() => DiseaseCareState.Parse("Stabilized"));
        Assert.Throws<ArgumentNullException>(() => DiseaseCareState.Parse(null!));
    }
}
