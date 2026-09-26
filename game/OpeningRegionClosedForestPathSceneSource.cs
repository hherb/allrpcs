using Allrpcs.Application.Navigation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the collision-backed approach and visibly closed deeper-forest gate.</summary>
public partial class OpeningRegionClosedForestPathSceneSource : Node2D
{
    /// <summary>Gets the exact clearing/path binding after scene sources become ready.</summary>
    public OpeningRegionClosedForestPathSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware closed-path layout after scene sources become ready.</summary>
    public OpeningRegionClosedForestPathLayout Layout { get; private set; } = null!;

    /// <summary>Gets the inert marker at the visibly closed gate.</summary>
    public Marker2D ClosedGateMarker { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionWoodlandClearingLayout clearingLayout =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource").Layout;
        OpeningRegionAnchor closedPath = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "closed-deeper-forest-path");
        Binding = new OpeningRegionClosedForestPathSceneBinding(clearingLayout, closedPath);
        Layout = new OpeningRegionClosedForestPathLayout(
            Binding,
            OpeningRegionClosedForestPathGeometry.ApproachHalfWidthPixels,
            OpeningRegionClosedForestPathGeometry.BoundaryThicknessPixels,
            OpeningRegionClosedForestPathGeometry.ApproachLengthPixels,
            OpeningRegionClosedForestPathGeometry.GateCenter);
        ClosedGateMarker = GetNode<Marker2D>("ClosedGateMarker");
        if (ToNumerics(ClosedGateMarker.Position) != Layout.GateCenter)
        {
            throw new InvalidOperationException(
                "The closed-gate marker must remain at the exact authored gate center.");
        }

        ConfigureApproachBoundaries();
        ConfigureBoundary(GetNode<StaticBody2D>("ClosedGate"), Layout.GateBoundary);
    }

    private void ConfigureApproachBoundaries()
    {
        Node2D owner = GetNode<Node2D>("ApproachBoundaries");
        if (owner.GetChildCount() != Layout.ApproachBoundarySegments.Count)
        {
            throw new InvalidOperationException(
                "The authored approach-boundary count must match the layout.");
        }

        for (int index = 0; index < Layout.ApproachBoundarySegments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            if (body.Name.ToString() != $"ApproachBoundary{index + 1}")
            {
                throw new InvalidOperationException(
                    "Approach boundaries must retain their stable authored order.");
            }

            ConfigureBoundary(body, Layout.ApproachBoundarySegments[index]);
        }
    }

    private static void ConfigureBoundary(
        StaticBody2D body,
        OpeningRegionBoundarySegment segment)
    {
        if (body.GetChildCount() != 2
            || body.GetChild(0) is not CollisionShape2D collision
            || body.GetChild(1) is not Line2D line)
        {
            throw new InvalidOperationException(
                "Each closed-path boundary must contain one collision shape and one line.");
        }

        collision.Shape = new SegmentShape2D
        {
            A = ToGodot(segment.Start),
            B = ToGodot(segment.End),
        };
        line.Points = [ToGodot(segment.Start), ToGodot(segment.End)];
        line.Width = OpeningRegionClosedForestPathGeometry.BoundaryThicknessPixels;
    }

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);
}
