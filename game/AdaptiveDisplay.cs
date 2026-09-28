using Godot;

namespace Allrpcs.Game;

/// <summary>Applies the minimum supported window size without constraining larger or fullscreen views.</summary>
public partial class AdaptiveDisplay : Node
{
    /// <summary>Gets the minimum supported physical window width.</summary>
    public const int MinimumWindowWidthPixels = 1280;

    /// <summary>Gets the minimum supported physical window height.</summary>
    public const int MinimumWindowHeightPixels = 720;

    /// <inheritdoc />
    public override void _Ready()
    {
        GetWindow().MinSize = new Vector2I(
            MinimumWindowWidthPixels,
            MinimumWindowHeightPixels);
    }
}
