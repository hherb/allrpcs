using System.Numerics;
using Allrpcs.Application.Movement;

namespace Allrpcs.Application.Tests;

public sealed class MovementCalculatorTests
{
    private const float TestSpeedPixelsPerSecond = 240.0f;

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f, 0.0f)]
    [InlineData(1.0f, 0.0f, TestSpeedPixelsPerSecond, 0.0f)]
    [InlineData(0.0f, -1.0f, 0.0f, -TestSpeedPixelsPerSecond)]
    [InlineData(0.5f, 0.0f, TestSpeedPixelsPerSecond / 2.0f, 0.0f)]
    public void CalculatesCardinalAndProportionalVelocity(
        float inputX,
        float inputY,
        float expectedX,
        float expectedY)
    {
        Vector2 velocity = MovementCalculator.CalculateVelocity(
            new Vector2(inputX, inputY),
            TestSpeedPixelsPerSecond);

        Assert.Equal(new Vector2(expectedX, expectedY), velocity);
    }

    [Fact]
    public void NormalizesDiagonalInputToConfiguredSpeed()
    {
        Vector2 velocity = MovementCalculator.CalculateVelocity(
            new Vector2(1.0f, 1.0f),
            TestSpeedPixelsPerSecond);

        Assert.Equal(TestSpeedPixelsPerSecond, velocity.Length(), precision: 4);
        Assert.Equal(velocity.X, velocity.Y, precision: 4);
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RejectsInvalidSpeed(float speedPixelsPerSecond)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovementCalculator.CalculateVelocity(Vector2.UnitX, speedPixelsPerSecond));
    }

    [Fact]
    public void RejectsNonFiniteInput()
    {
        Vector2 input = new(float.NegativeInfinity, 0.0f);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovementCalculator.CalculateVelocity(input, TestSpeedPixelsPerSecond));
    }
}
