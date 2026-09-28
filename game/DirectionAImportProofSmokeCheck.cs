using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the isolated Direction A import proof without touching the ordinary scene.</summary>
public partial class DirectionAImportProofSmokeCheck : Node2D
{
    private const string WalkAnimation = "walk_south";
    private const int ExpectedFrameCount = 6;
    private const float ExpectedFramesPerSecond = 1000.0f / 120.0f;
    private const float TimingTolerance = 0.001f;

    private static readonly (string Path, int Width, int Height)[] ExpectedSprites =
    [
        ("Terrain/Ground00", 256, 256),
        ("Terrain/Path00", 96, 256),
        ("Props/EmptyBed", 96, 80),
        ("Props/PlantedBed", 96, 80),
        ("Props/PlantedBedFoliage", 72, 56),
        ("Props/PreparationTable", 80, 80),
        ("Props/PreparationTableIdentifiedCue", 80, 80),
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        if (TextureFilter != TextureFilterEnum.Linear
            || !HasExpectedSprites()
            || !HasNormalizedWalk()
            || !KeepsPresentationInert())
        {
            Fail("Direction A import proof lost filtering, geometry, animation, or inertness.");
            return;
        }

        GD.Print(
            "Direction A import proof passed: lossless source-parity textures, linear filtering, "
            + "separate table cue, six 120 ms south-walk frames, and no gameplay authority.");
        GetTree().Quit();
    }

    private bool HasExpectedSprites()
    {
        foreach ((string path, int width, int height) in ExpectedSprites)
        {
            Sprite2D sprite = GetNode<Sprite2D>(path);
            if (sprite.Texture is not { } texture
                || texture.GetWidth() != width
                || texture.GetHeight() != height
                || sprite.Centered)
            {
                return false;
            }
        }

        Sprite2D table = GetNode<Sprite2D>("Props/PreparationTable");
        Sprite2D cue = GetNode<Sprite2D>("Props/PreparationTableIdentifiedCue");
        return table.Position == cue.Position
            && table.Texture != cue.Texture
            && cue.ZIndex > table.ZIndex;
    }

    private bool HasNormalizedWalk()
    {
        AnimatedSprite2D seeker = GetNode<AnimatedSprite2D>("Characters/Seeker");
        SpriteFrames frames = seeker.SpriteFrames;
        StringName animation = WalkAnimation;
        if (seeker.Centered
            || seeker.Animation != animation
            || !frames.HasAnimation(animation)
            || frames.GetFrameCount(animation) != ExpectedFrameCount
            || !frames.GetAnimationLoop(animation)
            || Mathf.Abs((float)frames.GetAnimationSpeed(animation) - ExpectedFramesPerSecond)
                > TimingTolerance)
        {
            return false;
        }

        HashSet<Rid> textureRids = [];
        for (int index = 0; index < ExpectedFrameCount; index++)
        {
            Texture2D texture = frames.GetFrameTexture(animation, index);
            if (texture.GetWidth() != 36
                || texture.GetHeight() != 56
                || !textureRids.Add(texture.GetRid()))
            {
                return false;
            }
        }

        return true;
    }

    private bool KeepsPresentationInert()
    {
        foreach (Node node in FindChildren("*", owned: false))
        {
            if (node is CollisionObject2D
                or CollisionPolygon2D
                or CollisionShape2D
                or NavigationRegion2D
                or NavigationObstacle2D
                or Control)
            {
                return false;
            }
        }

        return true;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
