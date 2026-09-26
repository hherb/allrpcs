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
        TemporarySeekerCharacterSource characterSource =
            GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        SeekerCustomizationApplicationCoordinator coordinator =
            GetNode<SeekerCustomizationApplicationCoordinator>(
                "../SeekerCustomizationApplicationCoordinator");
        SeekerCustomizationSelectionState initialCustomization =
            customizationSource.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;

        if (initialRoutes.SelectedVariation is not null
            || initialRoutes.PreviousState is not null
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
                initialRoutes.OpeningRegion.Anchors[10]))
        {
            Fail("Exact applied background did not install its optional route definition.");
            return;
        }

        SeekerBackgroundRouteVariationState retainedRoutes = routeSource.CurrentState;
        Character retainedCharacter = characterSource.Character;
        if (coordinator.TryApply(initialCustomization, selected)
            || !ReferenceEquals(routeSource.CurrentState, retainedRoutes)
            || !ReferenceEquals(characterSource.Character, retainedCharacter))
        {
            Fail("Repeated customization changed the one-shot optional route state.");
            return;
        }

        GD.Print(
            "Seeker background route state smoke passed: inert exact four-route catalogue, "
            + "one exact applied Forester selection, closed-path definition retention, "
            + "one-shot compare-and-swap, and unchanged navigation and presentation.");
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
