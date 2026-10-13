using Allrpcs.Application.Input;

namespace Allrpcs.Application.Tests.Input;

public sealed class ControlBindingCaptureTests
{
    [Fact]
    public void InitiatorEchoWrongClassAndReleaseNeverBecomeCandidates()
    {
        ControlBindingCapture capture = new(); capture.Begin("Keyboard"); ControlBinding key = new("key", 81, "physical");
        Assert.Null(capture.Observe(key, true)); capture.ArmAfterRelease();
        Assert.Null(capture.Observe(key, false)); Assert.Null(capture.Observe(key, true, echo: true));
        Assert.Null(capture.Observe(new("mouseButton", 8), true)); Assert.True(capture.Active);
        Assert.Same(key, capture.Observe(key, true)); Assert.False(capture.Active);
        Assert.Null(capture.Observe(new("key", 76, "physical"), true));
    }
    [Theory]
    [InlineData("Mouse", "mouseButton", 8)]
    [InlineData("Controller", "joyButton", 20)]
    public void CapturesExplicitDeviceClass(string device, string kind, int code)
    {
        ControlBindingCapture capture = new(); capture.Begin(device); capture.ArmAfterRelease();
        Assert.Equal(code, capture.Observe(new(kind, code), true)!.Code);
    }
    [Fact]
    public void AxisRequiresNeutralThenCrossingAndSelectedDevice()
    {
        ControlBindingCapture capture = new(); capture.Begin("Controller", 3); capture.ArmAfterRelease();
        Assert.Null(capture.ObserveAxis(0, 0.9, 3)); Assert.Null(capture.ObserveAxis(0, 0.1, 3));
        Assert.Null(capture.ObserveAxis(0, -0.8, 4)); Assert.Null(capture.ObserveAxis(0, 0.7, 3));
        Assert.Null(capture.ObserveAxis(1, -0.9, 3));
        ControlBinding result = capture.ObserveAxis(0, -0.75, 3)!;
        Assert.Equal(-1, result.Sign); Assert.Equal(0, result.Code);
    }
    [Fact]
    public void FirstControllerIsRetainedUntilEnd()
    {
        ControlBindingCapture capture = new(); capture.Begin("Controller"); capture.ArmAfterRelease();
        Assert.Null(capture.ObserveAxis(2, 0, 7)); Assert.Equal(7, capture.Device);
        Assert.Null(capture.Observe(new("joyButton", 0), true, device: 8));
        capture.Disconnect(8); Assert.True(capture.Active); capture.Disconnect(7); Assert.False(capture.Active);
        Assert.Null(capture.Candidate); Assert.Contains("disconnected", capture.Status);
    }
    [Fact]
    public void TimeoutEscapeAndFocusCancellationCannotApplyCandidates()
    {
        ControlBindingCapture capture = new(); capture.Begin("Keyboard"); capture.ArmAfterRelease();
        capture.Advance(9.9); Assert.True(capture.Active); capture.Advance(0.1); Assert.False(capture.Active);
        Assert.Null(capture.Observe(new("key", 81, "physical"), true));
        capture.Begin("Keyboard"); capture.ArmAfterRelease();
        Assert.Null(capture.Observe(new("key", 4194305, "physical"), true)); Assert.False(capture.Active);
        capture.Begin("Mouse"); capture.Cancel("Window lost focus"); Assert.Null(capture.Candidate);
        Assert.Throws<ArgumentOutOfRangeException>(() => capture.Advance(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => capture.Advance(-1));
    }
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1.1)]
    public void InvalidAxisSamplesCannotSelectCandidates(double value)
    {
        ControlBindingCapture capture = new(); capture.Begin("Controller"); capture.ArmAfterRelease();
        Assert.Null(capture.ObserveAxis(0, value, 1)); Assert.True(capture.Active);
    }
}
