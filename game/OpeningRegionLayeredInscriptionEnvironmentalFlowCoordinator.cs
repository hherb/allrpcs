using Allrpcs.Application.Discovery;
using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Completes one exact discovered-inscription environmental presentation flow.</summary>
public partial class OpeningRegionLayeredInscriptionEnvironmentalFlowCoordinator : Node
{
    /// <summary>Gets the exact scene-owned qualitative environmental source.</summary>
    public OpeningRegionEnvironmentalStateSource EnvironmentalSource { get; private set; } = null!;

    /// <summary>Gets the exact scene-owned old-shrine source.</summary>
    public OpeningRegionOldShrineSceneSource ShrineSource { get; private set; } = null!;

    /// <summary>Gets the exact scene-owned variation coordinator.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator VariationCoordinator
    {
        get;
        private set;
    } = null!;

    /// <summary>Gets the exact scene-owned environmental presenter.</summary>
    public OpeningRegionEnvironmentalGreyboxPresentationController PresentationController
    {
        get;
        private set;
    } = null!;

    /// <summary>Gets the exact completed flow token, or null before successful completion.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalFlow? CompletedFlow { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        EnvironmentalSource = GetNode<OpeningRegionEnvironmentalStateSource>(
            "../OpeningRegionEnvironmentalStateSource");
        ShrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        VariationCoordinator =
            GetNode<OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator>(
                "../OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator");
        PresentationController =
            GetNode<OpeningRegionEnvironmentalGreyboxPresentationController>(
                "../OpeningRegionEnvironmentalGreyboxPresentationController");
        CompletedFlow = null;
    }

    /// <summary>Completes the flow from the scene's current shrine and revealed variation state.</summary>
    public bool TryCompleteRevealed() =>
        TryComplete(
            ShrineSource.Presentation,
            EnvironmentalSource.BaselineRegions,
            EnvironmentalSource.CurrentRegions,
            VariationCoordinator.RevealedLightingState,
            VariationCoordinator.RevealedSoundState);

    /// <summary>Preflights and completes one exact discovery-authorized environmental flow.</summary>
    public bool TryComplete(
        OpeningRegionLayeredInscriptionPresentation? presentation,
        OpeningRegionEnvironmentalRegionSet? baselineRegions,
        OpeningRegionEnvironmentalRegionSet? currentRegions,
        EnvironmentalState? revealedLightingState,
        EnvironmentalState? revealedSoundState)
    {
        if (presentation is null
            || baselineRegions is null
            || currentRegions is null
            || revealedLightingState is null
            || revealedSoundState is null
            || CompletedFlow is not null
            || !ReferenceEquals(ShrineSource.Presentation, presentation)
            || !ReferenceEquals(EnvironmentalSource.BaselineRegions, baselineRegions)
            || !ReferenceEquals(EnvironmentalSource.CurrentRegions, currentRegions)
            || !ReferenceEquals(VariationCoordinator.RevealedLightingState, revealedLightingState)
            || !ReferenceEquals(VariationCoordinator.RevealedSoundState, revealedSoundState))
        {
            return false;
        }

        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation =
            VariationCoordinator.CreateVariation(
                presentation,
                baselineRegions,
                currentRegions,
                revealedLightingState,
                revealedSoundState);
        if (variation is null)
        {
            return false;
        }

        Location shrine = ShrineSource.Binding.OldShrineAnchor.Location;
        OpeningRegionEnvironmentalLocationPresentation locationPresentation;
        try
        {
            locationPresentation = OpeningRegionEnvironmentalLocationPresentationProjection.Project(
                variation.VariedRegions,
                shrine);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (!PresentationController.CanPresentAfterRegionReplacement(
                currentRegions,
                variation.VariedRegions,
                shrine,
                locationPresentation))
        {
            return false;
        }

        if (!VariationCoordinator.TryApply(currentRegions, variation)
            || !PresentationController.TryPresent(
                variation.VariedRegions,
                shrine,
                locationPresentation)
            || !ReferenceEquals(EnvironmentalSource.CurrentRegions, variation.VariedRegions)
            || !ReferenceEquals(VariationCoordinator.CurrentVariation, variation)
            || !ReferenceEquals(
                PresentationController.CurrentPresentation,
                locationPresentation))
        {
            throw new InvalidOperationException(
                "A preflighted inscription environmental flow must complete exactly once.");
        }

        CompletedFlow = new OpeningRegionLayeredInscriptionEnvironmentalFlow(
            presentation,
            variation,
            locationPresentation);
        return true;
    }
}
