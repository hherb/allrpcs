using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership of bounded independent-task step timing.</summary>
public partial class CharacterIndependentTaskStepClockControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_STEP_CLOCK_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterIndependentTaskStepClockController controller =
            GetNode<CharacterIndependentTaskStepClockController>(
                "../CharacterIndependentTaskStepClockController");
        CharacterIndependentTaskStepCadence cadence = new(TimeSpan.FromSeconds(3));
        if (!RejectsNullInitialization(controller)
            || !controller.TryInitialize(cadence)
            || !ReferenceEquals(controller.Cadence, cadence)
            || controller.Clock is not { } initial
            || !ReferenceEquals(initial.Cadence, cadence)
            || initial.ElapsedTowardNextStep != TimeSpan.Zero
            || controller.LastAdvanceResult is not null
            || controller.TryInitialize(new CharacterIndependentTaskStepCadence(cadence.Interval)))
        {
            Fail("The task-step clock controller rejected exact one-time initialization.");
            return;
        }

        CharacterIndependentTaskStepCadence distinctCadence = new(cadence.Interval);
        CharacterIndependentTaskStepClock distinctClock = new(cadence);
        if (controller.TryAdvance(distinctCadence, initial, TimeSpan.Zero)
            || controller.TryAdvance(cadence, distinctClock, TimeSpan.Zero)
            || controller.TryAdvance(cadence, initial, TimeSpan.FromTicks(-1))
            || controller.TryAdvance(
                cadence,
                initial,
                cadence.Interval + TimeSpan.FromTicks(1))
            || !RejectsNullAdvanceInputs(controller, cadence, initial)
            || !HasExactState(controller, cadence, initial, null))
        {
            Fail("Stale, invalid, or null task-step timing input changed scene state.");
            return;
        }

        if (!controller.TryAdvance(cadence, initial, TimeSpan.FromSeconds(2))
            || controller.Clock is not { } first
            || controller.LastAdvanceResult is not { } firstResult
            || !ReferenceEquals(firstResult.PreviousClock, initial)
            || !ReferenceEquals(firstResult.Clock, first)
            || firstResult.IsStepDue
            || first.ElapsedTowardNextStep != TimeSpan.FromSeconds(2)
            || controller.TryAdvance(cadence, initial, TimeSpan.Zero)
            || !HasExactState(controller, cadence, first, firstResult))
        {
            Fail("The task-step clock controller failed exact immutable accumulation.");
            return;
        }

        if (!controller.TryAdvance(cadence, first, TimeSpan.FromSeconds(2))
            || controller.Clock is not { } dueClock
            || controller.LastAdvanceResult is not { IsStepDue: true } dueResult
            || !ReferenceEquals(dueResult.PreviousClock, first)
            || !ReferenceEquals(dueResult.Clock, dueClock)
            || dueClock.ElapsedTowardNextStep != TimeSpan.FromSeconds(1)
            || controller.TryAdvance(cadence, first, TimeSpan.Zero)
            || !HasExactState(controller, cadence, dueClock, dueResult))
        {
            Fail("The task-step clock controller did not expose one exact due step.");
            return;
        }

        GD.Print(
            "Independent-task step clock controller smoke passed: exact one-time cadence "
            + "ownership, compare-and-swap immutable elapsed progress, one due step, and "
            + "fail-closed stale, invalid, repeated, and null input.");
        GetTree().Quit();
    }

    private static bool HasExactState(
        CharacterIndependentTaskStepClockController controller,
        CharacterIndependentTaskStepCadence cadence,
        CharacterIndependentTaskStepClock clock,
        CharacterIndependentTaskStepClockAdvanceResult? result) =>
        ReferenceEquals(controller.Cadence, cadence)
        && ReferenceEquals(controller.Clock, clock)
        && ReferenceEquals(controller.LastAdvanceResult, result);

    private static bool RejectsNullInitialization(
        CharacterIndependentTaskStepClockController controller) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!))
        && controller.Cadence is null
        && controller.Clock is null
        && controller.LastAdvanceResult is null;

    private static bool RejectsNullAdvanceInputs(
        CharacterIndependentTaskStepClockController controller,
        CharacterIndependentTaskStepCadence cadence,
        CharacterIndependentTaskStepClock clock) =>
        ThrowsArgumentNull(() => controller.TryAdvance(null!, clock, TimeSpan.Zero))
        && ThrowsArgumentNull(() => controller.TryAdvance(cadence, null!, TimeSpan.Zero));

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
