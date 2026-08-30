using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Provides a stable initial frame and named smoothing tuning for the Seeker camera.
/// </summary>
public partial class FollowCamera : Camera2D
{
    private const float DefaultFollowSmoothingSpeed = 6.0f;

    /// <summary>
    /// Gets or sets how quickly the camera catches up to the Seeker.
    /// </summary>
    [Export(PropertyHint.Range, "1.0,20.0,0.1,or_greater")]
    public float FollowSmoothingSpeed { get; set; } = DefaultFollowSmoothingSpeed;

    /// <inheritdoc />
    public override void _Ready()
    {
        Position = Vector2.Zero;
        Zoom = Vector2.One;
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = FollowSmoothingSpeed;
        MakeCurrent();
        ResetSmoothing();
    }
}
