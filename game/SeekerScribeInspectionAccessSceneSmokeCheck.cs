using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves exact Scribe selection installs inert old-shrine inspection access.</summary>
public partial class SeekerScribeInspectionAccessSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_SCRIBE_INSPECTION_ACCESS_CHECK";
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
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionEnvironmentalStateSource environmentSource =
            GetNode<OpeningRegionEnvironmentalStateSource>(
                "../OpeningRegionEnvironmentalStateSource");
        OpeningRegionClosedForestPathSceneSource closedPathSource =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        SeekerCustomizationSelectionState initialCustomization =
            customizationSource.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;
        OpeningRegionLayeredInscriptionDiscoveryState initialDiscovery =
            shrineSource.DiscoveryState;

        if (shrineSource.CurrentInspectionAccess is not null
            || initialDiscovery.Resolution is not null
            || shrineSource.Presentation is not null
            || shrineSource.EvidenceExposure is not null
            || shrineSource.LayeredInscription.Visible
            || !ReferenceEquals(environmentSource.CurrentRegions, environmentSource.BaselineRegions)
            || coordinator.TryApply(null, null)
            || !ReferenceEquals(customizationSource.CurrentState, initialCustomization)
            || !ReferenceEquals(routeSource.CurrentState, initialRoutes))
        {
            Fail("Scribe inspection access did not retain inert exact startup state.");
            return;
        }

        SeekerCustomizationSelectionState selected = SelectedState(
            initialCustomization,
            initialCharacter.Identity.Id);
        if (!coordinator.TryApply(initialCustomization, selected)
            || !ReferenceEquals(customizationSource.CurrentState, selected)
            || !ReferenceEquals(routeSource.CurrentState.PreviousState, initialRoutes)
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation,
                initialRoutes.Variations[1])
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.RouteState,
                routeSource.CurrentState)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.DestinationAnchor,
                shrineSource.Binding.OldShrineAnchor)
            || !ReferenceEquals(
                shrineSource.CurrentInspectionAccess?.RouteAccess,
                routeAccessSource.CurrentAccess)
            || !ReferenceEquals(
                shrineSource.CurrentInspectionAccess?.ScribeVariation,
                routeSource.CurrentState.SelectedVariation)
            || !ReferenceEquals(
                shrineSource.CurrentInspectionAccess?.ShrineSceneBinding,
                shrineSource.Binding)
            || !ReferenceEquals(
                shrineSource.CurrentInspectionAccess?.InscriptionBinding,
                shrineSource.InscriptionBinding)
            || !ReferenceEquals(shrineSource.DiscoveryState, initialDiscovery)
            || shrineSource.DiscoveryState.Resolution is not null
            || shrineSource.Presentation is not null
            || shrineSource.EvidenceExposure is not null
            || shrineSource.LayeredInscription.Visible
            || !ReferenceEquals(environmentSource.CurrentRegions, environmentSource.BaselineRegions)
            || closedPathSource.CurrentAccess is not null)
        {
            Fail("Exact Scribe selection did not install inert old-shrine inspection access.");
            return;
        }

        OpeningRegionOldShrineInspectionAccess installed =
            shrineSource.CurrentInspectionAccess!;
        SeekerBackgroundRouteVariationState installedRoutes = routeSource.CurrentState;
        Character installedCharacter = characterSource.Character;
        if (coordinator.TryApply(initialCustomization, selected)
            || !ReferenceEquals(shrineSource.CurrentInspectionAccess, installed)
            || !ReferenceEquals(routeSource.CurrentState, installedRoutes)
            || !ReferenceEquals(characterSource.Character, installedCharacter)
            || !ReferenceEquals(shrineSource.DiscoveryState, initialDiscovery)
            || shrineSource.LayeredInscription.Visible)
        {
            Fail("Repeated Scribe selection changed one-shot inspection or discovery state.");
            return;
        }

        GD.Print(
            "Scribe inspection access smoke passed: inert startup, one exact Scribe route, "
            + "one exact old-shrine inspection token, concealed inscription, unchanged "
            + "discovery and environment state, and one-shot preservation.");
        GetTree().Quit();
    }

    private static SeekerCustomizationSelectionState SelectedState(
        SeekerCustomizationSelectionState initial,
        CharacterId characterId) => SeekerCustomizationSelectionStateTransition.Select(
            initial,
            new CharacterIdentity(characterId, "Mara"),
            new SeekerPronouns("they", "them", "their", "theirs", "themself"),
            initial.Options.AppearanceChoices[0],
            initial.Options.BackgroundChoices[1]);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
