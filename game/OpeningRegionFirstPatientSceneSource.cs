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

    /// <summary>Gets the exact patient-scene binding after scene sources become ready.</summary>
    public OpeningRegionFirstPatientSceneBinding? Binding { get; private set; }

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
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
