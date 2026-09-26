using System.Reflection;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionSouthernRoadWagonAccessPolicyTests
{
    [Fact]
    public void AuthorizesOneExactTravellerAccessWithoutChangingTheWagon()
    {
        TestInput input = CreateInput(backgroundIndex: 3);

        OpeningRegionSouthernRoadWagonAccess? access =
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null);

        Assert.NotNull(access);
        Assert.Same(input.RouteAccess, access.RouteAccess);
        Assert.Same(input.RouteAccess.SelectedVariation, access.TravellerVariation);
        Assert.Same(input.SceneBinding, access.SceneBinding);
        Assert.Same(
            input.Projection.Anchors[8],
            access.SceneBinding.SouthernRoadAnchor);
        Assert.Same(
            input.Projection.Anchors[9],
            access.SceneBinding.AbandonedWagonAnchor);
    }

    [Fact]
    public void RejectsOtherBackgroundNonCurrentAndRepeatedInputs()
    {
        TestInput forester = CreateInput(backgroundIndex: 0);
        TestInput scribe = CreateInput(backgroundIndex: 1);
        TestInput apothecary = CreateInput(backgroundIndex: 2);
        TestInput traveller = CreateInput(backgroundIndex: 3);
        OpeningRegionSouthernRoadWagonAccess issued = Assert.IsType<
            OpeningRegionSouthernRoadWagonAccess>(Authorize(traveller));

        Assert.Null(Authorize(forester));
        Assert.Null(Authorize(scribe));
        Assert.Null(Authorize(apothecary));
        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            traveller.RouteAccess,
            forester.RouteAccess,
            traveller.SceneBinding,
            traveller.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            traveller.RouteAccess,
            traveller.RouteAccess,
            traveller.SceneBinding,
            traveller.SceneBinding,
            issued));
        Assert.Same(traveller.RouteAccess, issued.RouteAccess);
    }

    [Fact]
    public void RejectsStaleRebuiltForeignAndNonDestinationBindings()
    {
        TestInput input = CreateInput(backgroundIndex: 3);
        TestInput rebuilt = CreateInput(
            backgroundIndex: 3,
            input.Options,
            input.Projection);
        TestInput foreign = CreateInput(backgroundIndex: 3);
        OpeningRegionSouthernRoadWagonSceneBinding rebuiltBinding = Binding(
            input.Projection);

        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            input.RouteAccess,
            rebuilt.RouteAccess,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            input.RouteAccess,
            foreign.RouteAccess,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null));
        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            input.RouteAccess,
            input.RouteAccess,
            input.SceneBinding,
            rebuiltBinding,
            issuedAccess: null));
    }

    [Fact]
    public void RejectsMalformedAndNullInputs()
    {
        TestInput input = CreateInput(backgroundIndex: 3);
        ConstructorInfo constructor = typeof(SeekerBackgroundRouteAccess)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single();
        SeekerBackgroundRouteAccess malformed =
            Assert.IsType<SeekerBackgroundRouteAccess>(constructor.Invoke(
            [
                input.RouteAccess.RouteState,
                input.RouteAccess.SelectedVariation,
                input.RouteAccess.RouteState.OpeningRegion.Anchors[0],
            ]));

        Assert.Null(OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
            malformed,
            malformed,
            input.SceneBinding,
            input.SceneBinding,
            issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                null!,
                input.RouteAccess,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                input.RouteAccess,
                null!,
                input.SceneBinding,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                null!,
                input.SceneBinding,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
                input.RouteAccess,
                input.RouteAccess,
                input.SceneBinding,
                null!,
                issuedAccess: null));
    }

    private static OpeningRegionSouthernRoadWagonAccess? Authorize(TestInput input) =>
        OpeningRegionSouthernRoadWagonAccessPolicy.Authorize(
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
        return new TestInput(options, projection, routeAccess, Binding(projection));
    }

    private static OpeningRegionSouthernRoadWagonSceneBinding Binding(
        OpeningRegionProjection projection) => new(
        OpeningRegionSouthernRoadWagonSceneBindingTests.ShrineLayout(projection),
        projection.Anchors[8],
        projection.Anchors[9],
        new NavigationWorldBounds(648.0f, 744.0f, 776.0f, 876.0f));

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
        OpeningRegionSouthernRoadWagonSceneBinding SceneBinding);
}
