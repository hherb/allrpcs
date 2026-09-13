using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskStepClockTests
{
    [Fact]
    public void CadenceAndInitialClockRetainValidatedImmutableTiming()
    {
        TimeSpan interval = TimeSpan.FromSeconds(3);

        CharacterIndependentTaskStepCadence cadence = new(interval);
        CharacterIndependentTaskStepClock clock = new(cadence);

        Assert.Equal(interval, cadence.Interval);
        Assert.Same(cadence, clock.Cadence);
        Assert.Equal(TimeSpan.Zero, clock.ElapsedTowardNextStep);
        Assert.All(
            typeof(CharacterIndependentTaskStepCadence).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.All(
            typeof(CharacterIndependentTaskStepClock).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void ClockAccumulatesElapsedTimeAndSignalsOnlyOneExactDueStep()
    {
        CharacterIndependentTaskStepCadence cadence = new(TimeSpan.FromSeconds(3));
        CharacterIndependentTaskStepClock initial = new(cadence);

        CharacterIndependentTaskStepClockAdvanceResult first =
            CharacterIndependentTaskStepClockTransition.Advance(
                initial,
                cadence,
                TimeSpan.FromSeconds(2));
        CharacterIndependentTaskStepClockAdvanceResult due =
            CharacterIndependentTaskStepClockTransition.Advance(
                first.Clock,
                cadence,
                TimeSpan.FromSeconds(2));

        Assert.Same(initial, first.PreviousClock);
        Assert.NotSame(initial, first.Clock);
        Assert.Same(cadence, first.Clock.Cadence);
        Assert.Equal(TimeSpan.FromSeconds(2), first.Clock.ElapsedTowardNextStep);
        Assert.False(first.IsStepDue);
        Assert.Same(first.Clock, due.PreviousClock);
        Assert.NotSame(first.Clock, due.Clock);
        Assert.Same(cadence, due.Clock.Cadence);
        Assert.Equal(TimeSpan.FromSeconds(1), due.Clock.ElapsedTowardNextStep);
        Assert.True(due.IsStepDue);
        Assert.Equal(TimeSpan.Zero, initial.ElapsedTowardNextStep);
    }

    [Fact]
    public void ClockAcceptsZeroAndExactIntervalBoundaries()
    {
        CharacterIndependentTaskStepCadence cadence = new(TimeSpan.FromTicks(1));
        CharacterIndependentTaskStepClock initial = new(cadence);

        CharacterIndependentTaskStepClockAdvanceResult unchanged =
            CharacterIndependentTaskStepClockTransition.Advance(
                initial,
                cadence,
                TimeSpan.Zero);
        CharacterIndependentTaskStepClockAdvanceResult due =
            CharacterIndependentTaskStepClockTransition.Advance(
                unchanged.Clock,
                cadence,
                cadence.Interval);

        Assert.Equal(TimeSpan.Zero, unchanged.Clock.ElapsedTowardNextStep);
        Assert.False(unchanged.IsStepDue);
        Assert.Equal(TimeSpan.Zero, due.Clock.ElapsedTowardNextStep);
        Assert.True(due.IsStepDue);
    }

    [Fact]
    public void CadenceRejectsNonPositiveInfiniteAndOversizedIntervals()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskStepCadence(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskStepCadence(TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskStepCadence(Timeout.InfiniteTimeSpan));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CharacterIndependentTaskStepCadence(
                CharacterIndependentTaskStepCadence.MaximumInterval + TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void ClockRejectsDistinctCadenceInvalidElapsedAndNulls()
    {
        CharacterIndependentTaskStepCadence cadence = new(TimeSpan.FromSeconds(3));
        CharacterIndependentTaskStepCadence distinctCadence = new(cadence.Interval);
        CharacterIndependentTaskStepClock clock = new(cadence);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskStepClockTransition.Advance(
                clock,
                distinctCadence,
                TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterIndependentTaskStepClockTransition.Advance(
                clock,
                cadence,
                TimeSpan.FromTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterIndependentTaskStepClockTransition.Advance(
                clock,
                cadence,
                cadence.Interval + TimeSpan.FromTicks(1)));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterIndependentTaskStepClock(null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskStepClockTransition.Advance(
                null!,
                cadence,
                TimeSpan.Zero));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskStepClockTransition.Advance(
                clock,
                null!,
                TimeSpan.Zero));
        Assert.Equal(TimeSpan.Zero, clock.ElapsedTowardNextStep);
    }
}
