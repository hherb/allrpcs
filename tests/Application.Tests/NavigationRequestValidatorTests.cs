using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests;

public sealed class NavigationRequestValidatorTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void CreatesRequestInsideWorldBounds()
    {
        Vector2 origin = new(800.0f, 450.0f);
        Vector2 destination = new(808.0f, 80.0f);

        NavigationRequest request = NavigationRequestValidator.Create(
            TestWorldBounds,
            origin,
            destination);

        Assert.Equal(TestWorldBounds, request.WorldBounds);
        Assert.Equal(origin, request.Origin);
        Assert.Equal(destination, request.Destination);
    }

    [Theory]
    [InlineData(0.0f, 0.0f)]
    [InlineData(1600.0f, 900.0f)]
    public void AcceptsPointsOnWorldBoundary(float x, float y)
    {
        Vector2 point = new(x, y);

        NavigationRequest request = NavigationRequestValidator.Create(
            TestWorldBounds,
            point,
            point);

        Assert.Equal(point, request.Origin);
        Assert.Equal(point, request.Destination);
    }

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f, 900.0f)]
    [InlineData(0.0f, 0.0f, 1600.0f, 0.0f)]
    [InlineData(1600.0f, 0.0f, 0.0f, 900.0f)]
    [InlineData(0.0f, 900.0f, 1600.0f, 0.0f)]
    [InlineData(float.NaN, 0.0f, 1600.0f, 900.0f)]
    [InlineData(0.0f, 0.0f, float.PositiveInfinity, 900.0f)]
    public void RejectsInvalidWorldBounds(float left, float top, float right, float bottom)
    {
        NavigationWorldBounds bounds = new(left, top, right, bottom);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NavigationRequestValidator.Create(bounds, Vector2.Zero, Vector2.One));
    }

    [Theory]
    [InlineData(float.NaN, 450.0f)]
    [InlineData(800.0f, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 450.0f)]
    public void RejectsNonFiniteOrigin(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NavigationRequestValidator.Create(TestWorldBounds, new Vector2(x, y), Vector2.One));
    }

    [Theory]
    [InlineData(float.NaN, 450.0f)]
    [InlineData(800.0f, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 450.0f)]
    public void RejectsNonFiniteDestination(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NavigationRequestValidator.Create(TestWorldBounds, Vector2.One, new Vector2(x, y)));
    }

    [Theory]
    [InlineData(-0.01f, 450.0f)]
    [InlineData(1600.01f, 450.0f)]
    [InlineData(800.0f, -0.01f)]
    [InlineData(800.0f, 900.01f)]
    public void RejectsOriginOutsideWorldBounds(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NavigationRequestValidator.Create(TestWorldBounds, new Vector2(x, y), Vector2.One));
    }

    [Theory]
    [InlineData(-0.01f, 450.0f)]
    [InlineData(1600.01f, 450.0f)]
    [InlineData(800.0f, -0.01f)]
    [InlineData(800.0f, 900.01f)]
    public void RejectsDestinationOutsideWorldBounds(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NavigationRequestValidator.Create(TestWorldBounds, Vector2.One, new Vector2(x, y)));
    }
}
