using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class InteriorAudioRampTests
{
    [Fact]
    public void SlewIsBoundedAndLeavesShelteredRainAtUnity()
    {
        InteriorAudioRamp ramp = new();
        ramp.Advance(new(InteriorAudioRoom.Sickhouse, 1, FootstepMaterial.Timber), 0.25);
        Assert.Equal(1, ramp.Gain(0));
        Assert.Equal(2.0 / 3, ramp.Gain(1), 8);
        ramp.Advance(new(InteriorAudioRoom.Sickhouse, 1, FootstepMaterial.Timber), 0.25);
        Assert.Equal(Math.Pow(10, -9.0 / 20), ramp.Gain(1), 8);
        for (int i = 0; i < 3; i++) { ramp.Advance(InteriorAudioSnapshot.Neutral, 0.25); }
        Assert.Equal(1, ramp.Gain(1));
    }

    [Fact]
    public void FixedTargetPartitionAndDoorwayJitterNeverRestartOrOvershoot()
    {
        InteriorAudioRamp coarse = new();
        InteriorAudioRamp fine = new();
        InteriorAudioSnapshot target = new(InteriorAudioRoom.Laundry, 1, FootstepMaterial.Earth);
        coarse.Advance(target, 0.25);
        for (int i = 0; i < 25; i++) { fine.Advance(target, 0.01); }
        Assert.Equal(coarse.Gain(2), fine.Gain(2), 8);
        for (int i = 0; i < 120; i++)
        {
            fine.Advance(new(InteriorAudioRoom.Sickhouse, i % 2 == 0 ? 0 : 1, FootstepMaterial.Timber), 1 / 60.0);
            Assert.InRange(fine.Gain(2), Math.Pow(10, -9.0 / 20), 1);
        }
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(0.251)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidPresentationTimeResetsToNeutral(double delta)
    {
        InteriorAudioRamp ramp = new();
        ramp.Advance(new(InteriorAudioRoom.Sickhouse, 1, FootstepMaterial.Timber), 0.25);
        Assert.False(ramp.Advance(new(InteriorAudioRoom.Sickhouse, 1, FootstepMaterial.Timber), delta));
        for (int i = 0; i < 4; i++) { Assert.Equal(1, ramp.Gain(i)); }
    }

    [Fact]
    public void ResetAndInvalidSnapshotCannotAttenuateOrAmplify()
    {
        InteriorAudioRamp ramp = new();
        Assert.False(ramp.Advance(new(InteriorAudioRoom.Sickhouse, double.NaN, FootstepMaterial.Timber), 0.1));
        Assert.False(ramp.Advance(new((InteriorAudioRoom)99, 1, FootstepMaterial.Earth), 0.1));
        ramp.Advance(new(InteriorAudioRoom.Laundry, 1, FootstepMaterial.Earth), 0.25);
        ramp.Reset();
        Assert.Equal(1, ramp.Gain(3));
    }
}
