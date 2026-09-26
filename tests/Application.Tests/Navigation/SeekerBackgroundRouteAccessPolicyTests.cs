using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class SeekerBackgroundRouteAccessPolicyTests
{
    [Fact]
    public void AuthorizesOneExactSelectedDestinationWithoutChangingRouteState()
    {
        RouteInput input = SelectedRoute(2);

        SeekerBackgroundRouteAccess? access = SeekerBackgroundRouteAccessPolicy.Authorize(
            input.SelectedState,
            input.SelectedState,
            input.SelectedState.SelectedVariation!.DestinationAnchor,
            issuedAccess: null);

        Assert.NotNull(access);
        Assert.Same(input.SelectedState, access.RouteState);
        Assert.Same(input.SelectedState.SelectedVariation, access.SelectedVariation);
        Assert.Same(
            input.SelectedState.SelectedVariation.DestinationAnchor,
            access.DestinationAnchor);
        Assert.Same(input.InitialState, input.SelectedState.PreviousState);
        Assert.Null(input.InitialState.SelectedVariation);
    }

    [Fact]
    public void RejectsEmptyStaleRebuiltForeignAndNonDestinationInputs()
    {
        RouteInput input = SelectedRoute(0);
        RouteInput rebuilt = SelectedRoute(0, input.Options, input.Projection);
        RouteInput foreign = SelectedRoute(0);

        Assert.Null(SeekerBackgroundRouteAccessPolicy.Authorize(
            input.InitialState,
            input.InitialState,
            input.SelectedState.SelectedVariation!.DestinationAnchor,
            issuedAccess: null));
        Assert.Null(SeekerBackgroundRouteAccessPolicy.Authorize(
            input.SelectedState,
            input.InitialState,
            input.SelectedState.SelectedVariation.DestinationAnchor,
            issuedAccess: null));
        Assert.Null(SeekerBackgroundRouteAccessPolicy.Authorize(
            input.SelectedState,
            rebuilt.SelectedState,
            input.SelectedState.SelectedVariation.DestinationAnchor,
            issuedAccess: null));
        Assert.Null(SeekerBackgroundRouteAccessPolicy.Authorize(
            input.SelectedState,
            foreign.SelectedState,
            input.SelectedState.SelectedVariation.DestinationAnchor,
            issuedAccess: null));
        Assert.Null(SeekerBackgroundRouteAccessPolicy.Authorize(
            input.SelectedState,
            input.SelectedState,
            input.SelectedState.OpeningRegion.Anchors[0],
            issuedAccess: null));
        Assert.Same(input.InitialState, input.SelectedState.PreviousState);
        Assert.Null(input.InitialState.SelectedVariation);
    }

    [Fact]
    public void RejectsRepeatedAuthorizationWithoutReplacingTheExactAccess()
    {
        RouteInput input = SelectedRoute(3);
        OpeningRegionAnchor destination =
            input.SelectedState.SelectedVariation!.DestinationAnchor;
        SeekerBackgroundRouteAccess access = Assert.IsType<SeekerBackgroundRouteAccess>(
            SeekerBackgroundRouteAccessPolicy.Authorize(
                input.SelectedState,
                input.SelectedState,
                destination,
                issuedAccess: null));

        SeekerBackgroundRouteAccess? repeated =
            SeekerBackgroundRouteAccessPolicy.Authorize(
                input.SelectedState,
                input.SelectedState,
                destination,
                access);

        Assert.Null(repeated);
        Assert.Same(input.SelectedState, access.RouteState);
        Assert.Same(destination, access.DestinationAnchor);
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        RouteInput input = SelectedRoute(1);
        OpeningRegionAnchor destination =
            input.SelectedState.SelectedVariation!.DestinationAnchor;

        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteAccessPolicy.Authorize(
                null!,
                input.SelectedState,
                destination,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteAccessPolicy.Authorize(
                input.SelectedState,
                null!,
                destination,
                issuedAccess: null));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteAccessPolicy.Authorize(
                input.SelectedState,
                input.SelectedState,
                null!,
                issuedAccess: null));
    }

    private static RouteInput SelectedRoute(
        int backgroundIndex,
        SeekerCustomizationOptions? options = null,
        OpeningRegionProjection? projection = null)
    {
        options ??= RepresentativeSeekerCustomizationOptionsSource.Create();
        projection ??= OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        Character current = Character();
        SeekerCustomizationSelectionState customization = SelectedCustomization(
            options,
            options.BackgroundChoices[backgroundIndex],
            current);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(customization, current);
        SeekerBackgroundRouteVariationState selected =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                customization,
                application);
        return new RouteInput(options, projection, initial, selected);
    }

    private static SeekerCustomizationSelectionState SelectedCustomization(
        SeekerCustomizationOptions options,
        SeekerBackgroundChoice background,
        Character current)
    {
        return SeekerCustomizationSelectionStateTransition.Select(
            SeekerCustomizationSelectionStateTransition.Initialize(options),
            new CharacterIdentity(current.Identity.Id, "Mara"),
            new SeekerPronouns("they", "them", "their", "theirs", "themself"),
            options.AppearanceChoices[0],
            background);
    }

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

    private sealed record RouteInput(
        SeekerCustomizationOptions Options,
        OpeningRegionProjection Projection,
        SeekerBackgroundRouteVariationState InitialState,
        SeekerBackgroundRouteVariationState SelectedState);
}
