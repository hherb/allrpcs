using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionRemedyPreparationAccessPolicyTests
{
    [Fact]
    public void AuthorizesOneExactApothecaryPreparationWithoutStartingIt()
    {
        TestInput input = CreateInput(backgroundIndex: 2);

        OpeningRegionRemedyPreparationAccess? access =
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null);

        Assert.NotNull(access);
        Assert.Same(input.RouteAccess, access.RouteAccess);
        Assert.Same(input.RouteAccess.SelectedVariation, access.ApothecaryVariation);
        Assert.Same(input.SceneBinding, access.SceneBinding);
        Assert.Same(input.SceneBinding.TreatmentContext, access.TreatmentContext);
        Assert.Same(input.SceneBinding.Recipe, access.Recipe);
    }

    [Fact]
    public void RejectsOtherBackgroundNonCurrentAndRepeatedInputs()
    {
        TestInput forester = CreateInput(backgroundIndex: 0);
        TestInput scribe = CreateInput(backgroundIndex: 1);
        TestInput apothecary = CreateInput(backgroundIndex: 2);
        TestInput traveller = CreateInput(backgroundIndex: 3);
        OpeningRegionRemedyPreparationAccess issued = Assert.IsType<
            OpeningRegionRemedyPreparationAccess>(Authorize(apothecary));

        Assert.Null(Authorize(forester));
        Assert.Null(Authorize(scribe));
        Assert.Null(Authorize(traveller));
        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            apothecary.RouteAccess,
            forester.RouteAccess,
            apothecary.SceneBinding,
            apothecary.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            apothecary.RouteAccess,
            apothecary.RouteAccess,
            apothecary.SceneBinding,
            apothecary.SceneBinding,
            issued));
        Assert.Same(apothecary.RouteAccess, issued.RouteAccess);
    }

    [Fact]
    public void RejectsStaleRebuiltForeignAndNonDestinationBindings()
    {
        TestInput input = CreateInput(backgroundIndex: 2);
        TestInput rebuilt = CreateInput(
            backgroundIndex: 2,
            input.Options,
            input.Projection);
        TestInput foreign = CreateInput(backgroundIndex: 2);
        OpeningRegionRemedyPreparationSceneBinding rebuiltBinding = Binding(
            input.Projection,
            input.SceneBinding.TreatmentContext);
        OpeningRegionRemedyPreparationSceneBinding foreignContextBinding = Binding(
            input.Projection,
            RepresentativeFirstPatientTreatmentContextSource.Create());

        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            input.RouteAccess,
            rebuilt.RouteAccess,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            input.RouteAccess,
            foreign.RouteAccess,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.SceneBinding,
            rebuiltBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.SceneBinding,
            foreignContextBinding,
            issuedAccess: null));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        TestInput input = CreateInput(backgroundIndex: 2);

        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                null!,
                input.RouteAccess,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                input.RouteAccess,
                null!,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                null!,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionRemedyPreparationAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.SceneBinding,
                null!,
                issuedAccess: null));
    }

    private static OpeningRegionRemedyPreparationAccess? Authorize(TestInput input) =>
        OpeningRegionRemedyPreparationAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null);

    private static TestInput CreateInput(
        int backgroundIndex,
        SeekerCustomizationOptions? options = null,
        OpeningRegionProjection? projection = null)
    {
        options ??= RepresentativeSeekerCustomizationOptionsSource.Create();
        projection ??= OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        Character current = Character();
        SeekerCustomizationSelectionState selection =
            SeekerCustomizationSelectionStateTransition.Select(
                SeekerCustomizationSelectionStateTransition.Initialize(options),
                new CharacterIdentity(current.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                options.AppearanceChoices[0],
                options.BackgroundChoices[backgroundIndex]);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selection, current);
        SeekerBackgroundRouteVariationState routeState =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                selection,
                application);
        SeekerBackgroundRouteAccess routeAccess =
            Assert.IsType<SeekerBackgroundRouteAccess>(
                SeekerBackgroundRouteAccessPolicy.Authorize(
                    routeState,
                    routeState,
                    routeState.SelectedVariation!.DestinationAnchor,
                    issuedAccess: null));
        FirstPatientTreatmentContext context =
            RepresentativeFirstPatientTreatmentContextSource.Create();
        return new TestInput(
            options,
            projection,
            routeAccess,
            Binding(projection, context));
    }

    private static OpeningRegionRemedyPreparationSceneBinding Binding(
        OpeningRegionProjection projection,
        FirstPatientTreatmentContext context) => new(
        projection,
        projection.Anchors[1],
        context,
        new Vector2(920.0f, 620.0f),
        72.0f);

    private static Character Character()
    {
        CharacterId id = new("seeker");
        return new Character(
            new CharacterIdentity(id, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground(string.Empty),
            new CharacterCapabilitySet([]),
            new CharacterLimitationSet([]),
            new CharacterControllerAssignment(id, ControllerType.Human),
            new CharacterConditionSet([]),
            new CharacterInventory([]),
            new CharacterKnowledge([]),
            new CharacterBeliefSet([]),
            new CharacterRelationshipSet([]),
            new CharacterGoalSet([]));
    }

    private sealed record TestInput(
        SeekerCustomizationOptions Options,
        OpeningRegionProjection Projection,
        SeekerBackgroundRouteAccess RouteAccess,
        OpeningRegionRemedyPreparationSceneBinding SceneBinding);
}
