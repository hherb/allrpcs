using System.Numerics;
using Allrpcs.Application.Camera;

namespace Allrpcs.Application.Tests;

public sealed class CameraFramingCalculatorTests
{
    private static readonly CameraWorldBounds TestWorldBounds = new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly Vector2 TestViewportSize = new(960.0f, 540.0f);

    [Fact]
    public void CreatesValidatedFramingPlan()
    {
        CameraFramingPlan plan = CameraFramingCalculator.Create(
            TestWorldBounds,
            TestViewportSize,
            minimumZoom: 0.75f,
            defaultZoom: 1.0f,
            maximumZoom: 2.0f);

        Assert.Equal(TestWorldBounds, plan.WorldBounds);
        Assert.Equal(0.75f, plan.MinimumZoom);
        Assert.Equal(1.0f, plan.DefaultZoom);
        Assert.Equal(2.0f, plan.MaximumZoom);
    }

    [Fact]
    public void AcceptsMinimumZoomThatExactlyCoversWorld()
    {
        CameraFramingPlan plan = CameraFramingCalculator.Create(
            TestWorldBounds,
            TestViewportSize,
            minimumZoom: 0.6f,
            defaultZoom: 1.0f,
            maximumZoom: 2.0f);

        Assert.Equal(0.6f, plan.MinimumZoom);
    }

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f, 900.0f)]
    [InlineData(0.0f, 0.0f, 1600.0f, 0.0f)]
    [InlineData(1600.0f, 0.0f, 0.0f, 900.0f)]
    [InlineData(0.0f, 900.0f, 1600.0f, 0.0f)]
    [InlineData(0.0f, 0.0f, float.PositiveInfinity, 900.0f)]
    public void RejectsInvalidWorldBounds(float left, float top, float right, float bottom)
    {
        CameraWorldBounds bounds = new(left, top, right, bottom);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(bounds, TestViewportSize, 0.75f, 1.0f, 2.0f));
    }

    [Theory]
    [InlineData(0.0f, 540.0f)]
    [InlineData(960.0f, 0.0f)]
    [InlineData(float.NaN, 540.0f)]
    [InlineData(960.0f, float.NegativeInfinity)]
    public void RejectsInvalidViewportSize(float width, float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(
                TestWorldBounds,
                new Vector2(width, height),
                0.75f,
                1.0f,
                2.0f));
    }

    [Theory]
    [InlineData(0.0f, 1.0f, 2.0f)]
    [InlineData(1.25f, 1.0f, 2.0f)]
    [InlineData(0.75f, 2.5f, 2.0f)]
    [InlineData(0.75f, float.NaN, 2.0f)]
    [InlineData(0.75f, 1.0f, float.PositiveInfinity)]
    public void RejectsInvalidZoomTuning(float minimumZoom, float defaultZoom, float maximumZoom)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(
                TestWorldBounds,
                TestViewportSize,
                minimumZoom,
                defaultZoom,
                maximumZoom));
    }

    [Fact]
    public void RejectsMinimumZoomThatWouldRevealBeyondWorld()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(
                TestWorldBounds,
                TestViewportSize,
                minimumZoom: 0.5f,
                defaultZoom: 1.0f,
                maximumZoom: 2.0f));
    }
}
