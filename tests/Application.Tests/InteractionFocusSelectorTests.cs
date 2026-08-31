using System.Numerics;
using Allrpcs.Application.Interaction;

namespace Allrpcs.Application.Tests;

public sealed class InteractionFocusSelectorTests
{
    private const float InteractionRangePixels = 100.0f;
    private static readonly Vector2 SeekerPosition = new(400.0f, 300.0f);

    [Fact]
    public void SelectsOnlyVisibleEnabledCandidateInsideInclusiveRange()
    {
        InteractionFocusCandidate[] candidates =
        [
            CandidateAt(500.0f, 300.0f),
            CandidateAt(420.0f, 300.0f, isVisible: false),
            CandidateAt(430.0f, 300.0f, isInteractionEnabled: false),
            CandidateAt(500.01f, 300.0f),
        ];

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.NotNull(selection);
        Assert.Equal(0, selection.Value.CandidateIndex);
        Assert.Equal(InteractionRangePixels, selection.Value.DistancePixels);
    }

    [Fact]
    public void SelectsNearestEligibleCandidate()
    {
        InteractionFocusCandidate[] candidates =
        [
            CandidateAt(490.0f, 300.0f),
            CandidateAt(430.0f, 340.0f),
            CandidateAt(470.0f, 300.0f),
        ];

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.NotNull(selection);
        Assert.Equal(1, selection.Value.CandidateIndex);
        Assert.Equal(50.0f, selection.Value.DistancePixels);
    }

    [Fact]
    public void BreaksEqualDistanceTieByAuthoredInputOrder()
    {
        InteractionFocusCandidate[] candidates =
        [
            CandidateAt(350.0f, 300.0f),
            CandidateAt(450.0f, 300.0f),
        ];

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.NotNull(selection);
        Assert.Equal(0, selection.Value.CandidateIndex);
    }

    [Fact]
    public void IgnoresCloserHiddenAndDisabledCandidates()
    {
        InteractionFocusCandidate[] candidates =
        [
            CandidateAt(401.0f, 300.0f, isVisible: false),
            CandidateAt(402.0f, 300.0f, isInteractionEnabled: false),
            CandidateAt(440.0f, 300.0f),
        ];

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.NotNull(selection);
        Assert.Equal(2, selection.Value.CandidateIndex);
    }

    [Fact]
    public void ReturnsNoFocusWhenThereAreNoEligibleCandidates()
    {
        InteractionFocusCandidate[] candidates =
        [
            CandidateAt(401.0f, 300.0f, isVisible: false),
            CandidateAt(402.0f, 300.0f, isInteractionEnabled: false),
            CandidateAt(501.0f, 300.0f),
        ];

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.Null(selection);
    }

    [Fact]
    public void ReturnsNoFocusForEmptyCandidateSet()
    {
        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            Array.Empty<InteractionFocusCandidate>());

        Assert.Null(selection);
    }

    [Fact]
    public void RejectsNullCandidateSet()
    {
        Assert.Throws<ArgumentNullException>(() => InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            null!));
    }

    [Fact]
    public void RejectsCandidateSetAboveBound()
    {
        InteractionFocusCandidate[] candidates = Enumerable
            .Repeat(CandidateAt(400.0f, 300.0f), InteractionFocusSelector.MaximumCandidateCount + 1)
            .ToArray();

        Assert.Throws<ArgumentOutOfRangeException>(() => InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates));
    }

    [Fact]
    public void AcceptsCandidateSetAtBound()
    {
        InteractionFocusCandidate[] candidates = Enumerable
            .Repeat(CandidateAt(400.0f, 300.0f), InteractionFocusSelector.MaximumCandidateCount)
            .ToArray();

        InteractionFocusSelection? selection = InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates);

        Assert.NotNull(selection);
        Assert.Equal(0, selection.Value.CandidateIndex);
    }

    [Theory]
    [InlineData(float.NaN, 300.0f)]
    [InlineData(float.PositiveInfinity, 300.0f)]
    [InlineData(400.0f, float.NegativeInfinity)]
    public void RejectsNonFiniteSeekerPosition(float x, float y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => InteractionFocusSelector.Select(
            new Vector2(x, y),
            InteractionRangePixels,
            Array.Empty<InteractionFocusCandidate>()));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(-0.01f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void RejectsInvalidInteractionRange(float interactionRangePixels)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => InteractionFocusSelector.Select(
            SeekerPosition,
            interactionRangePixels,
            Array.Empty<InteractionFocusCandidate>()));
    }

    [Theory]
    [InlineData(float.NaN, 300.0f)]
    [InlineData(float.PositiveInfinity, 300.0f)]
    [InlineData(400.0f, float.NegativeInfinity)]
    public void RejectsNonFiniteCandidatePositionEvenWhenCandidateIsHidden(float x, float y)
    {
        InteractionFocusCandidate[] candidates =
        [
            new InteractionFocusCandidate(
                new Vector2(x, y),
                IsVisible: false,
                IsInteractionEnabled: false),
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => InteractionFocusSelector.Select(
            SeekerPosition,
            InteractionRangePixels,
            candidates));
    }

    private static InteractionFocusCandidate CandidateAt(
        float x,
        float y,
        bool isVisible = true,
        bool isInteractionEnabled = true)
    {
        return new InteractionFocusCandidate(
            new Vector2(x, y),
            isVisible,
            isInteractionEnabled);
    }
}
