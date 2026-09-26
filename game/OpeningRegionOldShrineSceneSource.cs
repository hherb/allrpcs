using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the inert old-shrine shell and concealed layered inscription.</summary>
public partial class OpeningRegionOldShrineSceneSource : Node2D
{
    /// <summary>Gets the shared width of both authored shrine openings.</summary>
    public const float OpeningWidthPixels = OpeningRegionOldShrineGeometry.OpeningWidthPixels;

    /// <summary>Gets the thickness of every collision-backed shrine wall.</summary>
    public const float WallThicknessPixels = OpeningRegionOldShrineGeometry.WallThicknessPixels;

    /// <summary>Gets the exact old-shrine binding after scene sources become ready.</summary>
    public OpeningRegionOldShrineSceneBinding Binding { get; private set; } = null!;

    /// <summary>Gets the collision-aware old-shrine layout after scene sources become ready.</summary>
    public OpeningRegionOldShrineLayout Layout { get; private set; } = null!;

    /// <summary>Gets the caller-owned representative discovery scenario.</summary>
    public DiscoveryScenario DiscoveryScenario { get; private set; } = null!;

    /// <summary>Gets the inert exact shrine-inscription binding.</summary>
    public OpeningRegionLayeredInscriptionBinding InscriptionBinding { get; private set; } = null!;

    /// <summary>Gets the exact scene-authored layered-inscription marker.</summary>
    public Node2D LayeredInscription { get; private set; } = null!;

    /// <summary>Gets the one-shot scene-owned inscription discovery state.</summary>
    public OpeningRegionLayeredInscriptionDiscoveryState DiscoveryState { get; private set; } =
        null!;

    /// <summary>Gets the exact presented discovery token, or null while concealed.</summary>
    public OpeningRegionLayeredInscriptionPresentation? Presentation { get; private set; }

    /// <summary>Gets the exact filtered evidence exposure, or null before presentation.</summary>
    public OpeningRegionLayeredInscriptionEvidenceExposure? EvidenceExposure { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        OpeningRegionAnchor shrine = projection.Anchors.Single(anchor =>
            anchor.Location.Identity.Id.Value == "old-shrine");
        Binding = new OpeningRegionOldShrineSceneBinding(
            projection,
            shrine,
            OpeningRegionOldShrineGeometry.ShrineBounds);
        Layout = new OpeningRegionOldShrineLayout(
            Binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            OpeningRegionOldShrineGeometry.SouthernRoadOpening);

        DiscoveryScenario = RepresentativeDiscoveryScenarioSource.Create();
        WorldObject inscription = DiscoveryScenario.WorldObjects.Entries.Single(candidate =>
            candidate.Identity.Id.Value == "shrine-inscription");
        WorldObjectVisibility visibility = DiscoveryScenario.InitialPerception.Entries.Single(candidate =>
            candidate.ObjectId.Value == "shrine-inscription");
        LayeredInscription = GetNode<Node2D>("LayeredInscription");
        InscriptionBinding = new OpeningRegionLayeredInscriptionBinding(
            Binding,
            DiscoveryScenario,
            inscription,
            visibility,
            ToNumerics(LayeredInscription.Position));
        DiscoveryState = OpeningRegionLayeredInscriptionDiscoveryStateTransition.Initialize(
            InscriptionBinding);
        Presentation = null;
        EvidenceExposure = null;
    }

    /// <summary>Applies the exact first pure inscription resolution without presenting it.</summary>
    public bool TryApplyInscriptionResolution(
        OpeningRegionLayeredInscriptionResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        OpeningRegionLayeredInscriptionAccess access = resolution.CandidateBinding.Access;
        if (DiscoveryState.Resolution is not null
            || Presentation is not null
            || EvidenceExposure is not null
            || LayeredInscription.Visible
            || !ReferenceEquals(
                access.InscriptionBinding,
                InscriptionBinding)
            || access.SeekerPosition != ToNumerics(
                GetNode<CharacterBody2D>("../Seeker").GlobalPosition)
            || access.InscriptionPosition != ToNumerics(LayeredInscription.GlobalPosition))
        {
            return false;
        }

        DiscoveryState = OpeningRegionLayeredInscriptionDiscoveryStateTransition.Apply(
            DiscoveryState,
            resolution);
        return true;
    }

    /// <summary>Reveals the existing marker once for the exact applied discovery state.</summary>
    public bool TryPresentInscription(
        OpeningRegionLayeredInscriptionPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        if (Presentation is not null
            || LayeredInscription.Visible
            || !ReferenceEquals(presentation.DiscoveryState, DiscoveryState)
            || !ReferenceEquals(presentation.Resolution, DiscoveryState.Resolution))
        {
            return false;
        }

        LayeredInscription.Visible = true;
        Presentation = presentation;
        return true;
    }

    /// <summary>Retains the exact single-evidence exposure once without adding UI or persistence.</summary>
    public bool TryExposeInscriptionEvidence(
        OpeningRegionLayeredInscriptionEvidenceExposure exposure)
    {
        ArgumentNullException.ThrowIfNull(exposure);
        if (EvidenceExposure is not null
            || !LayeredInscription.Visible
            || !ReferenceEquals(exposure.Presentation, Presentation))
        {
            return false;
        }

        EvidenceExposure = exposure;
        return true;
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);
}
