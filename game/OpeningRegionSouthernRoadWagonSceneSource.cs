using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the collision-aware southern road and one inert abandoned-wagon marker.</summary>
public partial class OpeningRegionSouthernRoadWagonSceneSource : Node2D
{
    /// <summary>Gets the exact road/wagon binding after scene sources become ready.</summary>
    public OpeningRegionSouthernRoadWagonSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware road/wagon layout after scene sources become ready.</summary>
    public OpeningRegionSouthernRoadWagonLayout Layout { get; private set; } = null!;

    /// <summary>Gets the exact inert wagon marker.</summary>
    public Marker2D AbandonedWagonMarker { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionOldShrineLayout shrineLayout =
            GetNode<OpeningRegionOldShrineSceneSource>(
                "../OpeningRegionOldShrineSceneSource").Layout;
        OpeningRegionAnchor road = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "southern-road");
        OpeningRegionAnchor wagon = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "abandoned-wagon");
        Binding = new OpeningRegionSouthernRoadWagonSceneBinding(
            shrineLayout,
            road,
            wagon,
            OpeningRegionSouthernRoadWagonGeometry.WagonBounds);
        Layout = new OpeningRegionSouthernRoadWagonLayout(
            Binding,
            OpeningRegionSouthernRoadWagonGeometry.RoadHalfWidthPixels,
            OpeningRegionSouthernRoadWagonGeometry.BoundaryThicknessPixels,
            OpeningRegionSouthernRoadWagonGeometry.WagonOpeningWidthPixels,
            OpeningRegionSouthernRoadWagonGeometry.WagonOpening);
        AbandonedWagonMarker = GetNode<Marker2D>("AbandonedWagonMarker");
        if (ToNumerics(AbandonedWagonMarker.Position) != wagon.Position)
        {
            throw new InvalidOperationException(
                "The inert wagon marker must remain at the exact projected wagon anchor.");
        }

        ConfigureBoundaries(
            GetNode<Node2D>("RoadBoundaries"),
            Layout.RoadBoundarySegments,
            "RoadBoundary");
        ConfigureBoundaries(
            GetNode<Node2D>("WagonBoundaries"),
            Layout.WagonBoundarySegments,
            "WagonBoundary");
    }

    private static void ConfigureBoundaries(
        Node2D owner,
        IReadOnlyList<OpeningRegionBoundarySegment> segments,
        string expectedPrefix)
    {
        if (owner.GetChildCount() != segments.Count)
        {
            throw new InvalidOperationException("The authored boundary count must match the layout.");
        }

        for (int index = 0; index < segments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            if (!body.Name.ToString().StartsWith(expectedPrefix, StringComparison.Ordinal)
                || body.GetChildCount() != 2
                || body.GetChild(0) is not CollisionShape2D collision
                || body.GetChild(1) is not Line2D line)
            {
                throw new InvalidOperationException(
                    "Each authored boundary must contain one collision shape and one line.");
            }

            OpeningRegionBoundarySegment segment = segments[index];
            collision.Shape = new SegmentShape2D
            {
                A = ToGodot(segment.Start),
                B = ToGodot(segment.End),
            };
            line.Points = [ToGodot(segment.Start), ToGodot(segment.End)];
            line.Width = OpeningRegionSouthernRoadWagonGeometry.BoundaryThicknessPixels;
        }
    }

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);
}
