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
    private const string LayeredInscriptionDiscoverAction = "layered_inscription_discover";
    private const string UnrelatedAction = "opening_region_environment_unrelated";
    private static readonly Vector2 InscriptionAccessPosition = new(110.0f, 128.0f);

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
        OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator variationCoordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator>(
                "../OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator");
        OpeningRegionEnvironmentalGreyboxPresentationController presentationController =
            GetNode<OpeningRegionEnvironmentalGreyboxPresentationController>(
                "../OpeningRegionEnvironmentalGreyboxPresentationController");
        OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator flowCoordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator>(
                "../OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator");
        Main main = GetNode<Main>("..");
        Label presentationLabel = shrineSource.GetNode<Label>("EnvironmentalVariationStatus");
        OpeningRegionEnvironmentalRegionSet baseline = environmentalSource.BaselineRegions;
        OpeningRegionEnvironmentalRegionSet rebuiltBaseline =
            RepresentativeOpeningRegionEnvironmentalSource.Create(projectionSource.Projection.Topology);
        object initialDiscoveryState = shrineSource.DiscoveryState;
        object? initialPresentation = shrineSource.Presentation;
        object? initialEvidenceExposure = shrineSource.EvidenceExposure;

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
        bool nullFlow = flowCoordinator.TryComplete(
            null,
            baseline,
            environmentalSource.CurrentRegions,
            variationCoordinator.RevealedLightingState,
            variationCoordinator.RevealedSoundState);
        bool absentPresentation = flowCoordinator.TryComplete(
            shrineSource.Presentation,
            baseline,
            environmentalSource.CurrentRegions,
            variationCoordinator.RevealedLightingState,
            variationCoordinator.RevealedSoundState);
        bool rebuiltBaselineAccepted = flowCoordinator.TryComplete(
            shrineSource.Presentation,
            rebuiltBaseline,
            environmentalSource.CurrentRegions,
            variationCoordinator.RevealedLightingState,
            variationCoordinator.RevealedSoundState);
        if (nullFlow
            || absentPresentation
            || rebuiltBaselineAccepted
            || flowCoordinator.CompletedFlow is not null
            || variationCoordinator.CurrentVariation is not null
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
        main._UnhandledInput(new InputEventAction
        {
            Action = LayeredInscriptionDiscoverAction,
            Pressed = true,
        });
        if (shrineSource.Presentation is not { } presentation
            || flowCoordinator.CompletedFlow is not { } flow)
        {
            Fail("The ordinary I-key path did not complete the exact environmental flow.");
            return;
        }

        OpeningRegionLayeredInscriptionDiscoveryState discoveryState = shrineSource.DiscoveryState;
        OpeningRegionLayeredInscriptionEvidenceExposure? evidenceExposure =
            shrineSource.EvidenceExposure;
        EnvironmentalState rebuiltLighting = new(
            variationCoordinator.RevealedLightingState.Key,
            variationCoordinator.RevealedLightingState.Description);
        OpeningRegionEnvironmentalRegionSet current = environmentalSource.CurrentRegions;
        OpeningRegionLayeredInscriptionEnvironmentalVariation variation = flow.Variation;
        OpeningRegionEnvironmentalLocationPresentation locationPresentation =
            flow.LocationPresentation;
        bool rebuiltStateAccepted = flowCoordinator.TryComplete(
            presentation,
            baseline,
            current,
            rebuiltLighting,
            variationCoordinator.RevealedSoundState);
        bool staleCurrentAccepted = flowCoordinator.TryComplete(
            presentation,
            baseline,
            baseline,
            variationCoordinator.RevealedLightingState,
            variationCoordinator.RevealedSoundState);
        bool repeated = flowCoordinator.TryComplete(
            presentation,
            baseline,
            current,
            variationCoordinator.RevealedLightingState,
            variationCoordinator.RevealedSoundState);
        Allrpcs.Domain.Locations.Location shrine =
            shrineSource.Binding.OldShrineAnchor.Location;
        Allrpcs.Domain.Locations.Location clearing = current.Topology.Locations.Single(location =>
            location.Identity.Id.Value == "woodland-clearing");
        OpeningRegionEnvironmentalLocationPresentation clearingPresentation =
            OpeningRegionEnvironmentalLocationPresentationProjection.Project(current, clearing);
        bool foreignLocationPresented = presentationController.TryPresent(
            current,
            clearing,
            clearingPresentation);
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
            || staleCurrentAccepted
            || repeated
            || foreignLocationPresented
            || presentationRepeated
            || !hiddenPresentationInvalidated
            || !restoredPresentationIsCurrent
            || !ReferenceEquals(flowCoordinator.CompletedFlow, flow)
            || !ReferenceEquals(flow.Presentation, presentation)
            || !ReferenceEquals(flow.Variation, variation)
            || !ReferenceEquals(flow.LocationPresentation, locationPresentation)
            || !ReferenceEquals(variation.BaselineRegions, baseline)
            || !ReferenceEquals(variation.Presentation, presentation)
            || !ReferenceEquals(variation.VariedRegions, current)
            || !ReferenceEquals(
                variation.RevealedLightingState,
                variationCoordinator.RevealedLightingState)
            || !ReferenceEquals(
                variation.RevealedSoundState,
                variationCoordinator.RevealedSoundState)
            || !ReferenceEquals(variationCoordinator.CurrentVariation, variation)
            || !ReferenceEquals(shrineSource.DiscoveryState, discoveryState)
            || !ReferenceEquals(shrineSource.Presentation, presentation)
            || !ReferenceEquals(shrineSource.EvidenceExposure, evidenceExposure)
            || !ReferenceEquals(discoveryCoordinator.CompletedExposure, evidenceExposure)
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
            || !VerifyRegions(baseline, current, variationCoordinator))
        {
            Fail("The exact environmental variation lost authority or retained state.");
            return;
        }

        GD.Print(
            "Opening-region environmental-state smoke passed: inert exact baseline ownership, "
            + "released/unrelated/null/absent/rebuilt input rejection, one ordinary I-key exact "
            + "discovery-to-variation-to-label flow, stale/already-varied/already-presented/repeated "
            + "rejection, retained weather/non-woodland regions, unchanged exact discovery and "
            + "evidence state, and one exact completed flow token.");
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
