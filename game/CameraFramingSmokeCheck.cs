using Godot;

namespace Allrpcs.Game;

/// <summary>Checks real viewport resize signals and portrait camera coverage without Main.</summary>
public partial class CameraFramingSmokeCheck : Node
{
    private SubViewport _viewport = null!;
    private FollowCamera _camera = null!;
    private SubViewport _portraitViewport = null!;
    private FollowCamera _portraitCamera = null!;
    private int _stage;
    private int _frames;

    /// <inheritdoc />
    public override void _Ready()
    {
        _viewport = new SubViewport
        {
            Size = new Vector2I(1280, 720),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        AddChild(_viewport);
        _camera = CreateCamera();
        _viewport.AddChild(_camera);
        _portraitViewport = new SubViewport
        {
            Size = new Vector2I(1280, 2275),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        AddChild(_portraitViewport);
        _portraitCamera = CreateCamera();
        _portraitViewport.AddChild(_portraitCamera);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        try
        {
            RunStep();
        }
        catch (InvalidOperationException exception)
        {
            GD.PushError(exception.Message);
            SetProcess(false);
            GetTree().Quit(exitCode: 1);
        }
    }

    private void RunStep()
    {
        _frames++;
        if (_frames < 3)
        {
            return;
        }

        _frames = 0;
        switch (_stage++)
        {
            case 0:
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 1.0f), "Landscape startup default");
                _camera.Zoom = new Vector2(0.9f, 0.9f);
                _viewport.Size = new Vector2I(1280, 2275);
                break;
            case 1:
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 2275.0f / 900.0f), "Portrait resize coverage");
                CheckCoverage();
                Require(_camera.PositionSmoothingEnabled && _camera.LimitSmoothed, "Smoothing remains enabled");
                _camera.Position = new Vector2(800, 450);
                _viewport.Size = new Vector2I(1280, 720);
                break;
            case 2:
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 2.0f), "Return clamps current zoom to maximum");
                Require(_camera.Position == new Vector2(800, 450), "Resize preserves follow position");
                CheckCoverage();
                _camera.Position = new Vector2(900, 450);
                _camera.Zoom = new Vector2(1.5f, 1.5f);
                _viewport.Size = new Vector2I(1440, 810);
                break;
            case 3:
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 1.5f), "Valid current zoom is preserved");
                Vector2 movingCenter = _camera.GetScreenCenterPosition();
                Require(movingCenter.X > 800 && movingCenter.X < 900,
                    "Smoothing still advances toward follow position after valid resize");
                _viewport.RemoveChild(_camera);
                _viewport.Size = new Vector2I(1280, 2275);
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 1.5f), "Exit unsubscribes viewport");
                _camera.Free();
                _camera = _portraitCamera;
                _viewport = _portraitViewport;
                break;
            default:
                Require(Mathf.IsEqualApprox(_camera.Zoom.X, 2275.0f / 900.0f), "Portrait startup covers world");
                CheckCoverage();
                Require(float.IsFinite(_camera.GetScreenCenterPosition().X), "Smoothing center remains finite");
                GD.Print("Camera framing smoke passed: landscape, portrait resize, return, preserved zoom, exit and portrait startup.");
                GetTree().Quit();
                break;
        }
    }

    private static FollowCamera CreateCamera() => new()
    {
        WorldRightLimitPixels = 1600,
        WorldBottomLimitPixels = 900,
    };

    private void CheckCoverage()
    {
        Require(float.IsFinite(_camera.Zoom.X) && _camera.Zoom.X > 0, "Finite positive zoom");
        Require(_viewport.Size.X / _camera.Zoom.X <= 1600.01f, "Visible width stays in world");
        Require(_viewport.Size.Y / _camera.Zoom.Y <= 900.01f, "Visible height stays in world");
        Vector2 center = _camera.GetScreenCenterPosition();
        Vector2 halfView = (Vector2)_viewport.Size / _camera.Zoom / 2.0f;
        Require(center.X - halfView.X >= -0.01f && center.X + halfView.X <= 1600.01f,
            $"Rendered horizontal edges stay in world: stage {_stage}, center {center}, half {halfView}");
        Require(center.Y - halfView.Y >= -0.01f && center.Y + halfView.Y <= 900.01f,
            $"Rendered vertical edges stay in world: stage {_stage}, center {center}, half {halfView}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
