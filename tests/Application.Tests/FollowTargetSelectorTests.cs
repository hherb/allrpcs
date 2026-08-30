using System.Numerics;
using Allrpcs.Application.Movement;

namespace Allrpcs.Application.Tests;

public sealed class FollowTargetSelectorTests
{
    private const float TestSpeedPixelsPerSecond = 180.0f;
    private const float TestStopDistancePixels = 72.0f;

    [Fact]
    public void SelectsFollowedPositionOutsideStopDistance()
    {
        Vector2 followerPosition = new(600.0f, 450.0f);
        Vector2 followedPosition = new(800.0f, 450.0f);

        FollowTargetSelection selection = FollowTargetSelector.Select(
            followerPosition,
            followedPosition,
            TestSpeedPixelsPerSecond,
            TestStopDistancePixels);

        Assert.True(selection.ShouldMove);
        Assert.Equal(followedPosition, selection.NavigationDestination);
        Assert.Equal(TestSpeedPixelsPerSecond, selection.SpeedPixelsPerSecond);
        Assert.Equal(TestStopDistancePixels, selection.StopDistancePixels);
    }

    [Theory]
    [InlineData(728.0f, 450.0f)]
    [InlineData(750.0f, 450.0f)]
    [InlineData(800.0f, 450.0f)]
    public void DoesNotSelectTargetWithinInclusiveStopDistance(float followerX, float followerY)
    {
        FollowTargetSelection selection = FollowTargetSelector.Select(
            new Vector2(followerX, followerY),
            new Vector2(800.0f, 450.0f),
            TestSpeedPixelsPerSecond,
            TestStopDistancePixels);

        Assert.False(selection.ShouldMove);
        Assert.Null(selection.NavigationDestination);
    }

    [Theory]
    [InlineData(float.NaN, 450.0f)]
    [InlineData(800.0f, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 450.0f)]
    public void RejectsNonFiniteFollowerPosition(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FollowTargetSelector.Select(
                new Vector2(x, y),
                Vector2.Zero,
                TestSpeedPixelsPerSecond,
                TestStopDistancePixels));
    }

    [Theory]
    [InlineData(float.NaN, 450.0f)]
    [InlineData(800.0f, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 450.0f)]
    public void RejectsNonFiniteFollowedPosition(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FollowTargetSelector.Select(
                Vector2.Zero,
                new Vector2(x, y),
                TestSpeedPixelsPerSecond,
                TestStopDistancePixels));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-1.0f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RejectsInvalidSpeed(float speedPixelsPerSecond)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FollowTargetSelector.Select(
                Vector2.Zero,
                Vector2.One,
                speedPixelsPerSecond,
                TestStopDistancePixels));
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RejectsInvalidStopDistance(float stopDistancePixels)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FollowTargetSelector.Select(
                Vector2.Zero,
                Vector2.One,
                TestSpeedPixelsPerSecond,
                stopDistancePixels));
    }
}
