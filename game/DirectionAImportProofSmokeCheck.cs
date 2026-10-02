using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the isolated Direction A import proof without touching the ordinary scene.</summary>
public partial class DirectionAImportProofSmokeCheck : Node2D
{
    private const int ExpectedWalkFrameCount = 6;
    private const int ExpectedIdleFrameCount = 2;
    private const int ExpectedInteractionFrameCount = 4;
    private const int ExpectedCharacterFrameHeight = 168;
    private const float ExpectedWalkFramesPerSecond = 1000.0f / 120.0f;
    private const float ExpectedIdleFramesPerSecond = 1000.0f / 600.0f;
    private const float ExpectedInteractionFramesPerSecond = 1000.0f / 160.0f;
    private const float TimingTolerance = 0.001f;

    private static readonly (
        string Name,
        int Width,
        int FrameCount,
        float FramesPerSecond,
        bool Loop)[]
        ExpectedAnimations =
    [
        ("walk_south", 108, ExpectedWalkFrameCount, ExpectedWalkFramesPerSecond, true),
        ("walk_west", 144, ExpectedWalkFrameCount, ExpectedWalkFramesPerSecond, true),
        ("walk_east", 144, ExpectedWalkFrameCount, ExpectedWalkFramesPerSecond, true),
        ("walk_north", 108, ExpectedWalkFrameCount, ExpectedWalkFramesPerSecond, true),
        ("idle_south", 108, ExpectedIdleFrameCount, ExpectedIdleFramesPerSecond, true),
        ("idle_west", 144, ExpectedIdleFrameCount, ExpectedIdleFramesPerSecond, true),
        ("idle_east", 144, ExpectedIdleFrameCount, ExpectedIdleFramesPerSecond, true),
        ("idle_north", 108, ExpectedIdleFrameCount, ExpectedIdleFramesPerSecond, true),
        (
            "interact_south",
            192,
            ExpectedInteractionFrameCount,
            ExpectedInteractionFramesPerSecond,
            false),
        (
            "interact_west",
            192,
            ExpectedInteractionFrameCount,
            ExpectedInteractionFramesPerSecond,
            false),
        (
            "interact_east",
            192,
            ExpectedInteractionFrameCount,
            ExpectedInteractionFramesPerSecond,
            false),
        (
            "interact_north",
            192,
            ExpectedInteractionFrameCount,
            ExpectedInteractionFramesPerSecond,
            false),
    ];

    private static readonly (string Path, int Width, int Height)[] ExpectedSprites =
    [
        ("Terrain/Ground00", 256, 256),
        ("Terrain/Path00", 96, 256),
        ("Terrain/PathBendProof", 256, 256),
        ("Terrain/PathTJunctionProof", 256, 256),
        ("Terrain/PathCrossJunctionProof", 256, 256),
        ("Terrain/PathEndCapProof", 256, 256),
        ("Terrain/OpeningJunctionProof", 512, 512),
        ("Terrain/VillageSquareJunctionProof", 512, 512),
        ("Terrain/VillageEdgeSquareConnectorProof", 124, 96),
        ("Terrain/SquareGardenPathProof", 200, 220),
        ("Terrain/SquareSickhousePathProof", 380, 360),
        ("Terrain/SquareSouthernRoadPathProof", 520, 416),
        ("Terrain/SouthernRoadWagonPathProof", 560, 432),
        ("Terrain/ShrineSouthernRoadPathProof", 1152, 1080),
        ("Terrain/ShrineSouthernRoadEndpointBlendProof", 160, 160),
        ("Terrain/StreamWaterProof", 128, 96),
        ("Terrain/StreamBankProof", 128, 32),
        ("Terrain/StreamBankOpeningCapProof", 32, 32),
        ("Terrain/WoodlandLeafLitterProof", 256, 256),
        ("Terrain/WoodlandLeafLitterTransitionProof", 256, 256),
        ("Terrain/WoodlandLeafLitterCornerTransitionProof", 256, 256),
        ("Terrain/WoodlandLeafLitterLeftCornerTransitionProof", 256, 256),
        ("Terrain/WoodlandLeafLitterInsetCornerTransitionProof", 256, 256),
        ("Terrain/WoodlandLeafLitterLeftInsetCornerTransitionProof", 256, 256),
        ("Terrain/WoodlandVillageEdgeTransitionProof", 1024, 512),
        ("Props/EmptyBed", 96, 80),
        ("Props/PlantedBed", 96, 80),
        ("Props/PlantedBedFoliage", 72, 56),
        ("Props/PreparationTable", 96, 96),
        ("Props/PreparationTableIdentifiedCue", 96, 96),
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        if (TextureFilter != TextureFilterEnum.Linear
            || !HasExpectedSprites()
            || !HasNormalizedAnimations()
            || !KeepsPresentationInert())
        {
            Fail("Direction A import proof lost filtering, geometry, animation, or inertness.");
            return;
        }

        GD.Print(
            "Direction A import proof passed: lossless source-parity textures, linear filtering, "
            + "straight, 90-degree, T-junction, cross-junction, end-cap, and exact live-angle "
            + "opening- and Village-Square-junction path modules, "
            + "repeatable stream water, mirrored bank edge, and reusable opening cap, "
            + "repeatable woodland leaf litter and its straight, outer-corner, and two inset-corner "
            + "quiet-moss transitions plus the softly feathered Village Edge overlay, "
            + "separate table cue, "
            + "144px-minimum visible Seeker, four six-frame 120 ms walks, "
            + "four two-frame 600 ms idle loops, "
            + "four four-frame 160 ms non-looping interactions, 48 unique character textures, "
            + "and no gameplay authority.");
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
        Sprite2D pathSprite = GetNode<Sprite2D>("Terrain/Path00");
        return pathSprite.Position == new Vector2(592, 0)
            && table.Position == cue.Position
            && table.Texture != cue.Texture
            && cue.ZIndex > table.ZIndex;
    }

    private bool HasNormalizedAnimations()
    {
        AnimatedSprite2D seeker = GetNode<AnimatedSprite2D>("Characters/Seeker");
        SpriteFrames frames = seeker.SpriteFrames;
        if (seeker.Centered
            || seeker.Offset != Vector2.Zero
            || seeker.Position != new Vector2(586, 276)
            || frames.GetAnimationNames().Length != ExpectedAnimations.Length)
        {
            return false;
        }

        HashSet<Rid> textureRids = [];
        foreach ((string name, int width, int frameCount, float framesPerSecond, bool loop)
            in ExpectedAnimations)
        {
            StringName animation = name;
            seeker.Play(animation);
            if (seeker.Animation != animation
                || !frames.HasAnimation(animation)
                || frames.GetFrameCount(animation) != frameCount
                || frames.GetAnimationLoop(animation) != loop
                || Mathf.Abs((float)frames.GetAnimationSpeed(animation) - framesPerSecond)
                    > TimingTolerance)
            {
                return false;
            }

            for (int index = 0; index < frameCount; index++)
            {
                Texture2D texture = frames.GetFrameTexture(animation, index);
                if (texture.GetWidth() != width
                    || texture.GetHeight() != ExpectedCharacterFrameHeight
                    || Mathf.Abs(frames.GetFrameDuration(animation, index) - 1.0f)
                        > TimingTolerance
                    || !textureRids.Add(texture.GetRid()))
                {
                    return false;
                }
            }
        }

        return seeker.IsPlaying();
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
