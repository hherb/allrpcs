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
            return false;
        }

        try
        {
            OpeningRegionLayeredInscriptionEnvironmentalVariation variation =
                OpeningRegionLayeredInscriptionEnvironmentalVariationTransition.Create(
                    baselineRegions,
                    presentation,
                    revealedLightingState,
                    revealedSoundState);
            if (!EnvironmentalSource.TryInstallVariation(
                    baselineRegions,
                    currentRegions,
                    presentation,
                    variation))
            {
                return false;
            }

            CurrentVariation = variation;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
