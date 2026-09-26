using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Atomically installs one exact Seeker selection and projected character.</summary>
public partial class SeekerCustomizationApplicationCoordinator : Node
{
    private SeekerCustomizationSelectionSceneSource selectionSource = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private SeekerBackgroundRouteVariationSceneSource routeSource = null!;
    private SeekerBackgroundRouteAccessSceneSource routeAccessSource = null!;
    private OpeningRegionClosedForestPathSceneSource closedForestPathSource = null!;
    private OpeningRegionOldShrineSceneSource oldShrineSource = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        selectionSource = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../SeekerCustomizationSelectionSceneSource");
        characterSource = GetNode<TemporarySeekerCharacterSource>(
            "../TemporarySeekerCharacterSource");
        routeSource = GetNode<SeekerBackgroundRouteVariationSceneSource>(
            "../SeekerBackgroundRouteVariationSceneSource");
        routeAccessSource = GetNode<SeekerBackgroundRouteAccessSceneSource>(
            "../SeekerBackgroundRouteAccessSceneSource");
        closedForestPathSource = GetNode<OpeningRegionClosedForestPathSceneSource>(
            "../OpeningRegionClosedForestPathSceneSource");
        oldShrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
    }

    /// <summary>Installs exact preflighted selection and character states in one scene call.</summary>
    public bool TryApply(
        SeekerCustomizationSelectionState? expectedCurrent,
        SeekerCustomizationSelectionState? selectedState)
    {
        Character currentCharacter = characterSource.Character;
        if (expectedCurrent is null || selectedState?.SelectedProfile is null)
        {
            return false;
        }

        if (!ReferenceEquals(selectionSource.CurrentState, expectedCurrent)
            || expectedCurrent.SelectedProfile is not null
            || !ReferenceEquals(expectedCurrent.Options, selectionSource.Options)
            || !ReferenceEquals(selectedState.Options, selectionSource.Options)
            || !ReferenceEquals(selectedState.PreviousState, expectedCurrent)
            || characterSource.CustomizationApplication is not null
            || !ReferenceEquals(
                selectedState.SelectedProfile.Identity.Id,
                currentCharacter.Identity.Id)
            || ReferenceEquals(
                selectedState.SelectedProfile.Identity,
                currentCharacter.Identity)
            || routeSource.CurrentState.SelectedVariation is not null)
        {
            return false;
        }

        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selectedState, currentCharacter);
        SeekerBackgroundRouteVariationState currentRoutes = routeSource.CurrentState;
        SeekerBackgroundRouteVariationState selectedRoutes =
            SeekerBackgroundRouteVariationStateTransition.Select(
                currentRoutes,
                selectedState,
                application);
        if (!routeSource.CanInstallSelection(currentRoutes, selectedRoutes))
        {
            return false;
        }

        SeekerBackgroundRouteAccess? routeAccess =
            SeekerBackgroundRouteAccessPolicy.Authorize(
                selectedRoutes,
                selectedRoutes,
                selectedRoutes.SelectedVariation!.DestinationAnchor,
                routeAccessSource.CurrentAccess);
        if (routeAccess is null
            || !routeAccessSource.CanInstallAccess(selectedRoutes, routeAccess))
        {
            return false;
        }

        OpeningRegionClosedForestPathAccess? closedPathAccess = null;
        if (ReferenceEquals(
                routeAccess.DestinationAnchor,
                closedForestPathSource.Binding.ClosedForestPathAnchor))
        {
            closedPathAccess = new OpeningRegionClosedForestPathAccess(
                closedForestPathSource.Binding,
                routeAccess);
            if (!closedForestPathSource.CanInstallAccess(
                    selectedRoutes,
                    closedPathAccess))
            {
                return false;
            }
        }

        OpeningRegionOldShrineInspectionAccess? inspectionAccess = null;
        if (ReferenceEquals(
                routeAccess.DestinationAnchor,
                oldShrineSource.Binding.OldShrineAnchor))
        {
            inspectionAccess = OpeningRegionOldShrineInspectionAccessPolicy.Authorize(
                routeAccess,
                routeAccess,
                oldShrineSource.Binding,
                oldShrineSource.Binding,
                oldShrineSource.InscriptionBinding,
                oldShrineSource.InscriptionBinding,
                oldShrineSource.CurrentInspectionAccess);
            if (inspectionAccess is null
                || !oldShrineSource.CanInstallInspectionAccess(
                    selectedRoutes,
                    inspectionAccess))
            {
                return false;
            }
        }

        if (!selectionSource.TryInstallSelection(expectedCurrent, selectedState))
        {
            return false;
        }

        if (!characterSource.TryApplyCustomization(application)
            || !routeSource.TryInstallSelection(currentRoutes, selectedRoutes)
            || !routeAccessSource.TryInstallAccess(selectedRoutes, routeAccess))
        {
            return false;
        }

        if (closedPathAccess is not null
            && !closedForestPathSource.TryInstallAccess(selectedRoutes, closedPathAccess))
        {
            return false;
        }

        return inspectionAccess is null
            || oldShrineSource.TryInstallInspectionAccess(selectedRoutes, inspectionAccess);
    }
}
