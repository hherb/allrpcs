using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
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

    /// <summary>Gets the exact scenario-owned knowledgeable Seeker.</summary>
    public Character KnowledgeableSeeker { get; private set; } = null!;

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

    /// <summary>Gets exact additive Scribe inspection access, or null before selection.</summary>
    public OpeningRegionOldShrineInspectionAccess? CurrentInspectionAccess { get; private set; }

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
        ConfigurePresentation();
        ConfigureBoundaries();

        DiscoveryScenario = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").RepresentativeDiscoveryScenario;
        KnowledgeableSeeker = CreateKnowledgeableSeeker(DiscoveryScenario);
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
        CurrentInspectionAccess = null;
    }

    /// <summary>Reports whether exact current Scribe access can be retained once.</summary>
    public bool CanInstallInspectionAccess(
        SeekerBackgroundRouteVariationState? currentRouteState,
        OpeningRegionOldShrineInspectionAccess? access) =>
        CurrentInspectionAccess is null
        && access is not null
        && ReferenceEquals(access.RouteAccess.RouteState, currentRouteState)
        && ReferenceEquals(access.ShrineSceneBinding, Binding)
        && ReferenceEquals(access.InscriptionBinding, InscriptionBinding)
        && ReferenceEquals(DiscoveryState.InscriptionBinding, InscriptionBinding)
        && DiscoveryState.Resolution is null
        && Presentation is null
        && EvidenceExposure is null
        && !LayeredInscription.Visible;

    /// <summary>Retains exact Scribe inspection authorization without changing discovery.</summary>
    public bool TryInstallInspectionAccess(
        SeekerBackgroundRouteVariationState? currentRouteState,
        OpeningRegionOldShrineInspectionAccess? access)
    {
        if (!CanInstallInspectionAccess(currentRouteState, access))
        {
            return false;
        }

        CurrentInspectionAccess = access;
        return true;
    }

    /// <summary>Commits one completely preflighted discovery graph without partial application.</summary>
    public bool TryCommitInscriptionDiscovery(
        OpeningRegionLayeredInscriptionDiscoveryState? expectedState,
        OpeningRegionLayeredInscriptionDiscoveryState? appliedState,
        OpeningRegionLayeredInscriptionPresentation? presentation,
        OpeningRegionLayeredInscriptionEvidenceExposure? evidenceExposure)
    {
        OpeningRegionLayeredInscriptionResolution? resolution = appliedState?.Resolution;
        OpeningRegionLayeredInscriptionAccess? access = resolution?.CandidateBinding.Access;
        if (expectedState is null
            || appliedState is null
            || presentation is null
            || evidenceExposure is null
            || !ReferenceEquals(DiscoveryState, expectedState)
            || expectedState.Resolution is not null
            || Presentation is not null
            || EvidenceExposure is not null
            || LayeredInscription.Visible
            || !ReferenceEquals(expectedState.InscriptionBinding, InscriptionBinding)
            || !ReferenceEquals(expectedState.Perception, DiscoveryScenario.InitialPerception)
            || !ReferenceEquals(
                expectedState.Candidates,
                DiscoveryScenario.ReinterpretationCandidates)
            || !ReferenceEquals(appliedState.InscriptionBinding, InscriptionBinding)
            || resolution is null
            || !ReferenceEquals(resolution.Seeker, KnowledgeableSeeker)
            || !ReferenceEquals(access?.InscriptionBinding, InscriptionBinding)
            || !ReferenceEquals(appliedState.Perception, resolution.Result.Perception)
            || !ReferenceEquals(appliedState.Candidates, resolution.Result.Candidates)
            || !ReferenceEquals(presentation.DiscoveryState, appliedState)
            || !ReferenceEquals(presentation.Resolution, resolution)
            || !ReferenceEquals(evidenceExposure.Presentation, presentation)
            || access.SeekerPosition != ToNumerics(
                GetNode<CharacterBody2D>("../Seeker").GlobalPosition)
            || access.InscriptionPosition != ToNumerics(LayeredInscription.GlobalPosition))
        {
            return false;
        }

        DiscoveryState = appliedState;
        LayeredInscription.Visible = true;
        Presentation = presentation;
        EvidenceExposure = evidenceExposure;
        return true;
    }

    private static NumericsVector2 ToNumerics(Vector2 position) =>
        new(position.X, position.Y);

    private void ConfigurePresentation()
    {
        GetNode<Polygon2D>("ShrineArea").Polygon = Rectangle(Binding.OldShrineBounds);
        GetNode<Polygon2D>("WoodlandClearingOpening").Polygon = VerticalOpening(
            Layout.WoodlandClearingOpening);
        GetNode<Polygon2D>("SouthernRoadOpening").Polygon = HorizontalOpening(
            Layout.SouthernRoadOpening);
        GetNode<Label>("ShrineLabel").Position = new Vector2(
            Binding.OldShrineBounds.Left + 12.0f,
            Binding.OldShrineBounds.Top + 12.0f);
    }

    private void ConfigureBoundaries()
    {
        float half = Layout.OpeningWidthPixels / 2.0f;
        NavigationWorldBounds bounds = Binding.OldShrineBounds;
        List<OpeningRegionBoundarySegment> segments =
        [
            new(new(bounds.Left, bounds.Top), new(bounds.Right, bounds.Top)),
            new(new(bounds.Left, bounds.Top), new(bounds.Left, bounds.Bottom)),
            new(new(bounds.Right, bounds.Top), new(bounds.Right, Layout.WoodlandClearingOpening.Y - half)),
            new(new(bounds.Right, Layout.WoodlandClearingOpening.Y + half), new(bounds.Right, bounds.Bottom)),
            new(new(bounds.Left, bounds.Bottom), new(Layout.SouthernRoadOpening.X - half, bounds.Bottom)),
        ];
        Node2D owner = GetNode<Node2D>("ShrineBoundaries");
        if (owner.GetChildCount() != segments.Count)
        {
            throw new InvalidOperationException(
                "The authored shrine-boundary count must match the derived layout.");
        }

        for (int index = 0; index < segments.Count; index++)
        {
            StaticBody2D body = owner.GetChild<StaticBody2D>(index);
            body.Position = Vector2.Zero;
            body.GetChild<CollisionShape2D>(0).Shape = new SegmentShape2D
            {
                A = ToGodot(segments[index].Start),
                B = ToGodot(segments[index].End),
            };
        }
    }

    private static Vector2 ToGodot(NumericsVector2 position) =>
        new(position.X, position.Y);

    private static Vector2[] Rectangle(NavigationWorldBounds bounds) =>
    [
        new(bounds.Left, bounds.Top),
        new(bounds.Right, bounds.Top),
        new(bounds.Right, bounds.Bottom),
        new(bounds.Left, bounds.Bottom),
    ];

    private static Vector2[] HorizontalOpening(NumericsVector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y - WallThicknessPixels),
            new(center.X + half, center.Y + WallThicknessPixels),
            new(center.X - half, center.Y + WallThicknessPixels),
        ];
    }

    private static Vector2[] VerticalOpening(NumericsVector2 center)
    {
        float half = OpeningWidthPixels / 2.0f;
        return
        [
            new(center.X - WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y - half),
            new(center.X + WallThicknessPixels, center.Y + half),
            new(center.X - WallThicknessPixels, center.Y + half),
        ];
    }

    private static Character CreateKnowledgeableSeeker(DiscoveryScenario scenario)
    {
        Allrpcs.Domain.Identifiers.CharacterId characterId = scenario.ObserverCharacterId;
        return new Character(
            new CharacterIdentity(characterId, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            scenario.AvailableKnowledge,
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }
}
