using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves exact Traveller selection installs inert road and wagon access.</summary>
public partial class SeekerTravellerRoadWagonAccessSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_TRAVELLER_ROAD_WAGON_ACCESS_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        RunChecks();
    }

    private void RunChecks()
    {
        SeekerCustomizationSelectionSceneSource customizationSource =
            GetNode<SeekerCustomizationSelectionSceneSource>(
                "../SeekerCustomizationSelectionSceneSource");
        SeekerBackgroundRouteVariationSceneSource routeSource =
            GetNode<SeekerBackgroundRouteVariationSceneSource>(
                "../SeekerBackgroundRouteVariationSceneSource");
        SeekerBackgroundRouteAccessSceneSource routeAccessSource =
            GetNode<SeekerBackgroundRouteAccessSceneSource>(
                "../SeekerBackgroundRouteAccessSceneSource");
        TemporarySeekerCharacterSource characterSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        SeekerCustomizationApplicationCoordinator coordinator =
            GetNode<SeekerCustomizationApplicationCoordinator>(
                "../SeekerCustomizationApplicationCoordinator");
        OpeningRegionSouthernRoadWagonSceneSource roadSource =
            GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
                "../OpeningRegionSouthernRoadWagonSceneSource");
        OpeningRegionRemedyPreparationSceneSource preparationSource =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource");
        OpeningRegionClosedForestPathSceneSource closedPathSource =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        SeekerCustomizationSelectionState initialCustomization =
            customizationSource.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;
        OpeningRegionSouthernRoadWagonSceneBinding binding = roadSource.Binding;
        OpeningRegionSouthernRoadWagonLayout initialLayout = roadSource.Layout;
        Vector2 initialMarkerPosition = roadSource.AbandonedWagonMarker.Position;
        bool initialMarkerVisibility = roadSource.AbandonedWagonMarker.Visible;
        bool[] initialBoundaryState = BoundaryState(roadSource);

        Candidate forester = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 0);
        Candidate scribe = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 1);
        Candidate apothecary = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 2);
        Candidate traveller = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 3);
        OpeningRegionSouthernRoadWagonAccess candidateAccess =
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                traveller.RouteAccess,
                traveller.RouteAccess,
                binding,
                binding,
                issuedAccess: null)!;
        SeekerBackgroundRouteVariationState rebuiltInitial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(
                customizationSource.Options,
                initialRoutes.OpeningRegion);
        Candidate rebuilt = CandidateFor(
            initialCustomization,
            rebuiltInitial,
            initialCharacter,
            backgroundIndex: 3);
        OpeningRegionSouthernRoadWagonAccess rebuiltAccess =
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                rebuilt.RouteAccess,
                rebuilt.RouteAccess,
                binding,
                binding,
                issuedAccess: null)!;
        OpeningRegionSouthernRoadWagonSceneSource preBindingSource = new();
        bool preBindingInstalled = preBindingSource.TryInstallTravellerAccess(
            traveller.RouteState,
            candidateAccess);
        preBindingSource.Free();

        if (roadSource.CurrentTravellerAccess is not null
            || roadSource.TryInstallTravellerAccess(forester.RouteState, candidateAccess)
            || roadSource.TryInstallTravellerAccess(scribe.RouteState, candidateAccess)
            || roadSource.TryInstallTravellerAccess(apothecary.RouteState, candidateAccess)
            || roadSource.TryInstallTravellerAccess(initialRoutes, candidateAccess)
            || roadSource.TryInstallTravellerAccess(traveller.RouteState, null)
            || preBindingInstalled
            || coordinator.TryApply(null, null)
            || !ReferenceEquals(customizationSource.CurrentState, initialCustomization)
            || !ReferenceEquals(routeSource.CurrentState, initialRoutes)
            || !HasUnchangedScene(
                roadSource,
                initialLayout,
                initialMarkerPosition,
                initialMarkerVisibility,
                initialBoundaryState))
        {
            Fail("Traveller road access did not reject inert invalid startup paths.");
            return;
        }

        if (!coordinator.TryApply(initialCustomization, traveller.Selection)
            || !ReferenceEquals(customizationSource.CurrentState, traveller.Selection)
            || !ReferenceEquals(routeSource.CurrentState.PreviousState, initialRoutes)
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation,
                initialRoutes.Variations[3])
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.RouteState,
                routeSource.CurrentState)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.DestinationAnchor,
                binding.SouthernRoadAnchor)
            || !ReferenceEquals(
                roadSource.CurrentTravellerAccess?.RouteAccess,
                routeAccessSource.CurrentAccess)
            || !ReferenceEquals(
                roadSource.CurrentTravellerAccess?.TravellerVariation,
                routeSource.CurrentState.SelectedVariation)
            || !ReferenceEquals(roadSource.CurrentTravellerAccess?.SceneBinding, binding)
            || preparationSource.CurrentPreparationAccess is not null
            || closedPathSource.CurrentAccess is not null
            || shrineSource.CurrentInspectionAccess is not null
            || !HasUnchangedScene(
                roadSource,
                initialLayout,
                initialMarkerPosition,
                initialMarkerVisibility,
                initialBoundaryState))
        {
            Fail("Exact Traveller selection did not install inert road and wagon access.");
            return;
        }

        OpeningRegionSouthernRoadWagonAccess installed = roadSource.CurrentTravellerAccess!;
        SeekerBackgroundRouteVariationState installedRoutes = routeSource.CurrentState;
        Character installedCharacter = characterSource.Character;
        if (coordinator.TryApply(initialCustomization, traveller.Selection)
            || roadSource.TryInstallTravellerAccess(traveller.RouteState, candidateAccess)
            || roadSource.TryInstallTravellerAccess(rebuilt.RouteState, rebuiltAccess)
            || !ReferenceEquals(roadSource.CurrentTravellerAccess, installed)
            || !ReferenceEquals(routeSource.CurrentState, installedRoutes)
            || !ReferenceEquals(characterSource.Character, installedCharacter)
            || !HasUnchangedScene(
                roadSource,
                initialLayout,
                initialMarkerPosition,
                initialMarkerVisibility,
                initialBoundaryState))
        {
            Fail("Repeated or rebuilt Traveller input changed road access or scene state.");
            return;
        }

        GD.Print(
            "Traveller road and wagon access smoke passed: inert startup, rejected Forester, "
            + "Scribe, Apothecary, null, stale, rebuilt, and pre-binding inputs, one exact "
            + "Traveller token, unchanged geometry and presentation, and one-shot preservation.");
        GetTree().Quit();
    }

    private static bool HasUnchangedScene(
        OpeningRegionSouthernRoadWagonSceneSource source,
        OpeningRegionSouthernRoadWagonLayout layout,
        Vector2 markerPosition,
        bool markerVisibility,
        bool[] boundaryState) =>
        ReferenceEquals(source.Layout, layout)
        && source.AbandonedWagonMarker.Position == markerPosition
        && source.AbandonedWagonMarker.Visible == markerVisibility
        && BoundaryState(source).SequenceEqual(boundaryState);

    private static bool[] BoundaryState(OpeningRegionSouthernRoadWagonSceneSource source) =>
        source.GetNode<Node2D>("RoadBoundaries").GetChildren()
            .Concat(source.GetNode<Node2D>("WagonBoundaries").GetChildren())
            .SelectMany(node => new[]
            {
                ((StaticBody2D)node).GetChild<CollisionShape2D>(0).Disabled,
                ((StaticBody2D)node).GetChild<Line2D>(1).Visible,
            })
            .ToArray();

    private static Candidate CandidateFor(
        SeekerCustomizationSelectionState initialCustomization,
        SeekerBackgroundRouteVariationState initialRoutes,
        Character currentCharacter,
        int backgroundIndex)
    {
        SeekerCustomizationSelectionState selection =
            SeekerCustomizationSelectionStateTransition.Select(
                initialCustomization,
                new CharacterIdentity(currentCharacter.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                initialCustomization.Options.AppearanceChoices[0],
                initialCustomization.Options.BackgroundChoices[backgroundIndex]);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selection, currentCharacter);
        SeekerBackgroundRouteVariationState routeState =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initialRoutes,
                selection,
                application);
        SeekerBackgroundRouteAccess routeAccess =
            SeekerBackgroundRouteAccessPolicy.Authorize(
                routeState,
                routeState,
                routeState.SelectedVariation!.DestinationAnchor,
                issuedAccess: null)!;
        return new Candidate(selection, routeState, routeAccess);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed record Candidate(
        SeekerCustomizationSelectionState Selection,
        SeekerBackgroundRouteVariationState RouteState,
        SeekerBackgroundRouteAccess RouteAccess);
}
