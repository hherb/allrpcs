using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves one exact customization installs one optional route definition only.</summary>
public partial class SeekerBackgroundRouteVariationSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_BACKGROUND_ROUTE_STATE_CHECK";
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
        OpeningRegionClosedForestPathSceneSource closedPathSource =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        SeekerCustomizationSelectionState initialCustomization =
            customizationSource.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;
        StaticBody2D closedGate = closedPathSource.GetNode<StaticBody2D>("ClosedGate");

        if (initialRoutes.SelectedVariation is not null
            || initialRoutes.PreviousState is not null
            || routeAccessSource.CurrentAccess is not null
            || closedPathSource.CurrentAccess is not null
            || closedGate.GetChild<CollisionShape2D>(0).Disabled
            || !closedGate.GetChild<Line2D>(1).Visible
            || !closedPathSource.ClosedGateMarker.Visible
            || !HasExactRepresentativeRoutes(
                customizationSource.Options,
                initialRoutes)
            || coordinator.TryApply(null, null)
            || !ReferenceEquals(customizationSource.CurrentState, initialCustomization)
            || !ReferenceEquals(routeSource.CurrentState, initialRoutes)
            || !ReferenceEquals(characterSource.Character, initialCharacter)
            || characterSource.CustomizationApplication is not null)
        {
            Fail("Optional background routes did not retain inert exact startup state.");
            return;
        }

        SeekerCustomizationSelectionState selected = SelectedState(
            initialCustomization,
            initialCharacter.Identity.Id,
            backgroundIndex: 0);
        if (!coordinator.TryApply(initialCustomization, selected)
            || !ReferenceEquals(customizationSource.CurrentState, selected)
            || !ReferenceEquals(
                characterSource.CustomizationApplication?.SelectedState,
                selected)
            || !ReferenceEquals(routeSource.CurrentState.PreviousState, initialRoutes)
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation,
                initialRoutes.Variations[0])
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation?.DestinationAnchor,
                initialRoutes.OpeningRegion.Anchors[10])
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.RouteState,
                routeSource.CurrentState)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.SelectedVariation,
                routeSource.CurrentState.SelectedVariation)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.DestinationAnchor,
                closedPathSource.Binding.ClosedForestPathAnchor)
            || !ReferenceEquals(
                closedPathSource.CurrentAccess?.RouteAccess,
                routeAccessSource.CurrentAccess)
            || !closedGate.GetChild<CollisionShape2D>(0).Disabled
            || closedGate.GetChild<Line2D>(1).Visible
            || closedPathSource.ClosedGateMarker.Visible)
        {
            Fail("Exact applied Forester background did not open its existing route gate.");
            return;
        }

        SeekerBackgroundRouteVariationState retainedRoutes = routeSource.CurrentState;
        Character retainedCharacter = characterSource.Character;
        OpeningRegionClosedForestPathAccess retainedAccess =
            closedPathSource.CurrentAccess!;
        SeekerBackgroundRouteAccess retainedRouteAccess =
            routeAccessSource.CurrentAccess!;
        if (coordinator.TryApply(initialCustomization, selected)
            || !ReferenceEquals(routeSource.CurrentState, retainedRoutes)
            || !ReferenceEquals(characterSource.Character, retainedCharacter)
            || !ReferenceEquals(closedPathSource.CurrentAccess, retainedAccess)
            || !ReferenceEquals(routeAccessSource.CurrentAccess, retainedRouteAccess)
            || !closedGate.GetChild<CollisionShape2D>(0).Disabled
            || closedGate.GetChild<Line2D>(1).Visible
            || closedPathSource.ClosedGateMarker.Visible)
        {
            Fail("Repeated customization changed the one-shot Forester route access.");
            return;
        }

        GD.Print(
            "Seeker background route state smoke passed: inert exact four-route catalogue, "
            + "one exact applied Forester selection, exact closed-path access consumption, "
            + "one removed gate barrier, and one-shot compare-and-swap preservation.");
        GetTree().Quit();
    }

    private static bool HasExactRepresentativeRoutes(
        SeekerCustomizationOptions options,
        SeekerBackgroundRouteVariationState state)
    {
        int[] anchorIndexes = [10, 7, 1, 8];
        return state.Variations.Count == options.BackgroundChoices.Count
            && state.Variations.Select((variation, index) =>
                ReferenceEquals(variation.Background, options.BackgroundChoices[index])
                && ReferenceEquals(
                    variation.DestinationAnchor,
                    state.OpeningRegion.Anchors[anchorIndexes[index]])).All(value => value);
    }

    private static SeekerCustomizationSelectionState SelectedState(
        SeekerCustomizationSelectionState initial,
        CharacterId characterId,
        int backgroundIndex) => SeekerCustomizationSelectionStateTransition.Select(
            initial,
            new CharacterIdentity(characterId, "Mara"),
            new SeekerPronouns("they", "them", "their", "theirs", "themself"),
            initial.Options.AppearanceChoices[0],
            initial.Options.BackgroundChoices[backgroundIndex]);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
