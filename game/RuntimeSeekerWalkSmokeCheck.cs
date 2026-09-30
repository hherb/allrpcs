using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the bounded manifest-owned Seeker walk and idle presentation.</summary>
public partial class RuntimeSeekerWalkSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_RUNTIME_SEEKER_WALK_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const float ExpectedWalkFramesPerSecond = 1000.0f / 120.0f;
    private const float ExpectedIdleFramesPerSecond = 1000.0f / 600.0f;
    private const float ExpectedInteractionFramesPerSecond = 1000.0f / 160.0f;
    private const float TimingTolerance = 0.001f;
    private static readonly (Vector2 Movement, string Walk, string Idle, string Interaction, int Width)[]
        ExpectedDirections =
    [
        (Vector2.Down, "walk_south", "idle_south", "interact_south", 108),
        (Vector2.Left, "walk_west", "idle_west", "interact_west", 144),
        (Vector2.Right, "walk_east", "idle_east", "interact_east", 144),
        (Vector2.Up, "walk_north", "idle_north", "interact_north", 108),
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        GetTree().ProcessFrame += RunChecks;
    }

    private void RunChecks()
    {
        GetTree().ProcessFrame -= RunChecks;
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
            || presenter.GetChildCount() != 0)
        {
            Fail("Runtime Seeker animation lost its exact non-authoritative presentation.");
            return;
        }

        if (frames.GetAnimationNames().Length != ExpectedDirections.Length * 3)
        {
            Fail(
                "Runtime Seeker animation resource must contain exactly four walk, idle, and "
                + "interaction animations, but contained "
                + $"{frames.GetAnimationNames().Length} animations.");
            return;
        }

        HashSet<Rid> textureRids = [];
        presenter.PresentMovement(Vector2.Zero);
        if (presenter.Animation != "idle_south" || !presenter.IsPlaying())
        {
            Fail("Stationary Seeker did not start with the south-facing idle loop.");
            return;
        }

        foreach (
            (Vector2 movement, string walkName, string idleName, string interactionName, int width)
            in ExpectedDirections)
        {
            StringName walk = walkName;
            presenter.PresentMovement(movement);
            if (presenter.Animation != walk
                || !presenter.IsPlaying()
                || frames.GetFrameCount(walk) != 6
                || !frames.GetAnimationLoop(walk)
                || Mathf.Abs((float)frames.GetAnimationSpeed(walk) - ExpectedWalkFramesPerSecond)
                    > TimingTolerance)
            {
                Fail("Runtime Seeker walk lost its four exact 120 ms loops.");
                return;
            }

            for (int index = 0; index < 6; index++)
            {
                Texture2D texture = frames.GetFrameTexture(walk, index);
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

            StringName idle = idleName;
            presenter.PresentMovement(Vector2.Zero);
            if (presenter.Animation != idle
                || !presenter.IsPlaying()
                || frames.GetFrameCount(idle) != 2
                || !frames.GetAnimationLoop(idle)
                || Mathf.Abs((float)frames.GetAnimationSpeed(idle) - ExpectedIdleFramesPerSecond)
                    > TimingTolerance)
            {
                Fail("Stationary Seeker did not play the matching two-frame 600 ms idle loop.");
                return;
            }

            for (int index = 0; index < 2; index++)
            {
                Texture2D texture = frames.GetFrameTexture(idle, index);
                if (texture.GetWidth() != width
                    || texture.GetHeight() != 168
                    || !texture.ResourcePath.StartsWith(
                        "res://art/runtime/characters/seeker/",
                        StringComparison.Ordinal)
                    || !textureRids.Add(texture.GetRid()))
                {
                    Fail("Runtime Seeker idle lost its eight unique runtime textures.");
                    return;
                }
            }

            StringName interaction = interactionName;
            presenter.PresentSuccessfulInteraction();
            if (presenter.Animation != interaction
                || !presenter.IsPlaying()
                || frames.GetFrameCount(interaction) != 4
                || frames.GetAnimationLoop(interaction)
                || Mathf.Abs(
                    (float)frames.GetAnimationSpeed(interaction)
                    - ExpectedInteractionFramesPerSecond) > TimingTolerance)
            {
                Fail("Seeker did not play the matching four-frame 160 ms interaction gesture.");
                return;
            }

            for (int index = 0; index < 4; index++)
            {
                Texture2D texture = frames.GetFrameTexture(interaction, index);
                if (texture.GetWidth() != 192
                    || texture.GetHeight() != 168
                    || !texture.ResourcePath.StartsWith(
                        "res://art/runtime/characters/seeker/",
                        StringComparison.Ordinal)
                    || !textureRids.Add(texture.GetRid()))
                {
                    Fail("Runtime Seeker interaction lost its 16 unique runtime textures.");
                    return;
                }
            }

            presenter.EmitSignal(AnimatedSprite2D.SignalName.AnimationFinished);
            if (presenter.Animation != idle || !presenter.IsPlaying())
            {
                Fail("Completed Seeker interaction did not return to its matching idle loop.");
                return;
            }

            presenter.PresentSuccessfulInteraction();
            presenter.PresentMovement(movement);
            if (presenter.Animation != walk || !presenter.IsPlaying())
            {
                Fail("Seeker movement did not interrupt the interaction gesture.");
                return;
            }
        }

        if (seeker.GetNode<CollisionShape2D>("CollisionShape2D").Shape is not CircleShape2D
            { Radius: 18.0f })
        {
            Fail("Runtime walk presentation changed Seeker collision authority.");
            return;
        }

        GD.Print(
            "Runtime Seeker animation smoke passed: 24 walk, eight idle/breathe, and 16 interaction "
            + "frames form direction-matched 120 ms walk, 600 ms idle, and interruptible 160 ms "
            + "non-looping interaction animations while leaving movement and collision authority "
            + "unchanged.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
