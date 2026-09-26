using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class SeekerBackgroundRouteVariationTests
{
    [Fact]
    public void AuthorsOneExactVariationPerRepresentativeBackgroundInStableOrder()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();

        IReadOnlyList<SeekerBackgroundRouteVariation> variations =
            RepresentativeSeekerBackgroundRouteVariationSource.Create(options, projection);

        Assert.Collection(
            variations,
            variation => AssertVariation(
                variation,
                options.BackgroundChoices[0],
                projection.Anchors[10]),
            variation => AssertVariation(
                variation,
                options.BackgroundChoices[1],
                projection.Anchors[7]),
            variation => AssertVariation(
                variation,
                options.BackgroundChoices[2],
                projection.Anchors[1]),
            variation => AssertVariation(
                variation,
                options.BackgroundChoices[3],
                projection.Anchors[8]));
    }

    [Fact]
    public void ProjectsOnlyTheExactAppliedBackgroundAndOpeningProjection()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        IReadOnlyList<SeekerBackgroundRouteVariation> variations =
            RepresentativeSeekerBackgroundRouteVariationSource.Create(options, projection);

        for (int index = 0; index < options.BackgroundChoices.Count; index++)
        {
            ApplicationInput input = AppliedInput(options, options.BackgroundChoices[index]);

            SeekerBackgroundRouteVariation? selected =
                SeekerBackgroundRouteVariationProjection.Project(
                    input.SelectedState,
                    input.Application,
                    projection,
                    variations);

            Assert.Same(variations[index], selected);
            Assert.Same(input.SelectedState, input.Application.SelectedState);
            Assert.Same(input.CurrentCharacter, input.Application.CurrentCharacter);
        }
    }

    [Fact]
    public void RejectsEmptyUnappliedRebuiltAndForeignCustomizationProvenance()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        IReadOnlyList<SeekerBackgroundRouteVariation> variations =
            RepresentativeSeekerBackgroundRouteVariationSource.Create(options, projection);
        SeekerCustomizationSelectionState empty =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        ApplicationInput applied = AppliedInput(options, options.BackgroundChoices[0]);
        ApplicationInput rebuilt = AppliedInput(options, options.BackgroundChoices[0]);
        SeekerCustomizationOptions foreignOptions =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        ApplicationInput foreign = AppliedInput(
            foreignOptions,
            foreignOptions.BackgroundChoices[0]);

        Assert.Null(SeekerBackgroundRouteVariationProjection.Project(
            empty,
            null,
            projection,
            variations));
        Assert.Null(SeekerBackgroundRouteVariationProjection.Project(
            applied.SelectedState,
            null,
            projection,
            variations));
        Assert.Null(SeekerBackgroundRouteVariationProjection.Project(
            rebuilt.SelectedState,
            applied.Application,
            projection,
            variations));
        Assert.Throws<ArgumentException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                foreign.SelectedState,
                foreign.Application,
                projection,
                variations));
    }

    [Fact]
    public void RejectsForeignProjectionMissingAnchorDuplicateCatalogueAndNullInputs()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionProjection foreignProjection =
            OpeningRegionOldShrineSceneBindingTests.Projection();
        IReadOnlyList<SeekerBackgroundRouteVariation> variations =
            RepresentativeSeekerBackgroundRouteVariationSource.Create(options, projection);
        ApplicationInput applied = AppliedInput(options, options.BackgroundChoices[0]);
        SeekerBackgroundRouteVariation missingAnchor = new(
            options.BackgroundChoices[0],
            foreignProjection.Anchors[10]);

        Assert.Throws<ArgumentException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                applied.SelectedState,
                applied.Application,
                foreignProjection,
                variations));
        Assert.Throws<ArgumentException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                applied.SelectedState,
                applied.Application,
                projection,
                variations.Skip(1).Prepend(missingAnchor)));
        Assert.Throws<ArgumentException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                applied.SelectedState,
                applied.Application,
                projection,
                variations.Append(variations[0])));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                null!,
                applied.Application,
                projection,
                variations));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                applied.SelectedState,
                applied.Application,
                null!,
                variations));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerBackgroundRouteVariationProjection.Project(
                applied.SelectedState,
                applied.Application,
                projection,
                null!));
    }

    private static void AssertVariation(
        SeekerBackgroundRouteVariation variation,
        SeekerBackgroundChoice background,
        OpeningRegionAnchor anchor)
    {
        Assert.Same(background, variation.Background);
        Assert.Same(anchor, variation.DestinationAnchor);
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
        SeekerCustomizedCharacterApplication application =
            SeekerCustomizedCharacterProjection.Project(selected, current);
        return new ApplicationInput(selected, application, current);
    }

    private static Character Character()
    {
        CharacterId id = new("seeker");
        return new Character(
            new CharacterIdentity(id, "SEEKER"),
            new PublicCharacterBackground(string.Empty),
            new PrivateCharacterBackground("Private history."),
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
        SeekerCustomizedCharacterApplication Application,
        Character CurrentCharacter);
}
