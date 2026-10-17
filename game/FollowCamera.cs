using Allrpcs.Application.Camera;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>
/// Keeps the Seeker camera inside the greybox region with validated zoom and smoothing tuning.
/// </summary>
public partial class FollowCamera : Camera2D
{
    private const float DefaultFollowSmoothingSpeed = 6.0f;
    private const int DefaultWorldLeftLimitPixels = 0;
    private const int DefaultWorldTopLimitPixels = 0;
    private const int DefaultWorldRightLimitPixels = 3200;
    private const int DefaultWorldBottomLimitPixels = 1800;
    private const float DefaultMinimumZoom = 0.8f;
    private const float DefaultStartingZoom = 1.0f;
    private const float DefaultMaximumZoom = 2.0f;

    private Viewport? _viewport;

    /// <summary>
    /// Gets or sets how quickly the camera catches up to the Seeker.
    /// </summary>
    [Export(PropertyHint.Range, "1.0,20.0,0.1,or_greater")]
    public float FollowSmoothingSpeed { get; set; } = DefaultFollowSmoothingSpeed;

    /// <summary>
    /// Gets or sets the left edge of the visible greybox world in pixels.
    /// </summary>
    [Export]
    public int WorldLeftLimitPixels { get; set; } = DefaultWorldLeftLimitPixels;

    /// <summary>
    /// Gets or sets the top edge of the visible greybox world in pixels.
    /// </summary>
    [Export]
    public int WorldTopLimitPixels { get; set; } = DefaultWorldTopLimitPixels;

    /// <summary>
    /// Gets or sets the right edge of the visible greybox world in pixels.
    /// </summary>
    [Export]
    public int WorldRightLimitPixels { get; set; } = DefaultWorldRightLimitPixels;

    /// <summary>
    /// Gets or sets the bottom edge of the visible greybox world in pixels.
    /// </summary>
    [Export]
    public int WorldBottomLimitPixels { get; set; } = DefaultWorldBottomLimitPixels;

    /// <summary>
    /// Gets or sets the furthest supported camera zoom-out value.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,4.0,0.05")]
    public float MinimumZoom { get; set; } = DefaultMinimumZoom;

    /// <summary>
    /// Gets or sets the stable camera zoom used when the scene starts.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,4.0,0.05")]
    public float StartingZoom { get; set; } = DefaultStartingZoom;

    /// <summary>
    /// Gets or sets the nearest supported camera zoom-in value.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,4.0,0.05")]
    public float MaximumZoom { get; set; } = DefaultMaximumZoom;

    /// <inheritdoc />
    public override void _Ready()
    {
        _viewport = GetViewport();
        CameraFramingPlan framing = CreateFraming();

        Position = Vector2.Zero;
        Offset = Vector2.Zero;
        Zoom = new Vector2(framing.DefaultZoom, framing.DefaultZoom);
        LimitLeft = WorldLeftLimitPixels;
        LimitTop = WorldTopLimitPixels;
        LimitRight = WorldRightLimitPixels;
        LimitBottom = WorldBottomLimitPixels;
        LimitEnabled = true;
        LimitSmoothed = true;
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = FollowSmoothingSpeed;
        MakeCurrent();
        ResetSmoothing();
        _viewport.SizeChanged += OnViewportSizeChanged;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (_viewport is not null)
        {
            _viewport.SizeChanged -= OnViewportSizeChanged;
            _viewport = null;
        }
    }

    private void OnViewportSizeChanged()
    {
        CameraFramingPlan framing = CreateFraming();
        float zoom = Mathf.Clamp(Zoom.X, framing.MinimumZoom, framing.MaximumZoom);
        Zoom = new Vector2(zoom, zoom);
        ForceUpdateScroll();
        Vector2 halfView = GetViewportRect().Size / Zoom / 2.0f;
        Vector2 center = GetScreenCenterPosition();
        if (center.X - halfView.X < WorldLeftLimitPixels
            || center.Y - halfView.Y < WorldTopLimitPixels
            || center.X + halfView.X > WorldRightLimitPixels
            || center.Y + halfView.Y > WorldBottomLimitPixels)
        {
            // A newly narrower valid center range must not expose outside space while smoothing catches up.
            ResetSmoothing();
            ForceUpdateScroll();
        }
    }

    private CameraFramingPlan CreateFraming()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        return CameraFramingCalculator.Create(
            new CameraWorldBounds(
                WorldLeftLimitPixels,
                WorldTopLimitPixels,
                WorldRightLimitPixels,
                WorldBottomLimitPixels),
            new NumericsVector2(viewportSize.X, viewportSize.Y),
            MinimumZoom,
            StartingZoom,
            MaximumZoom);
    }
}
