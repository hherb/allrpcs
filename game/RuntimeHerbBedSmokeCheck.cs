using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the exact bounded raised-bed presentation in the Healer's Garden.</summary>
public partial class RuntimeHerbBedSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_HERB_BED_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ExpectedTexturePath = "res://art/runtime/plants/herb-bed-empty.png";
    private static readonly Vector2 ExpectedPivotOffset = new(-48.0f, -77.0f);
    private static readonly (string Name, Vector2 LocalPosition)[] ExpectedBeds =
    [
        ("HerbBedLeft", new Vector2(-56.0f, 52.0f)),
        ("HerbBedRight", new Vector2(36.0f, 52.0f)),
    ];

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
        Node firstFoliage = garden.GetNode("BroadleafFoliageTopLeft");
        int runtimeBedCount = 0;
        foreach (Node child in garden.GetChildren())
        {
            if (child is Sprite2D { Texture.ResourcePath: ExpectedTexturePath })
            {
                runtimeBedCount++;
            }
        }

        if (runtimeBedCount != ExpectedBeds.Length)
        {
            Fail("The Healer's Garden must contain exactly two runtime herb beds.");
            return;
        }

        foreach ((string name, Vector2 localPosition) in ExpectedBeds)
        {
            Sprite2D? bed = garden.GetNodeOrNull<Sprite2D>(name);
            Texture2D? texture = bed?.Texture;
            if (bed is null
                || texture is null
                || texture.ResourcePath != ExpectedTexturePath
                || texture.GetWidth() != 96
                || texture.GetHeight() != 80)
            {
                Fail("Runtime herb beds lost their exact 96x80 texture identity.");
                return;
            }

            if (bed.Position != localPosition
                || bed.GlobalPosition != garden.GlobalPosition + localPosition
                || bed.Centered
                || bed.Offset != ExpectedPivotOffset
                || !Mathf.IsZeroApprox(bed.Rotation)
                || bed.Scale != Vector2.One
                || bed.RegionEnabled
                || bed.ZIndex != 0
                || bed.ShowBehindParent
                || bed.GetChildCount() != 0
                || bed.GetIndex() >= firstFoliage.GetIndex())
            {
                Fail("Runtime herb beds lost their exact non-authoritative presentation.");
                return;
            }
        }

        if (garden.HasNode("GardenPlot") || garden.HasNode("Beds"))
        {
            Fail("Runtime herb beds did not replace the greybox garden visuals.");
            return;
        }

        GD.Print(
            "Runtime herb-bed smoke passed: two placements of one exact manifest-bound 96x80 "
            + "raised-bed sprite retain their planted-foot pivots beneath the six manifest-bound "
            + "broadleaf plantings and add no collision or gameplay authority.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
