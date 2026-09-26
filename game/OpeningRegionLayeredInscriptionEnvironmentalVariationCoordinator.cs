using Allrpcs.Application.Discovery;
using Allrpcs.Application.EnvironmentalStates;
using Allrpcs.Domain.EnvironmentalStates;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates one exact scene-owned inscription environmental variation.</summary>
public partial class OpeningRegionLayeredInscriptionEnvironmentalVariationCoordinator : Node
{
    /// <summary>Gets the exact scene-owned qualitative environmental source.</summary>
    public OpeningRegionEnvironmentalStateSource EnvironmentalSource { get; private set; } = null!;

    /// <summary>Gets the exact scene-owned old-shrine source.</summary>
    public OpeningRegionOldShrineSceneSource ShrineSource { get; private set; } = null!;

    /// <summary>Gets the exact authored woodland lighting replacement.</summary>
    public EnvironmentalState RevealedLightingState { get; private set; } = null!;

    /// <summary>Gets the exact authored woodland sound replacement.</summary>
    public EnvironmentalState RevealedSoundState { get; private set; } = null!;

    /// <summary>Gets the exact installed variation, or null before explicit application.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalVariation? CurrentVariation
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public override void _Ready()
    {
        EnvironmentalSource = GetNode<OpeningRegionEnvironmentalStateSource>(
            "../OpeningRegionEnvironmentalStateSource");
        ShrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        RevealedLightingState = new EnvironmentalState(
            "inscription-revealed-light",
            "The identified layered inscription catches a restrained greybox highlight.");
        RevealedSoundState = new EnvironmentalState(
            "inscription-revealed-sound",
            "The woodland rain thins around the identified layered inscription.");
        CurrentVariation = null;
    }

    /// <summary>Attempts one exact variation while leaving every rejected path unchanged.</summary>
    public bool TryApply(
        OpeningRegionLayeredInscriptionPresentation? presentation,
        OpeningRegionEnvironmentalRegionSet? baselineRegions,
        OpeningRegionEnvironmentalRegionSet? currentRegions,
        EnvironmentalState? revealedLightingState,
        EnvironmentalState? revealedSoundState)
    {
        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation = CreateVariation(
            presentation,
            baselineRegions,
            currentRegions,
            revealedLightingState,
            revealedSoundState);
        return TryApply(currentRegions, variation);
    }

    /// <summary>Prepares one exact variation without changing scene-owned state.</summary>
    public OpeningRegionLayeredInscriptionEnvironmentalVariation? CreateVariation(
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
            || CurrentVariation is not null
            || !ReferenceEquals(ShrineSource.Presentation, presentation)
            || !ReferenceEquals(EnvironmentalSource.BaselineRegions, baselineRegions)
            || !ReferenceEquals(EnvironmentalSource.CurrentRegions, currentRegions)
            || !ReferenceEquals(RevealedLightingState, revealedLightingState)
            || !ReferenceEquals(RevealedSoundState, revealedSoundState))
        {
            return null;
        }

        try
        {
            OpeningRegionLayeredInscriptionEnvironmentalVariation variation =
                OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                    baselineRegions,
                    presentation,
                    revealedLightingState,
                    revealedSoundState);
            return OpeningRegionLayeredInscriptionEnvironmentalVariationInstallationPolicy
                .CanInstall(baselineRegions, currentRegions, presentation, variation)
                    ? variation
                    : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>Installs one exact prepared variation against the expected current snapshot.</summary>
    public bool TryApply(
        OpeningRegionEnvironmentalRegionSet? expectedCurrent,
        OpeningRegionLayeredInscriptionEnvironmentalVariation? variation)
    {
        if (expectedCurrent is null
            || variation is null
            || CurrentVariation is not null
            || !ReferenceEquals(EnvironmentalSource.BaselineRegions, variation.BaselineRegions)
            || !ReferenceEquals(EnvironmentalSource.CurrentRegions, expectedCurrent)
            || !ReferenceEquals(ShrineSource.Presentation, variation.Presentation)
            || !ReferenceEquals(RevealedLightingState, variation.RevealedLightingState)
            || !ReferenceEquals(RevealedSoundState, variation.RevealedSoundState)
            || !OpeningRegionLayeredInscriptionEnvironmentalVariationInstallationPolicy.CanInstall(
                variation.BaselineRegions,
                expectedCurrent,
                variation.Presentation,
                variation))
        {
            return false;
        }

        if (!EnvironmentalSource.TryInstallVariation(
                variation.BaselineRegions,
                expectedCurrent,
                variation.Presentation,
                variation))
        {
            throw new InvalidOperationException(
                "A preflighted inscription environmental variation must install exactly once.");
        }

        CurrentVariation = variation;
        return true;
    }
}
