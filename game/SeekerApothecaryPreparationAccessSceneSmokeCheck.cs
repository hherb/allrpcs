using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves exact Apothecary selection installs inert preparation access.</summary>
public partial class SeekerApothecaryPreparationAccessSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_APOTHECARY_PREPARATION_ACCESS_CHECK";
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
        OpeningRegionRemedyPreparationSceneSource preparationSource =
            GetNode<OpeningRegionRemedyPreparationSceneSource>(
                "../OpeningRegionRemedyPreparationSceneSource");
        FirstPatientPreparationController preparation =
            GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        OpeningRegionClosedForestPathSceneSource closedPathSource =
            GetNode<OpeningRegionClosedForestPathSceneSource>(
                "../OpeningRegionClosedForestPathSceneSource");
        OpeningRegionOldShrineSceneSource shrineSource =
            GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource");
        SeekerCustomizationSelectionState initialCustomization =
            customizationSource.CurrentState;
        SeekerBackgroundRouteVariationState initialRoutes = routeSource.CurrentState;
        Character initialCharacter = characterSource.Character;
        OpeningRegionRemedyPreparationSceneBinding? binding = preparationSource.Binding;

        if (binding is null
            || preparationSource.CurrentPreparationAccess is not null
            || preparation.Activity is not null
            || preparation.Outcome is not null)
        {
            Fail("Apothecary preparation access did not retain inert startup state.");
            return;
        }

        Candidate forester = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 0);
        Candidate scribe = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 1);
        Candidate apothecary = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 2);
        Candidate traveller = CandidateFor(
            initialCustomization,
            initialRoutes,
            initialCharacter,
            backgroundIndex: 3);
        OpeningRegionRemedyPreparationAccess candidateAccess =
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                apothecary.RouteAccess,
                apothecary.RouteAccess,
                binding,
                binding,
                issuedAccess: null)!;
        SeekerBackgroundRouteVariationState rebuiltInitial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(
                customizationSource.Options,
                initialRoutes.OpeningRegion);
        Candidate rebuilt = CandidateFor(
            initialCustomization,
            rebuiltInitial,
            initialCharacter,
            backgroundIndex: 2);
        OpeningRegionRemedyPreparationAccess rebuiltAccess =
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                rebuilt.RouteAccess,
                rebuilt.RouteAccess,
                binding,
                binding,
                issuedAccess: null)!;
        OpeningRegionRemedyPreparationSceneSource preBindingSource = new();
        bool preBindingInstalled = preBindingSource.TryInstallPreparationAccess(
            apothecary.RouteState,
            candidateAccess);
        preBindingSource.Free();

        if (preparationSource.TryInstallPreparationAccess(
                forester.RouteState,
                candidateAccess)
            || preparationSource.TryInstallPreparationAccess(
                scribe.RouteState,
                candidateAccess)
            || preparationSource.TryInstallPreparationAccess(
                traveller.RouteState,
                candidateAccess)
            || preparationSource.TryInstallPreparationAccess(initialRoutes, candidateAccess)
            || preparationSource.TryInstallPreparationAccess(apothecary.RouteState, null)
            || preBindingInstalled
            || preparationSource.CurrentPreparationAccess is not null)
        {
            Fail("Non-Apothecary, stale, rebuilt, null, or pre-binding access was installed.");
            return;
        }

        object? initialRecipe = preparation.Recipe;
        object? initialIngredients = preparation.Ingredients;
        if (!coordinator.TryApply(initialCustomization, apothecary.Selection)
            || !ReferenceEquals(customizationSource.CurrentState, apothecary.Selection)
            || !ReferenceEquals(routeSource.CurrentState.PreviousState, initialRoutes)
            || !ReferenceEquals(
                routeSource.CurrentState.SelectedVariation,
                initialRoutes.Variations[2])
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.RouteState,
                routeSource.CurrentState)
            || !ReferenceEquals(
                routeAccessSource.CurrentAccess?.DestinationAnchor,
                binding.GardenAnchor)
            || !ReferenceEquals(
                preparationSource.CurrentPreparationAccess?.RouteAccess,
                routeAccessSource.CurrentAccess)
            || !ReferenceEquals(
                preparationSource.CurrentPreparationAccess?.ApothecaryVariation,
                routeSource.CurrentState.SelectedVariation)
            || !ReferenceEquals(
                preparationSource.CurrentPreparationAccess?.SceneBinding,
                binding)
            || !ReferenceEquals(
                preparationSource.CurrentPreparationAccess?.TreatmentContext,
                binding.TreatmentContext)
            || !ReferenceEquals(
                preparationSource.CurrentPreparationAccess?.Recipe,
                binding.Recipe)
            || !ReferenceEquals(preparation.Recipe, initialRecipe)
            || !ReferenceEquals(preparation.Ingredients, initialIngredients)
            || preparation.Activity is not null
            || preparation.Outcome is not null
            || closedPathSource.CurrentAccess is not null
            || shrineSource.CurrentInspectionAccess is not null)
        {
            Fail("Exact Apothecary selection did not install inert preparation access.");
            return;
        }

        OpeningRegionRemedyPreparationAccess installed =
            preparationSource.CurrentPreparationAccess!;
        SeekerBackgroundRouteVariationState installedRoutes = routeSource.CurrentState;
        Character installedCharacter = characterSource.Character;
        if (coordinator.TryApply(initialCustomization, apothecary.Selection)
            || preparationSource.TryInstallPreparationAccess(
                apothecary.RouteState,
                candidateAccess)
            || preparationSource.TryInstallPreparationAccess(
                rebuilt.RouteState,
                rebuiltAccess)
            || !ReferenceEquals(preparationSource.CurrentPreparationAccess, installed)
            || !ReferenceEquals(routeSource.CurrentState, installedRoutes)
            || !ReferenceEquals(characterSource.Character, installedCharacter)
            || !ReferenceEquals(preparation.Recipe, initialRecipe)
            || !ReferenceEquals(preparation.Ingredients, initialIngredients)
            || preparation.Activity is not null
            || preparation.Outcome is not null)
        {
            Fail("Repeated or rebuilt Apothecary input changed preparation access or state.");
            return;
        }

        GD.Print(
            "Apothecary preparation access smoke passed: inert startup, rejected Forester, "
            + "Scribe, Traveller, null, stale, rebuilt, and pre-binding inputs, one exact "
            + "Apothecary token, unchanged preparation state, and one-shot preservation.");
        GetTree().Quit();
    }

    private static Candidate CandidateFor(
        SeekerCustomizationSelectionState initialCustomization,
        SeekerBackgroundRouteVariationState initialRoutes,
        Character currentCharacter,
        int backgroundIndex)
    {
        SeekerCustomizationSelectionState selection =
            SeekerCustomizationSelectionStateTransition.Select(
                initialCustomization,
                new CharacterIdentity(currentCharacter.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                initialCustomization.Options.AppearanceChoices[0],
                initialCustomization.Options.BackgroundChoices[backgroundIndex]);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selection, currentCharacter);
        SeekerBackgroundRouteVariationState routeState =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initialRoutes,
                selection,
                application);
        SeekerBackgroundRouteAccess routeAccess =
            SeekerBackgroundRouteAccessPolicy.Authorize(
                routeState,
                routeState,
                routeState.SelectedVariation!.DestinationAnchor,
                issuedAccess: null)!;
        return new Candidate(selection, routeState, routeAccess);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed record Candidate(
        SeekerCustomizationSelectionState Selection,
        SeekerBackgroundRouteVariationState RouteState,
        SeekerBackgroundRouteAccess RouteAccess);
}
