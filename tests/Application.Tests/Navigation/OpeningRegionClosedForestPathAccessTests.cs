using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionClosedForestPathAccessTests
{
    [Fact]
    public void BindsExactForesterAccessToTheExistingClosedPath()
    {
        TestInput input = CreateInput(backgroundIndex: 0);

        OpeningRegionClosedForestPathAccess access = new(
            input.SceneBinding,
            input.RouteAccess);

        Assert.Same(input.SceneBinding, access.SceneBinding);
        Assert.Same(input.RouteAccess, access.RouteAccess);
        Assert.Same(
            input.SceneBinding.ClosedForestPathAnchor,
            access.RouteAccess.DestinationAnchor);
    }

    [Fact]
    public void RejectsOtherBackgroundAndForeignSceneProvenance()
    {
        TestInput forester = CreateInput(backgroundIndex: 0);
        TestInput apothecary = CreateInput(backgroundIndex: 2);
        TestInput foreign = CreateInput(backgroundIndex: 0);

        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionClosedForestPathAccess(
                forester.SceneBinding,
                apothecary.RouteAccess));
        Assert.Throws<ArgumentException>(() =>
            new OpeningRegionClosedForestPathAccess(
                foreign.SceneBinding,
                forester.RouteAccess));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        TestInput input = CreateInput(backgroundIndex: 0);

        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionClosedForestPathAccess(null!, input.RouteAccess));
        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionClosedForestPathAccess(input.SceneBinding, null!));
    }

    private static TestInput CreateInput(int backgroundIndex)
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionClosedForestPathSceneBinding binding = SceneBinding(projection);
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        Character current = Character();
        SeekerCustomizationSelectionState selected =
            SeekerCustomizationSelectionStateTransition.Select(
                SeekerCustomizationSelectionStateTransition.Initialize(options),
                new CharacterIdentity(current.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                options.AppearanceChoices[0],
                options.BackgroundChoices[backgroundIndex]);
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selected, current);
        SeekerBackgroundRouteVariationState routeState =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                selected,
                application);
        SeekerBackgroundRouteAccess routeAccess =
            Assert.IsType<SeekerBackgroundRouteAccess>(
                SeekerBackgroundRouteAccessPolicy.Authorize(
                    routeState,
                    routeState,
                    routeState.SelectedVariation!.DestinationAnchor,
                    issuedAccess: null));
        return new TestInput(binding, routeAccess);
    }

    private static OpeningRegionClosedForestPathSceneBinding SceneBinding(
        OpeningRegionProjection projection)
    {
        return new OpeningRegionClosedForestPathSceneBinding(
            OpeningRegionClosedForestPathSceneBindingTests.ClearingLayout(projection),
            projection.Anchors[10]);
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

    private sealed record TestInput(
        OpeningRegionClosedForestPathSceneBinding SceneBinding,
        SeekerBackgroundRouteAccess RouteAccess);
}
