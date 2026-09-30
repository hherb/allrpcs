using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the exact bounded preparation-table presentation in the Healer's Garden.</summary>
public partial class RuntimePreparationTableSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_RUNTIME_PREPARATION_TABLE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ExpectedTexturePath =
        "res://art/runtime/objects/preparation-table.png";
    private static readonly Vector2 ExpectedLocalPosition = new(0.0f, 18.0f);
    private static readonly Vector2 ExpectedPivotOffset = new(-48.0f, -92.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        RunChecks();
    }

    private void RunChecks()
    {
        Node2D garden = GetNode<Node2D>("../OpeningRegionRemedyPreparationSceneSource");
        Sprite2D? table = garden.GetNodeOrNull<Sprite2D>("PreparationTable");
        if (table is null)
        {
            Fail("Runtime preparation table is absent from the Healer's Garden.");
            return;
        }

        Texture2D? texture = table.Texture;
        Node foliage = garden.GetNode("BroadleafFoliageBottomRight");
        Node label = garden.GetNode("Label");
        if (texture is null
            || texture.ResourcePath != ExpectedTexturePath
            || texture.GetWidth() != 96
            || texture.GetHeight() != 96)
        {
            Fail("Runtime preparation table lost its exact 96x96 texture identity.");
            return;
        }

        if (table.Position != ExpectedLocalPosition
            || table.GlobalPosition != garden.GlobalPosition + ExpectedLocalPosition
            || table.Centered
            || table.Offset != ExpectedPivotOffset
            || !Mathf.IsZeroApprox(table.Rotation)
            || table.Scale != Vector2.One
            || table.RegionEnabled
            || table.ZIndex != 0
            || table.ShowBehindParent
            || table.GetChildCount() != 0
            || table.GetIndex() <= foliage.GetIndex()
            || table.GetIndex() >= label.GetIndex()
            || garden.HasNode("PreparationBench")
            || garden.HasNode("Mortar"))
        {
            Fail("Runtime preparation table lost its exact non-authoritative presentation.");
            return;
        }

        GD.Print(
            "Runtime preparation-table smoke passed: one exact manifest-bound 96x96 table "
            + "retains its planted-foot pivot at the existing station, draws above the accepted "
            + "garden foliage, and adds no collision or gameplay authority.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
