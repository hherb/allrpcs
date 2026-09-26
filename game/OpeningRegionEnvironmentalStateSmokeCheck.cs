using Allrpcs.Application.Discovery;
using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Domain.EnvironmentalStates;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves inert baseline ownership and one exact inscription-driven variation.</summary>
public partial class OpeningRegionEnvironmentalStateSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_ENVIRONMENTAL_STATE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private static readonly Vector2 InscriptionAccessPosition = new(110.0f, 128.0f);

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        RunChecks();
    }

    private void RunChecks()
    {
        OpeningRegionProjectionSource projectionSource =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        OpeningRegionLayeredInscriptionDiscoveryCoordinator discoveryCoordinator =
            GetNode<OpeningRegionLayeredInscriptionDiscoveryCoordinator>(
                "../OpeningRegionLayeredInscriptionDiscoveryCoordinator");
        OpeningRegionEnvironmentalStateSource environmentalSource =
            GetNode<OpeningRegionEnvironmentalStateSource>(
                "../OpeningRegionEnvironmentalStateSource");
        OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator coordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator>(
                "../OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator");
        OpeningRegionEnvironmentalGreyboxPresentationController presentationController =
            GetNode<OpeningRegionEnvironmentalGreyboxPresentationController>(
                "../OpeningRegionEnvironmentalGreyboxPresentationController");
        Label presentationLabel = shrineSource.GetNode<Label>("EnvironmentalVariationStatus");
        OpeningRegionEnvironmentalRegionSet baseline = environmentalSource.BaselineRegions;
        OpeningRegionEnvironmentalRegionSet rebuiltBaseline =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projectionSource.Projection.Topology);
        object initialDiscoveryState = shrineSource.DiscoveryState;
        object? initialPresentation = shrineSource.Presentation;
        object? initialEvidenceExposure = shrineSource.EvidenceExposure;

        bool absentPresentation = coordinator.TryApply(
            shrineSource.Presentation,
            baseline,
            environmentalSource.CurrentRegions,
            coordinator.RevealedLightingState,
            coordinator.RevealedSoundState);
        bool rebuiltBaselineAccepted = coordinator.TryApply(
            shrineSource.Presentation,
            rebuiltBaseline,
            environmentalSource.CurrentRegions,
            coordinator.RevealedLightingState,
            coordinator.RevealedSoundState);
        if (absentPresentation
            || rebuiltBaselineAccepted
            || coordinator.CurrentVariation is not null
            || presentationController.CurrentPresentation is not null
            || presentationLabel.Visible
            || presentationLabel.Text.Length != 0
            || !ReferenceEquals(environmentalSource.CurrentRegions, baseline)
            || !ReferenceEquals(baseline.Topology, projectionSource.Projection.Topology)
            || !ReferenceEquals(shrineSource.DiscoveryState, initialDiscoveryState)
            || !ReferenceEquals(shrineSource.Presentation, initialPresentation)
            || !ReferenceEquals(shrineSource.EvidenceExposure, initialEvidenceExposure))
        {
            Fail("Ordinary startup or rejected input changed the environmental baseline.");
            return;
        }

        GetNode<CharacterBody2D>("../Seeker").GlobalPosition = InscriptionAccessPosition;
        if (!discoveryCoordinator.TryDiscover(shrineSource.KnowledgeableSeeker)
            || shrineSource.Presentation is not { } presentation)
        {
            Fail("The exact layered-inscription presentation was unavailable.");
            return;
        }

        OpeningRegionLayeredInscriptionDiscoveryState discoveryState = shrineSource.DiscoveryState;
        OpeningRegionLayeredInscriptionEvidenceExposure? evidenceExposure =
            shrineSource.EvidenceExposure;
        EnvironmentalState rebuiltLighting = new(
            coordinator.RevealedLightingState.Key,
            coordinator.RevealedLightingState.Description);
        bool rebuiltStateAccepted = coordinator.TryApply(
            presentation,
            baseline,
            baseline,
            rebuiltLighting,
            coordinator.RevealedSoundState);
        bool varied = coordinator.TryApply(
            presentation,
            baseline,
            baseline,
            coordinator.RevealedLightingState,
            coordinator.RevealedSoundState);
        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation =
            coordinator.CurrentVariation;
        OpeningRegionEnvironmentalRegionSet current = environmentalSource.CurrentRegions;
        bool repeated = coordinator.TryApply(
            presentation,
            baseline,
            baseline,
            coordinator.RevealedLightingState,
            coordinator.RevealedSoundState);
        Allrpcs.Domain.Locations.Location shrine =
            shrineSource.Binding.OldShrineAnchor.Location;
        OpeningRegionEnvironmentalLocationPresentation locationPresentation =
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(current, shrine);
        Allrpcs.Domain.Locations.Location clearing = current.Topology.Locations.Single(location =>
            location.Identity.Id.Value == "woodland-clearing");
        OpeningRegionEnvironmentalLocationPresentation clearingPresentation =
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(current, clearing);
        bool foreignLocationPresented = presentationController.TryPresent(
            current,
            clearing,
            clearingPresentation);
        bool presented = presentationController.TryPresent(
            current,
            shrine,
            locationPresentation);
        bool presentationRepeated = presentationController.TryPresent(
            current,
            shrine,
            locationPresentation);
        presentationLabel.Hide();
        bool hiddenPresentationInvalidated = presentationController.CurrentPresentation is null;
        presentationLabel.Show();
        bool restoredPresentationIsCurrent = ReferenceEquals(
            presentationController.CurrentPresentation,
            locationPresentation);

        if (rebuiltStateAccepted
            || !varied
            || repeated
            || foreignLocationPresented
            || !presented
            || presentationRepeated
            || !hiddenPresentationInvalidated
            || !restoredPresentationIsCurrent
            || variation is null
            || !ReferenceEquals(variation.BaselineRegions, baseline)
            || !ReferenceEquals(variation.Presentation, presentation)
            || !ReferenceEquals(variation.VariedRegions, current)
            || !ReferenceEquals(variation.RevealedLightingState, coordinator.RevealedLightingState)
            || !ReferenceEquals(variation.RevealedSoundState, coordinator.RevealedSoundState)
            || !ReferenceEquals(shrineSource.DiscoveryState, discoveryState)
            || !ReferenceEquals(shrineSource.Presentation, presentation)
            || !ReferenceEquals(shrineSource.EvidenceExposure, evidenceExposure)
            || !ReferenceEquals(
                presentationController.CurrentPresentation,
                locationPresentation)
            || !ReferenceEquals(locationPresentation.Regions, current)
            || !ReferenceEquals(locationPresentation.Location, shrine)
            || !ReferenceEquals(
                locationPresentation.Region,
                current.Regions.Single(region => region.Key == "woodland-edge"))
            || !presentationLabel.Visible
            || presentationLabel.Text != locationPresentation.Text
            || !VerifyRegions(baseline, current, coordinator))
        {
            Fail("The exact environmental variation lost authority or retained state.");
            return;
        }

        GD.Print(
            "Opening-region environmental-state smoke passed: inert exact baseline ownership, "
            + "absent/rebuilt input rejection, one exact inscription-authorized compare-and-swap, "
            + "repeated rejection, retained weather/non-woodland regions, unchanged discovery and "
            + "evidence state, and one exact location-scoped canonical greybox presentation.");
        GetTree().Quit();
    }

    private static bool VerifyRegions(
        OpeningRegionEnvironmentalRegionSet baseline,
        OpeningRegionEnvironmentalRegionSet current,
        OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator coordinator)
    {
        OpeningRegionEnvironmentalRegion baselineWoodland = baseline.Regions.Single(region =>
            region.Key == "woodland-edge");
        OpeningRegionEnvironmentalRegion currentWoodland = current.Regions.Single(region =>
            region.Key == "woodland-edge");
        return ReferenceEquals(current.Topology, baseline.Topology)
            && ReferenceEquals(currentWoodland.WeatherState, baselineWoodland.WeatherState)
            && ReferenceEquals(currentWoodland.LightingState, coordinator.RevealedLightingState)
            && ReferenceEquals(currentWoodland.SoundState, coordinator.RevealedSoundState)
            && baseline.Regions.Where(region => region.Key != "woodland-edge").All(region =>
                ReferenceEquals(
                    region,
                    current.Regions.Single(candidate => candidate.Key == region.Key)));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
