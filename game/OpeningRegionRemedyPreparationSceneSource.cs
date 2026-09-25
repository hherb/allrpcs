using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the temporary greybox healer-garden remedy-preparation station.</summary>
public partial class OpeningRegionRemedyPreparationSceneSource : Node2D
{
    private const string HealerGardenLocationId = "healer-garden";

    /// <summary>Gets the largest garden-anchor-to-station distance in pixels.</summary>
    public const float MaximumStationDistancePixels = 72.0f;

    /// <summary>Gets the exact garden and recipe binding after scene sources become ready.</summary>
    public OpeningRegionRemedyPreparationSceneBinding? Binding { get; private set; }

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
        OpeningRegionAnchor gardenAnchor = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == HealerGardenLocationId);
        Binding = new OpeningRegionRemedyPreparationSceneBinding(
            projection,
            gardenAnchor,
            context,
            ToNumerics(Position),
            MaximumStationDistancePixels);
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
