using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free autonomous-conversation budget ownership checks.</summary>
public partial class AutonomousConversationBudgetControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_AUTONOMOUS_CONVERSATION_BUDGET_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        AutonomousConversationBudgetController controller =
            GetNode<AutonomousConversationBudgetController>(
                "../AutonomousConversationBudgetController");
        AutonomousConversationBudget original = new(0, 2, false);

        if (controller.Budget is not null
            || !RejectsNullInitialization(controller)
            || !controller.TryInitialize(original)
            || !ReferenceEquals(controller.Budget, original)
            || controller.TryInitialize(new AutonomousConversationBudget(0, 1, false))
            || !controller.TryRecordCompletedTurn())
        {
            Fail("Budget controller rejected exact one-time initialization or first completion.");
            return;
        }

        AutonomousConversationBudget afterCompletion = controller.Budget!;
        if (ReferenceEquals(afterCompletion, original)
            || original.CompletedTurnCount != 0
            || afterCompletion.CompletedTurnCount != 1
            || afterCompletion.MaximumTurnCount != 2
            || afterCompletion.HasPlayerInterrupted
            || !controller.TryInterrupt())
        {
            Fail("Budget controller failed immutable completion or interruption.");
            return;
        }

        AutonomousConversationBudget interrupted = controller.Budget!;
        if (ReferenceEquals(interrupted, afterCompletion)
            || !interrupted.HasPlayerInterrupted
            || interrupted.CompletedTurnCount != 1
            || controller.TryInterrupt()
            || controller.TryRecordCompletedTurn()
            || !ReferenceEquals(controller.Budget, interrupted)
            || !HasClosedSurface())
        {
            Fail("Budget controller failed monotonic or fail-closed ownership checks.");
            return;
        }

        GD.Print(
            "Autonomous-conversation budget controller smoke passed: exact one-time ownership, "
            + "immutable completion, monotonic interruption, and fail-closed repeated operations.");
        GetTree().Quit();
    }

    private static bool RejectsNullInitialization(
        AutonomousConversationBudgetController controller)
    {
        try
        {
            _ = controller.TryInitialize(null!);
        }
        catch (ArgumentNullException)
        {
            return controller.Budget is null;
        }

        return false;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(AutonomousConversationBudgetController);
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        return property.Name == "Budget"
            && property.PropertyType == typeof(AutonomousConversationBudget)
            && property.SetMethod?.IsPrivate == true
            && methods.Select(method => method.Name).SequenceEqual(
                ["TryInitialize", "TryInterrupt", "TryRecordCompletedTurn"]);
    }

    private static T AssertSingle<T>(IReadOnlyList<T> entries) => entries.Count == 1
        ? entries[0]
        : throw new InvalidOperationException("Expected exactly one controller property.");

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
