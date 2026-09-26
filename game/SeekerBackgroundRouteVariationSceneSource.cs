using Allrpcs.Application.Navigation;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the representative optional-route catalogue and one selected variation.</summary>
public partial class SeekerBackgroundRouteVariationSceneSource : Node
{
    /// <summary>Gets the exact empty or selected route-variation state.</summary>
    public SeekerBackgroundRouteVariationState CurrentState { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        SeekerCustomizationSelectionSceneSource customizationSource =
            GetNode<SeekerCustomizationSelectionSceneSource>(
                "../SeekerCustomizationSelectionSceneSource");
        OpeningRegionProjection projection = GetNode<OpeningRegionProjectionSource>(
            "../OpeningRegionProjectionSource").Projection;
        CurrentState = SeekerBackgroundRouteVariationStateTransition.Initialize(
            customizationSource.Options,
            projection);
    }

    /// <summary>Reports whether an exact one-shot route-state transition can be installed.</summary>
    public bool CanInstallSelection(
        SeekerBackgroundRouteVariationState? expectedCurrent,
        SeekerBackgroundRouteVariationState? selectedState) =>
        ReferenceEquals(CurrentState, expectedCurrent)
        && expectedCurrent.SelectedVariation is null
        && selectedState?.SelectedVariation is not null
        && ReferenceEquals(selectedState.PreviousState, expectedCurrent)
        && ReferenceEquals(selectedState.OpeningRegion, expectedCurrent.OpeningRegion)
        && ReferenceEquals(selectedState.Variations, expectedCurrent.Variations);

    /// <summary>Installs one exact selected route state through compare-and-swap.</summary>
    public bool TryInstallSelection(
        SeekerBackgroundRouteVariationState? expectedCurrent,
        SeekerBackgroundRouteVariationState? selectedState)
    {
        if (!CanInstallSelection(expectedCurrent, selectedState))
        {
            return false;
        }

        CurrentState = selectedState!;
        return true;
    }
}
