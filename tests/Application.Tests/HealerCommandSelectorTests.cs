using System.Numerics;
using Allrpcs.Application.Movement;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests;

public sealed class HealerCommandSelectorTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void SelectsFollowWithoutDestination()
    {
        HealerCommandSelection selection = HealerCommandSelector.SelectFollow();

        Assert.Equal(HealerCommandMode.Follow, selection.Mode);
        Assert.Null(selection.NavigationDestination);
    }

    [Fact]
    public void SelectsWaitWithoutDestination()
    {
        HealerCommandSelection selection = HealerCommandSelector.SelectWait();

        Assert.Equal(HealerCommandMode.Wait, selection.Mode);
        Assert.Null(selection.NavigationDestination);
    }

    [Theory]
    [InlineData(800.0f, 450.0f)]
    [InlineData(0.0f, 0.0f)]
    [InlineData(1600.0f, 900.0f)]
    public void SelectsFiniteMoveDestinationInsideInclusiveWorldBounds(float x, float y)
    {
        Vector2 destination = new(x, y);

        HealerCommandSelection selection = HealerCommandSelector.SelectMoveToDestination(
            TestWorldBounds,
            destination);

        Assert.Equal(HealerCommandMode.MoveToDestination, selection.Mode);
        Assert.Equal(destination, selection.NavigationDestination);
    }

    [Theory]
    [InlineData(float.NaN, 450.0f)]
    [InlineData(800.0f, float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, 450.0f)]
    public void RejectsNonFiniteMoveDestination(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            HealerCommandSelector.SelectMoveToDestination(
                TestWorldBounds,
                new Vector2(x, y)));
    }

    [Theory]
    [InlineData(-0.01f, 450.0f)]
    [InlineData(1600.01f, 450.0f)]
    [InlineData(800.0f, -0.01f)]
    [InlineData(800.0f, 900.01f)]
    public void RejectsMoveDestinationOutsideWorldBounds(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            HealerCommandSelector.SelectMoveToDestination(
                TestWorldBounds,
                new Vector2(x, y)));
    }

    [Theory]
    [InlineData(0.0f, 0.0f, 0.0f, 900.0f)]
    [InlineData(0.0f, 0.0f, 1600.0f, 0.0f)]
    [InlineData(float.NaN, 0.0f, 1600.0f, 900.0f)]
    public void RejectsMoveDestinationWithInvalidWorldBounds(
        float left,
        float top,
        float right,
        float bottom)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            HealerCommandSelector.SelectMoveToDestination(
                new NavigationWorldBounds(left, top, right, bottom),
                Vector2.One));
    }
}
