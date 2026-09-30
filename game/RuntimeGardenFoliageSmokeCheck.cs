using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the exact bounded broadleaf presentation in the Healer's Garden.</summary>
public partial class RuntimeGardenFoliageSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_GARDEN_FOLIAGE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string ExpectedTexturePath =
        "res://art/runtime/plants/foliage-broadleaf.png";
    private static readonly Vector2 ExpectedPivotOffset = new(-36.0f, -53.0f);
    private static readonly (string Name, Vector2 LocalPosition)[] ExpectedPlantings =
    [
        ("BroadleafFoliageTopLeft", new Vector2(-68.0f, 1.0f)),
        ("BroadleafFoliageTopMiddle", new Vector2(-16.0f, 1.0f)),
        ("BroadleafFoliageTopRight", new Vector2(36.0f, 1.0f)),
        ("BroadleafFoliageBottomLeft", new Vector2(-68.0f, 49.0f)),
        ("BroadleafFoliageBottomMiddle", new Vector2(-16.0f, 49.0f)),
        ("BroadleafFoliageBottomRight", new Vector2(36.0f, 49.0f)),
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
        Sprite2D table = garden.GetNode<Sprite2D>("PreparationTable");
        foreach ((string name, Vector2 localPosition) in ExpectedPlantings)
        {
            Sprite2D foliage = garden.GetNode<Sprite2D>(name);
            Texture2D? texture = foliage.Texture;
            if (texture is null
                || texture.ResourcePath != ExpectedTexturePath
                || texture.GetWidth() != 72
                || texture.GetHeight() != 56)
            {
                Fail("Runtime garden foliage lost its exact 72x56 texture identity.");
                return;
            }

            if (foliage.Position != localPosition
                || foliage.GlobalPosition != garden.GlobalPosition + localPosition
                || foliage.Centered
                || foliage.Offset != ExpectedPivotOffset
                || !Mathf.IsZeroApprox(foliage.Rotation)
                || foliage.Scale != Vector2.One
                || foliage.RegionEnabled
                || foliage.ZIndex != 0
                || foliage.ShowBehindParent
                || foliage.GetChildCount() != 0
                || foliage.GetIndex() >= table.GetIndex())
            {
                Fail("Runtime garden foliage lost its exact non-authoritative presentation.");
                return;
            }
        }

        GD.Print(
            "Runtime garden foliage smoke passed: six placements of one exact manifest-bound "
            + "72x56 broadleaf sprite retain their planted-foot pivots, cover the existing "
            + "Healer's Garden beneath its preparation table, and add no collision or gameplay "
            + "authority.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
