using Godot;

namespace Allrpcs.Game;

/// <summary>Checks minimum-size, resize, and aspect-expanding display configuration when enabled.</summary>
public partial class AdaptiveDisplaySmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ADAPTIVE_DISPLAY_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        Window window = GetWindow();
        Vector2I expectedMinimum = new(
            AdaptiveDisplay.MinimumWindowWidthPixels,
            AdaptiveDisplay.MinimumWindowHeightPixels);
        if (window.MinSize != expectedMinimum
            || window.ContentScaleSize != expectedMinimum
            || window.ContentScaleMode != Window.ContentScaleModeEnum.CanvasItems
            || window.ContentScaleAspect != Window.ContentScaleAspectEnum.Expand
            || window.Unresizable)
        {
            GD.PushError(
                "Adaptive display configuration did not retain its minimum, resize, or "
                + "aspect-expanding contract.");
            GetTree().Quit(exitCode: 1);
            return;
        }

        GD.Print(
            "Adaptive display smoke passed: 960x540 minimum, resizable window, canvas-item "
            + "scaling, and aspect expansion support larger and fullscreen displays.");
        GetTree().Quit();
    }
}
