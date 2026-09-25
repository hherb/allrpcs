using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the temporary greybox placement of the exact first patient at the sickhouse.</summary>
public partial class OpeningRegionFirstPatientSceneSource : Node2D
{
    /// <summary>Gets the largest patient-to-sickhouse-anchor distance in pixels.</summary>
    public const float MaximumPatientDistancePixels = 96.0f;

    /// <summary>Gets half the sickhouse width in pixels.</summary>
    public const float RoomHalfWidthPixels = 96.0f;

    /// <summary>Gets the room's north-wall offset from the sickhouse anchor.</summary>
    public const float RoomTopOffsetPixels = 140.0f;

    /// <summary>Gets the room's south-wall offset from the sickhouse anchor.</summary>
    public const float RoomBottomOffsetPixels = 52.0f;

    /// <summary>Gets the single north entrance width in pixels.</summary>
    public const float EntranceWidthPixels = 64.0f;

    /// <summary>Gets the physical wall thickness in pixels.</summary>
    public const float WallThicknessPixels = 8.0f;

    /// <summary>Gets the exterior approach distance north of the entrance.</summary>
    public const float ExteriorApproachDistancePixels = 40.0f;

    /// <summary>Gets the exact patient-scene binding after scene sources become ready.</summary>
    public OpeningRegionFirstPatientSceneBinding? Binding { get; private set; }

    /// <summary>Gets the collision-aware room layout after scene sources become ready.</summary>
    public OpeningRegionSickhouseLayout? Layout { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        FirstPatientTreatmentContext context =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource").Context;
        OpeningRegionAnchor sickhouseAnchor = projection.Anchors.Single(anchor =>
            ReferenceEquals(anchor.Location, projection.Topology.EntryLocation));
        Binding = new OpeningRegionFirstPatientSceneBinding(
            projection,
            sickhouseAnchor,
            context,
            ToNumerics(Position),
            MaximumPatientDistancePixels);
        NumericsVector2 anchorPosition = sickhouseAnchor.Position;
        NavigationWorldBounds roomBounds = new(
            anchorPosition.X - RoomHalfWidthPixels,
            anchorPosition.Y - RoomTopOffsetPixels,
            anchorPosition.X + RoomHalfWidthPixels,
            anchorPosition.Y + RoomBottomOffsetPixels);
        NumericsVector2 entranceCenter = new(anchorPosition.X, roomBounds.Top);
        Layout = new OpeningRegionSickhouseLayout(
            Binding,
            roomBounds,
            entranceCenter,
            EntranceWidthPixels,
            WallThicknessPixels,
            entranceCenter - new NumericsVector2(0.0f, ExteriorApproachDistancePixels),
            anchorPosition);
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
