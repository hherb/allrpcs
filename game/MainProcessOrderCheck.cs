using Godot;

namespace Allrpcs.Game;

/// <summary>Instantiates Main and asserts its explicit same-frame processing contract.</summary>
public sealed partial class MainProcessOrderCheck : Node
{
    /// <inheritdoc />
    public override async void _Ready()
    {
        try
        {
            Main main = GD.Load<PackedScene>("res://Main.tscn").Instantiate<Main>();
            AddChild(main);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            // Pins the direction of IsGreaterThan that ProcessesBefore relies on.
            Require(main.GetChild(1).IsGreaterThan(main.GetChild(0)), "Tree-order comparison is inverted");
            Require(main.ProcessPriority == MainProcessOrder.MainPriority, "Main priority is not explicit");
            foreach ((string earlier, string later) in MainProcessOrder.Dependencies)
            {
                Require(
                    ProcessesBefore(main.GetNode(earlier), main.GetNode(later)),
                    $"{earlier} does not process before {later}");
            }

            Require(
                main.GetNode("HerbExaminationActionController").GetIndex() + 1
                    == main.GetNode("WholeChannelPresentation").GetIndex(),
                "The examination action is not immediately before WholeChannelPresentation");
            GD.Print($"PASS: Main process order: priority {main.ProcessPriority}, "
                + $"{MainProcessOrder.Dependencies.Length} dependencies.");
            GetTree().Quit();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            GetTree().Quit(1);
        }
    }

    // Godot processes lower priorities first and breaks ties in tree order.
    private static bool ProcessesBefore(Node first, Node second) =>
        first.ProcessPriority < second.ProcessPriority
        || (first.ProcessPriority == second.ProcessPriority && second.IsGreaterThan(first));

    private static void Require(bool value, string message)
    {
        if (!value)
        {
            throw new InvalidOperationException(message);
        }
    }
}
