using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Atomically installs one exact Seeker selection and projected character.</summary>
public partial class SeekerCustomizationApplicationCoordinator : Node
{
    private const string ApothecaryBackgroundKey = "apothecarys-assistant";

    private SeekerCustomizationSelectionSceneSource selectionSource = null!;
    private TemporarySeekerCharacterSource characterSource = null!;
    private SeekerBackgroundRouteVariationSceneSource routeSource = null!;
    private SeekerBackgroundRouteAccessSceneSource routeAccessSource = null!;
    private OpeningRegionClosedForestPathSceneSource closedForestPathSource = null!;
    private OpeningRegionOldShrineSceneSource oldShrineSource = null!;
    private OpeningRegionRemedyPreparationSceneSource remedyPreparationSource = null!;
    private OpeningRegionSouthernRoadWagonSceneSource southernRoadWagonSource = null!;

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
        remedyPreparationSource = GetNode<OpeningRegionRemedyPreparationSceneSource>(
            "../OpeningRegionRemedyPreparationSceneSource");
        southernRoadWagonSource = GetNode<OpeningRegionSouthernRoadWagonSceneSource>(
            "../OpeningRegionSouthernRoadWagonSceneSource");
    }

    /// <summary>Installs exact preflighted selection and character states in one scene call.</summary>
    public bool TryApply(
        SeekerCustomizationSelectionState? expectedCurrent,
        SeekerCustomizationSelectionState? selectedState)
    {
        if (Prepare(expectedCurrent, selectedState) is not { } installation) { return false; }
        Install(installation);
        return true;
    }

    /// <summary>
    /// Runs every check without changing the scene; null when the selection cannot be installed.
    /// </summary>
    internal SeekerCustomizationInstallation? Prepare(
        SeekerCustomizationSelectionState? expectedCurrent,
        SeekerCustomizationSelectionState? selectedState)
    {
        Character currentCharacter = characterSource.Character;
        if (expectedCurrent is null || selectedState?.SelectedProfile is null)
        {
            return null;
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
            return null;
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
            return null;
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
            return null;
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
                return null;
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
                return null;
            }
        }

        OpeningRegionRemedyPreparationAccess? preparationAccess = null;
        OpeningRegionRemedyPreparationSceneBinding? preparationBinding =
            remedyPreparationSource.Binding;
        if (selectedRoutes.SelectedVariation!.Background.Key == ApothecaryBackgroundKey)
        {
            if (preparationBinding is null
                || !ReferenceEquals(
                    routeAccess.DestinationAnchor,
                    preparationBinding.GardenAnchor))
            {
                return null;
            }

            preparationAccess = OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                routeAccess,
                routeAccess,
                preparationBinding,
                preparationBinding,
                remedyPreparationSource.CurrentPreparationAccess);
            if (preparationAccess is null
                || !remedyPreparationSource.CanInstallPreparationAccess(
                    selectedRoutes,
                    preparationAccess))
            {
                return null;
            }
        }

        OpeningRegionSouthernRoadWagonAccess? travellerAccess = null;
        if (ReferenceEquals(
                routeAccess.DestinationAnchor,
                southernRoadWagonSource.Binding.SouthernRoadAnchor))
        {
            travellerAccess = OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                routeAccess,
                routeAccess,
                southernRoadWagonSource.Binding,
                southernRoadWagonSource.Binding,
                southernRoadWagonSource.CurrentTravellerAccess);
            if (travellerAccess is null
                || !southernRoadWagonSource.CanInstallTravellerAccess(
                    selectedRoutes,
                    travellerAccess))
            {
                return null;
            }
        }

        return new SeekerCustomizationInstallation(
            expectedCurrent,
            selectedState,
            application,
            currentRoutes,
            selectedRoutes,
            routeAccess,
            closedPathAccess,
            inspectionAccess,
            preparationAccess,
            travellerAccess);
    }

    /// <summary>
    /// Installs one prepared selection. Throws if the scene changed since Prepare, which no caller
    /// allows; steps installed before the failing one are not rolled back.
    /// </summary>
    internal void Install(SeekerCustomizationInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);
        (SeekerCustomizationSelectionState expectedCurrent,
            SeekerCustomizationSelectionState selectedState,
            SeekerCustomizedCharacterApplication application,
            SeekerBackgroundRouteVariationState currentRoutes,
            SeekerBackgroundRouteVariationState selectedRoutes,
            SeekerBackgroundRouteAccess routeAccess,
            OpeningRegionClosedForestPathAccess? closedPathAccess,
            OpeningRegionOldShrineInspectionAccess? inspectionAccess,
            OpeningRegionRemedyPreparationAccess? preparationAccess,
            OpeningRegionSouthernRoadWagonAccess? travellerAccess) = installation;
        bool installed = selectionSource.TryInstallSelection(expectedCurrent, selectedState)
            && characterSource.TryApplyCustomization(application)
            && routeSource.TryInstallSelection(currentRoutes, selectedRoutes)
            && routeAccessSource.TryInstallAccess(selectedRoutes, routeAccess)
            && (closedPathAccess is null
                || closedForestPathSource.TryInstallAccess(selectedRoutes, closedPathAccess))
            && (inspectionAccess is null
                || oldShrineSource.TryInstallInspectionAccess(selectedRoutes, inspectionAccess))
            && (preparationAccess is null
                || remedyPreparationSource.TryInstallPreparationAccess(selectedRoutes, preparationAccess))
            && (travellerAccess is null
                || southernRoadWagonSource.TryInstallTravellerAccess(selectedRoutes, travellerAccess));
        if (!installed)
        {
            throw new InvalidOperationException(
                "The scene changed after the Seeker customization was prepared.");
        }
    }
}

/// <summary>Every value a prepared Seeker selection installs, computed without changing the scene.</summary>
internal sealed record SeekerCustomizationInstallation(
    SeekerCustomizationSelectionState ExpectedCurrent,
    SeekerCustomizationSelectionState SelectedState,
    SeekerCustomizedCharacterApplication Application,
    SeekerBackgroundRouteVariationState CurrentRoutes,
    SeekerBackgroundRouteVariationState SelectedRoutes,
    SeekerBackgroundRouteAccess RouteAccess,
    OpeningRegionClosedForestPathAccess? ClosedPathAccess,
    OpeningRegionOldShrineInspectionAccess? InspectionAccess,
    OpeningRegionRemedyPreparationAccess? PreparationAccess,
    OpeningRegionSouthernRoadWagonAccess? TravellerAccess);
