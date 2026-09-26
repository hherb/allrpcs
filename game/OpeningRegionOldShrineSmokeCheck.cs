using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Proves the inert shrine shell, layered marker, and clearing round trip.</summary>
public partial class OpeningRegionOldShrineSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumSynchronizationPhysicsFrames = 60;
    private const int ExpectedBoundarySectionCount = 5;
    private const int ExpectedInscriptionLayerCount = 2;
    private const string LayeredInscriptionDiscoverAction = "layered_inscription_discover";
    private const string UnrelatedAction = "old_shrine_smoke_unrelated";
    private const float SurfaceTolerancePixels = 1.0f;
    private static readonly NumericsVector2 InscriptionAccessPosition = new(110.0f, 128.0f);

    private int physicsFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool enabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetPhysicsProcess(enabled);
        if (!enabled)
        {
            return;
        }

        GetNode<CharacterBody2D>("../Seeker").SetPhysicsProcess(false);
        Healer healer = GetNode<Healer>("../Healer");
        healer.CommandWait();
        healer.GlobalPosition = new Vector2(1500.0f, 850.0f);
        healer.SetPhysicsProcess(false);
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        physicsFrames++;
        OpeningRegionOldShrineLayout? layout =
            GetNode<OpeningRegionOldShrineSceneSource>(
                "../OpeningRegionOldShrineSceneSource").Layout;
        NavigationRegion2D region = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        Rid navigationMap = NavigationServer2D.RegionGetMap(region.GetRid());
        bool synchronized = layout is not null
            && NavigationServer2D.RegionGetIterationId(region.GetRid()) > 0
            && IsOnSurface(navigationMap, layout.SceneBinding.OldShrineAnchor.Position);
        if (!synchronized && physicsFrames < MaximumSynchronizationPhysicsFrames)
        {
            return;
        }

        SetPhysicsProcess(false);
        if (layout is null || !synchronized)
        {
            Fail("The old-shrine layout or synchronized navigation surface was unavailable.");
            return;
        }

        RunChecks(layout, navigationMap);
    }

    private void RunChecks(OpeningRegionOldShrineLayout layout, Rid navigationMap)
    {
        OpeningRegionOldShrineSceneSource source =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionLayeredInscriptionDiscoveryCoordinator coordinator =
            GetNode<OpeningRegionLayeredInscriptionDiscoveryCoordinator>(
                "../OpeningRegionLayeredInscriptionDiscoveryCoordinator");
        Node boundaries = GetNode("../OpeningRegionOldShrineSceneSource/ShrineBoundaries");
        Node2D inscription = GetNode<Node2D>(
            "../OpeningRegionOldShrineSceneSource/LayeredInscription");
        CharacterBody2D seekerBody = GetNode<CharacterBody2D>("../Seeker");
        Main main = GetNode<Main>("..");
        OpeningRegionLayeredInscriptionDiscoveryState initialDiscoveryState =
            source.DiscoveryState;
        DiscoveryScenario scenario = source.DiscoveryScenario;
        Character rebuiltSeeker = CreateSeeker(
            scenario.ObserverCharacterId,
            scenario.AvailableKnowledge);
        Character foreignSeeker = CreateSeeker(
            new CharacterId(scenario.ObserverCharacterId.Value),
            scenario.AvailableKnowledge);
        Character missingKnowledgeSeeker = CreateSeeker(
            scenario.ObserverCharacterId,
            new CharacterKnowledge([]));
        bool initialInscriptionVisibility = inscription.Visible;
        bool nullDiscovery = coordinator.TryDiscover(null);
        bool rebuiltAuthorityDiscovery = coordinator.TryDiscover(rebuiltSeeker);
        bool foreignAuthorityDiscovery = coordinator.TryDiscover(foreignSeeker);
        bool missingKnowledgeDiscovery = coordinator.TryDiscover(missingKnowledgeSeeker);
        main._UnhandledInput(new InputEventAction
        {
            Action = LayeredInscriptionDiscoverAction,
            Pressed = false,
        });
        main._UnhandledInput(new InputEventAction
        {
            Action = UnrelatedAction,
            Pressed = true,
        });
        seekerBody.GlobalPosition = Vector2.Zero;
        PressDiscovery(main);
        bool farPositionPreservedState = ReferenceEquals(
            source.DiscoveryState,
            initialDiscoveryState);
        seekerBody.GlobalPosition = ToGodot(InscriptionAccessPosition);
        inscription.GlobalPosition += Vector2.Right;
        bool movedMarkerDiscovery = coordinator.TryDiscover(source.KnowledgeableSeeker);
        inscription.GlobalPosition -= Vector2.Right;
        inscription.Visible = true;
        bool alreadyVisibleDiscovery = coordinator.TryDiscover(source.KnowledgeableSeeker);
        inscription.Visible = false;
        bool rejectedPathsPreservedState = ReferenceEquals(
                source.DiscoveryState,
                initialDiscoveryState)
            && source.Presentation is null
            && source.EvidenceExposure is null
            && coordinator.CompletedExposure is null;
        PressDiscovery(main);
        bool discovered = !ReferenceEquals(source.DiscoveryState, initialDiscoveryState);
        OpeningRegionLayeredInscriptionDiscoveryState completedDiscoveryState =
            source.DiscoveryState;
        OpeningRegionLayeredInscriptionPresentation? completedPresentation =
            source.Presentation;
        OpeningRegionLayeredInscriptionEvidenceExposure? completedEvidenceExposure =
            source.EvidenceExposure;
        PressDiscovery(main);
        bool repeatedDiscoveryPreservedState = ReferenceEquals(
                source.DiscoveryState,
                completedDiscoveryState)
            && ReferenceEquals(source.Presentation, completedPresentation)
            && ReferenceEquals(source.EvidenceExposure, completedEvidenceExposure);
        OpeningRegionLayeredInscriptionResolution? resolution =
            source.DiscoveryState.Resolution;
        OpeningRegionLayeredInscriptionCandidateBinding? candidateBinding =
            resolution?.CandidateBinding;
        OpeningRegionLayeredInscriptionAccess? access = candidateBinding?.Access;
        OpeningRegionLayeredInscriptionPresentation? presentation = source.Presentation;
        OpeningRegionLayeredInscriptionEvidenceExposure? evidenceExposure =
            source.EvidenceExposure;
        WorldObjectVisibility? resolvedVisibility = resolution?.Result.Perception.Entries.Single(
            entry => ReferenceEquals(entry.ObjectId, candidateBinding?.Candidate.TargetObjectId));
        if (!HasCollisionShapes(boundaries)
            || !HasOnlyPhysicalKey(LayeredInscriptionDiscoverAction, Key.I)
            || initialInscriptionVisibility
            || !inscription.Visible
            || inscription.GetChildCount() != ExpectedInscriptionLayerCount
            || inscription.GetChildren().Any(child => child is not Marker2D marker
                || marker.GetChildCount() != 0)
            || ToNumerics(inscription.Position)
                != OpeningRegionOldShrineGeometry.LayeredInscriptionPosition
            || !ReferenceEquals(source.Layout, layout)
            || !ReferenceEquals(source.Binding, layout.SceneBinding)
            || !ReferenceEquals(source.LayeredInscription, inscription)
            || !ReferenceEquals(source.InscriptionBinding.ShrineSceneBinding, layout.SceneBinding)
            || !ReferenceEquals(
                source.InscriptionBinding.DiscoveryScenario,
                source.DiscoveryScenario)
            || !source.DiscoveryScenario.WorldObjects.Entries.Any(candidate =>
                ReferenceEquals(candidate, source.InscriptionBinding.WorldObject))
            || !source.DiscoveryScenario.InitialPerception.Entries.Any(candidate =>
                ReferenceEquals(candidate, source.InscriptionBinding.InitialVisibility))
            || !ReferenceEquals(
                source.InscriptionBinding.WorldObject.Identity.Id,
                source.InscriptionBinding.InitialVisibility.ObjectId)
            || !ReferenceEquals(coordinator.SceneSource, source)
            || !ReferenceEquals(source.KnowledgeableSeeker.Identity.Id, scenario.ObserverCharacterId)
            || !ReferenceEquals(source.KnowledgeableSeeker.Knowledge, scenario.AvailableKnowledge)
            || access is null
            || !ReferenceEquals(access.InscriptionBinding, source.InscriptionBinding)
            || !ReferenceEquals(
                access.SeekerCharacterId,
                source.DiscoveryScenario.ObserverCharacterId)
            || access.InscriptionPosition != ToNumerics(inscription.GlobalPosition)
            || candidateBinding is null
            || !ReferenceEquals(candidateBinding.Access, access)
            || !ReferenceEquals(
                candidateBinding.Candidate,
                source.DiscoveryScenario.ReinterpretationCandidates.Entries.Single())
            || !ReferenceEquals(
                candidateBinding.TriggerKnowledgeId,
                source.DiscoveryScenario.AvailableKnowledge.KnowledgeReferences.Single())
            || resolution is null
            || !ReferenceEquals(resolution.CandidateBinding, candidateBinding)
            || !ReferenceEquals(resolution.Seeker, source.KnowledgeableSeeker)
            || resolution.Result.Candidates.Entries.Count != 0
            || !ReferenceEquals(resolvedVisibility?.VisibilityState, VisibilityState.Identified)
            || nullDiscovery
            || rebuiltAuthorityDiscovery
            || foreignAuthorityDiscovery
            || missingKnowledgeDiscovery
            || !farPositionPreservedState
            || movedMarkerDiscovery
            || alreadyVisibleDiscovery
            || !rejectedPathsPreservedState
            || !discovered
            || !repeatedDiscoveryPreservedState
            || ReferenceEquals(source.DiscoveryState, initialDiscoveryState)
            || !ReferenceEquals(
                source.DiscoveryState.InscriptionBinding,
                source.InscriptionBinding)
            || !ReferenceEquals(source.DiscoveryState.Perception, resolution.Result.Perception)
            || !ReferenceEquals(source.DiscoveryState.Candidates, resolution.Result.Candidates)
            || !ReferenceEquals(source.DiscoveryState.Resolution, resolution)
            || initialDiscoveryState.Resolution is not null
            || presentation is null
            || !ReferenceEquals(source.Presentation, presentation)
            || !ReferenceEquals(presentation.DiscoveryState, source.DiscoveryState)
            || !ReferenceEquals(presentation.Resolution, resolution)
            || evidenceExposure is null
            || !ReferenceEquals(source.EvidenceExposure, evidenceExposure)
            || !ReferenceEquals(coordinator.CompletedExposure, evidenceExposure)
            || !ReferenceEquals(evidenceExposure.Presentation, presentation)
            || evidenceExposure.EvidenceView.EvidenceIds.Count != 1
            || !ReferenceEquals(
                evidenceExposure.EvidenceView.EvidenceIds.Single(),
                evidenceExposure.Evidence.EvidenceId)
            || evidenceExposure.Evidence.EvidenceId.Value != "partial-inscription"
            || !ReferenceEquals(
                source.InscriptionBinding.InitialVisibility.VisibilityState,
                VisibilityState.Noticed)
            || !ReferenceEquals(source.DiscoveryScenario, scenario)
            || !ReferenceEquals(source.DiscoveryScenario.WorldObjects, scenario.WorldObjects)
            || !ReferenceEquals(source.DiscoveryScenario.Evidence, scenario.Evidence)
            || !ReferenceEquals(
                source.DiscoveryScenario.InitialPerception,
                scenario.InitialPerception)
            || !ReferenceEquals(
                source.DiscoveryScenario.ReinterpretationCandidates,
                scenario.ReinterpretationCandidates)
            || !ReferenceEquals(
                layout.SceneBinding.Projection,
                GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection))
        {
            Fail("The shrine lost collision geometry, layered placement, or exact provenance.");
            return;
        }

        OpeningRegionWoodlandClearingLayout clearingLayout =
            GetNode<OpeningRegionWoodlandClearingSceneSource>(
                "../OpeningRegionWoodlandClearingSceneSource").Layout;
        NumericsVector2 clearing = clearingLayout.SceneBinding.WoodlandClearingAnchor.Position;
        NumericsVector2 shrine = layout.SceneBinding.OldShrineAnchor.Position;
        List<NumericsVector2> roundTrip = Route(navigationMap, clearing, shrine);
        if (roundTrip.Count == 0
            || !OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(clearingLayout, roundTrip)
            || !OpeningRegionOldShrineRoutePolicy.IsAcceptable(layout, roundTrip)
            || !AssertClear(ToGodot(clearing), ToGodot(shrine))
            || !AssertClear(ToGodot(shrine), ToGodot(clearing))
            || !AssertCollision(ToGodot(shrine), Vector2.Up * 80.0f, "ShrineTop")
            || !AssertCollision(ToGodot(shrine), Vector2.Left * 160.0f, "ShrineLeft"))
        {
            return;
        }

        GD.Print(
            "Opening-region old-shrine smoke passed: five collision-backed boundaries, two "
            + "topology openings, one exact shrine-local Seeker access, one exact noticed "
            + "inscription binding, complete preflight before one atomic exact-candidate "
            + "reinterpretation, rejected null/foreign/rebuilt/missing-knowledge/far/moved/visible "
            + "and repeated paths, released/unrelated input inertness, one ordinary I-key reveal, "
            + "one exact filtered evidence exposure, and synchronized clearing round trip.");
        GetTree().Quit();
    }

    private List<NumericsVector2> Route(
        Rid navigationMap,
        NumericsVector2 clearing,
        NumericsVector2 shrine)
    {
        List<NumericsVector2> roundTrip = [];
        foreach ((NumericsVector2 start, NumericsVector2 end) in new[]
        {
            (clearing, shrine),
            (shrine, clearing),
        })
        {
            Vector2[] path = NavigationServer2D.MapGetPath(
                navigationMap,
                ToGodot(start),
                ToGodot(end),
                optimize: true);
            if (path.Length < 2
                || path[^1].DistanceTo(ToGodot(end)) > SurfaceTolerancePixels)
            {
                Fail("The synchronized clearing/shrine round trip did not reach its endpoint.");
                return [];
            }

            foreach (Vector2 point in path)
            {
                NumericsVector2 converted = ToNumerics(point);
                if (roundTrip.Count == 0 || roundTrip[^1] != converted)
                {
                    roundTrip.Add(converted);
                }
            }
        }

        return roundTrip;
    }

    private bool AssertClear(Vector2 start, Vector2 end)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(end - start, testOnly: true);
        if (collision is null)
        {
            return true;
        }

        string actual = collision.GetCollider() is Node collider
            ? collider.Name.ToString()
            : collision.GetCollider().ToString() ?? "unknown";
        Fail($"The physical clearing/shrine route hit {actual}.");
        return false;
    }

    private bool AssertCollision(Vector2 start, Vector2 motion, string expectedName)
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        seeker.GlobalPosition = start;
        KinematicCollision2D? collision = seeker.MoveAndCollide(motion, testOnly: true);
        if (collision?.GetCollider() is Node collider && collider.Name == expectedName)
        {
            return true;
        }

        string actual = collision?.GetCollider() is Node actualCollider
            ? actualCollider.Name.ToString()
            : "none";
        Fail($"Expected shrine collision from {expectedName}, received {actual}.");
        return false;
    }

    private static bool HasCollisionShapes(Node boundaries) =>
        boundaries.GetChildCount() == ExpectedBoundarySectionCount
        && boundaries.GetChildren().All(child => child is StaticBody2D body
            && body.GetChildCount() == 1
            && body.GetChild(0) is CollisionShape2D shape
            && shape.Shape is not null);

    private static bool IsOnSurface(Rid navigationMap, NumericsVector2 position) =>
        NavigationServer2D.MapGetClosestPoint(navigationMap, ToGodot(position))
            .DistanceTo(ToGodot(position)) <= SurfaceTolerancePixels;

    private static Vector2 ToGodot(NumericsVector2 position) => new(position.X, position.Y);

    private static NumericsVector2 ToNumerics(Vector2 position) => new(position.X, position.Y);

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private static void PressDiscovery(Main main)
    {
        main._UnhandledInput(new InputEventAction
        {
            Action = LayeredInscriptionDiscoverAction,
            Pressed = true,
        });
    }

    private static Character CreateSeeker(
        CharacterId characterId,
        CharacterKnowledge knowledge)
    {
        return new Character(
            new CharacterIdentity(characterId, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(characterId, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            knowledge,
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
