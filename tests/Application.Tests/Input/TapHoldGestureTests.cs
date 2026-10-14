using Allrpcs.Application.Input;

namespace Allrpcs.Application.Tests.Input;

public sealed class TapHoldGestureTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(299.9)]
    public void ShortPressIsOneNativeTap(double duration)
    {
        TapHoldGesture gesture = new(); gesture.Press(100);
        Assert.Equal(TapHoldEffect.None, gesture.Tick(100 + duration));
        Assert.Equal(TapHoldEffect.Tap, gesture.Release(100 + duration));
        Assert.False(gesture.Active);
    }
    [Fact]
    public void HoldStartsOnceAndReleaseFinalizesWithoutTyping()
    {
        TapHoldGesture gesture = new(); gesture.Press(100);
        Assert.Equal(TapHoldEffect.BeginVoice, gesture.Tick(400));
        gesture.Press(500);
        Assert.Equal(TapHoldEffect.None, gesture.Tick(600));
        Assert.Equal(TapHoldEffect.EndVoice, gesture.Release(800));
    }
    [Fact]
    public void LateReleaseDoesNotReinterpretAHoldAsATap()
    {
        TapHoldGesture gesture = new(); gesture.Press(0);
        Assert.Equal(TapHoldEffect.BeginVoice | TapHoldEffect.EndVoice, gesture.Release(350));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationFencesRepeatedPressUntilPhysicalRelease(bool started)
    {
        TapHoldGesture gesture = new(); gesture.Press(0);
        if (started) { gesture.Tick(300); }
        gesture.Cancel(); gesture.Press(500);
        Assert.Equal(TapHoldEffect.None, gesture.Tick(800));
        Assert.Equal(TapHoldEffect.None, gesture.Release(900));
        gesture.Press(1000);
        Assert.Equal(TapHoldEffect.BeginVoice, gesture.Tick(1300));
    }
    [Fact]
    public void UnownedReleasesAndIdleTicksHaveNoEffect()
    {
        TapHoldGesture gesture = new(); gesture.Cancel();
        Assert.Equal(TapHoldEffect.None, gesture.Release(100));
        Assert.Equal(TapHoldEffect.None, gesture.Tick(200));
    }
    [Fact]
    public void InvalidOrBackwardsClocksAreRejected()
    {
        TapHoldGesture gesture = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => gesture.Press(double.NaN));
        gesture.Press(100);
        Assert.Throws<ArgumentOutOfRangeException>(() => gesture.Tick(99));
    }
}
