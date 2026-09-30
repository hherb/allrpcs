using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the bounded manifest-owned Seeker walk presentation.</summary>
public partial class RuntimeSeekerWalkSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_SEEKER_WALK_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const float ExpectedFramesPerSecond = 1000.0f / 120.0f;
    private const float TimingTolerance = 0.001f;
    private static readonly (Vector2 Movement, string Animation, int Width)[] ExpectedDirections =
    [
        (Vector2.Down, "walk_south", 108),
        (Vector2.Left, "walk_west", 144),
        (Vector2.Right, "walk_east", 144),
        (Vector2.Up, "walk_north", 108),
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        RunChecks();
    }

    private void RunChecks()
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        SeekerWalkAnimationPresenter presenter =
            seeker.GetNode<SeekerWalkAnimationPresenter>("WalkAnimation");
        SpriteFrames frames = presenter.SpriteFrames;

        if (seeker.GetNodeOrNull("Body") is not null
            || seeker.GetNodeOrNull("Facing") is not null
            || presenter.Centered is false
            || presenter.Offset != new Vector2(0.0f, -72.0f)
            || presenter.Position != Vector2.Zero
            || presenter.Scale != Vector2.One
            || presenter.Rotation != 0.0f
            || presenter.ZIndex != 0
            || presenter.GetChildCount() != 0
            || frames.GetAnimationNames().Length != ExpectedDirections.Length)
        {
            Fail("Runtime Seeker walk lost its exact non-authoritative presentation.");
            return;
        }

        HashSet<Rid> textureRids = [];
        foreach ((Vector2 movement, string animationName, int width) in ExpectedDirections)
        {
            StringName animation = animationName;
            presenter.PresentMovement(movement);
            if (presenter.Animation != animation
                || !presenter.IsPlaying()
                || frames.GetFrameCount(animation) != 6
                || !frames.GetAnimationLoop(animation)
                || Mathf.Abs((float)frames.GetAnimationSpeed(animation) - ExpectedFramesPerSecond)
                    > TimingTolerance)
            {
                Fail("Runtime Seeker walk lost its four exact 120 ms loops.");
                return;
            }

            for (int index = 0; index < 6; index++)
            {
                Texture2D texture = frames.GetFrameTexture(animation, index);
                if (texture.GetWidth() != width
                    || texture.GetHeight() != 168
                    || !texture.ResourcePath.StartsWith(
                        "res://art/runtime/characters/seeker/",
                        StringComparison.Ordinal)
                    || !textureRids.Add(texture.GetRid()))
                {
                    Fail("Runtime Seeker walk lost its 24 unique runtime textures.");
                    return;
                }
            }
        }

        presenter.PresentMovement(Vector2.Zero);
        if (presenter.IsPlaying() || presenter.Frame != 0 || presenter.Animation != "walk_north")
        {
            Fail("Stationary Seeker did not retain the first frame of the last facing direction.");
            return;
        }

        if (seeker.GetNode<CollisionShape2D>("CollisionShape2D").Shape is not CircleShape2D
            { Radius: 18.0f })
        {
            Fail("Runtime walk presentation changed Seeker collision authority.");
            return;
        }

        GD.Print(
            "Runtime Seeker walk smoke passed: 24 manifest-bound frames form four six-frame "
            + "120 ms loops, retain exact foot pivots, replace only greybox visuals, and leave "
            + "movement and collision authority unchanged.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
