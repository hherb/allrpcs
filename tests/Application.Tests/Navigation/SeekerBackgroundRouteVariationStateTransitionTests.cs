using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class SeekerBackgroundRouteVariationStateTransitionTests
{
    [Fact]
    public void InitializesEmptyAndSelectsOneExactAppliedVariation()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        ApplicationInput applied = AppliedInput(options, options.BackgroundChoices[1]);

        SeekerBackgroundRouteVariationState selected =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                applied.SelectedState,
                applied.Application);

        Assert.Same(projection, initial.OpeningRegion);
        Assert.Null(initial.SelectedVariation);
        Assert.Null(initial.PreviousState);
        Assert.Same(initial.OpeningRegion, selected.OpeningRegion);
        Assert.Same(initial.Variations, selected.Variations);
        Assert.Same(initial.Variations[1], selected.SelectedVariation);
        Assert.Same(initial, selected.PreviousState);
        Assert.Same(applied.SelectedState, applied.Application.SelectedState);
    }

    [Fact]
    public void RejectsEmptyUnappliedRebuiltForeignAndRepeatedInputs()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        SeekerCustomizationSelectionState empty =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        ApplicationInput applied = AppliedInput(options, options.BackgroundChoices[2]);
        ApplicationInput rebuilt = AppliedInput(options, options.BackgroundChoices[2]);
        SeekerCustomizationOptions foreignOptions =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        ApplicationInput foreign = AppliedInput(
            foreignOptions,
            foreignOptions.BackgroundChoices[2]);
        SeekerBackgroundRouteVariationState selected =
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                applied.SelectedState,
                applied.Application);

        Assert.Throws<InvalidOperationException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(initial, empty, null));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                applied.SelectedState,
                null));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                rebuilt.SelectedState,
                applied.Application));
        Assert.Throws<ArgumentException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                foreign.SelectedState,
                foreign.Application));
        Assert.Throws<InvalidOperationException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                selected,
                applied.SelectedState,
                applied.Application));
        Assert.Null(initial.SelectedVariation);
        Assert.Same(initial, selected.PreviousState);
    }

    [Fact]
    public void RejectsNullInitializationAndSelectionInputs()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        SeekerBackgroundRouteVariationState initial =
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, projection);
        ApplicationInput applied = AppliedInput(options, options.BackgroundChoices[3]);

        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Initialize(null!, projection));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Initialize(options, null!));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                null!,
                applied.SelectedState,
                applied.Application));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationStateTransition.Select(
                initial,
                null!,
                applied.Application));
    }

    private static ApplicationInput AppliedInput(
        SeekerCustomizationOptions options,
        SeekerBackgroundChoice background)
    {
        Character current = Character();
        SeekerCustomizationSelectionState initial =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        SeekerCustomizationSelectionState selected =
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                new CharacterIdentity(current.Identity.Id, "Mara"),
                new SeekerPronouns("they", "them", "their", "theirs", "themself"),
                options.AppearanceChoices[0],
                background);
        return new ApplicationInput(
            selected,
            SeekerCustomizedCharacterProjection.Project(selected, current));
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

    private sealed record ApplicationInput(
        SeekerCustomizationSelectionState SelectedState,
        SeekerCustomizedCharacterApplication Application);
}
