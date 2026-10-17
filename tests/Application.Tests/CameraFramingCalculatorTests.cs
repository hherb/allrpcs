using System.Numerics;
using Allrpcs.Application.Camera;

namespace Allrpcs.Application.Tests;

public sealed class CameraFramingCalculatorTests
{
    private static readonly CameraWorldBounds TestWorldBounds = new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly Vector2 TestViewportSize = new(1280.0f, 720.0f);

    [Fact]
    public void CreatesValidatedFramingPlan()
    {
        CameraFramingPlan plan = CameraFramingCalculator.Create(
            TestWorldBounds,
            TestViewportSize,
            minimumZoom: 0.8f,
            defaultZoom: 1.0f,
            maximumZoom: 2.0f);

        Assert.Equal(TestWorldBounds, plan.WorldBounds);
        Assert.Equal(0.8f, plan.MinimumZoom);
        Assert.Equal(1.0f, plan.DefaultZoom);
        Assert.Equal(2.0f, plan.MaximumZoom);
    }

    [Fact]
    public void AcceptsMinimumZoomThatExactlyCoversWorld()
    {
        CameraFramingPlan plan = CameraFramingCalculator.Create(
            TestWorldBounds,
            TestViewportSize,
            minimumZoom: 0.8f,
            defaultZoom: 1.0f,
            maximumZoom: 2.0f);

        Assert.Equal(0.8f, plan.MinimumZoom);
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
            CameraFramingCalculator.Create(bounds, TestViewportSize, 0.8f, 1.0f, 2.0f));
    }

    [Theory]
    [InlineData(0.0f, 720.0f)]
    [InlineData(1280.0f, 0.0f)]
    [InlineData(float.NaN, 720.0f)]
    [InlineData(1280.0f, float.NegativeInfinity)]
    public void RejectsInvalidViewportSize(float width, float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(
                TestWorldBounds,
                new Vector2(width, height),
                0.8f,
                1.0f,
                2.0f));
    }

    [Theory]
    [InlineData(0.0f, 1.0f, 2.0f)]
    [InlineData(1.25f, 1.0f, 2.0f)]
    [InlineData(0.8f, 2.5f, 2.0f)]
    [InlineData(0.8f, float.NaN, 2.0f)]
    [InlineData(0.8f, 1.0f, float.PositiveInfinity)]
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

    [Theory]
    [InlineData(float.Epsilon, 1.0f)]
    [InlineData(1.0f, float.Epsilon)]
    public void RejectsUnrepresentableCoveringZoom(float worldWidth, float worldHeight)
    {
        CameraWorldBounds bounds = new(0.0f, 0.0f, worldWidth, worldHeight);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CameraFramingCalculator.Create(bounds, Vector2.One, 0.8f, 1.0f, 2.0f));
    }

    [Theory]
    [InlineData(1280.0f, 720.0f, 0.8f)]
    [InlineData(1279.6875f, 2275.0f, 2275.0f / 900.0f)]
    [InlineData(900.0f, 1600.0f, 1600.0f / 900.0f)]
    public void ClampsZoomToCoverViewport(float width, float height, float expectedMinimum)
    {
        CameraFramingPlan plan = CameraFramingCalculator.Create(
            TestWorldBounds, new Vector2(width, height), 0.75f, 1.0f, 2.0f);

        Assert.Equal(expectedMinimum, plan.MinimumZoom);
        Assert.Equal(Math.Clamp(1.0f, expectedMinimum, Math.Max(2.0f, expectedMinimum)), plan.DefaultZoom);
        Assert.Equal(Math.Max(2.0f, expectedMinimum), plan.MaximumZoom);
        Assert.True(float.IsFinite(plan.MinimumZoom));
        Assert.True(float.IsFinite(plan.DefaultZoom));
        Assert.True(float.IsFinite(plan.MaximumZoom));
        Assert.True(plan.MinimumZoom <= plan.DefaultZoom && plan.DefaultZoom <= plan.MaximumZoom);
        Assert.True(width / plan.MinimumZoom <= TestWorldBounds.Width + 0.001f);
        Assert.True(height / plan.MinimumZoom <= TestWorldBounds.Height + 0.001f);
    }
}
