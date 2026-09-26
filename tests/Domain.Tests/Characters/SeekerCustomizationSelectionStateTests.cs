using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class SeekerCustomizationSelectionStateTests
{
    [Fact]
    public void InitializesEmptyStateWithExactRepresentativeOptions()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();

        SeekerCustomizationSelectionState state =
            SeekerCustomizationSelectionStateTransition.Initialize(options);

        Assert.Same(options, state.Options);
        Assert.Null(state.SelectedProfile);
        Assert.Null(state.PreviousState);
    }

    [Fact]
    public void SelectsOnceThroughExactRetainedChoices()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState initial =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = options.AppearanceChoices[1];
        SeekerBackgroundChoice background = options.BackgroundChoices[2];

        SeekerCustomizationSelectionState selected =
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                pronouns,
                appearance,
                background);

        Assert.NotSame(initial, selected);
        Assert.Same(options, selected.Options);
        Assert.Same(initial, selected.PreviousState);
        Assert.Null(initial.SelectedProfile);
        Assert.Same(identity, selected.SelectedProfile!.Identity);
        Assert.Same(pronouns, selected.SelectedProfile.Pronouns);
        Assert.Same(appearance, selected.SelectedProfile.Appearance);
        Assert.Same(background, selected.SelectedProfile.Background);
    }

    [Fact]
    public void RejectsRepeatedRebuiltForeignWrongCharacterAndIncompleteSelections()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState initial =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = options.AppearanceChoices[0];
        SeekerBackgroundChoice background = options.BackgroundChoices[0];
        SeekerCustomizationSelectionState selected =
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                pronouns,
                appearance,
                background);
        SeekerAppearanceChoice rebuiltAppearance = new(
            appearance.Key,
            appearance.DisplayName);
        SeekerCustomizationOptions foreignOptions =
            RepresentativeSeekerCustomizationOptionsSource.Create();

        Assert.Throws<InvalidOperationException>(() =>
            SeekerCustomizationSelectionStateTransition.Select(
                selected,
                identity,
                pronouns,
                appearance,
                background));
        Assert.Throws<ArgumentException>(() =>
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                pronouns,
                rebuiltAppearance,
                background));
        Assert.Throws<ArgumentException>(() =>
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                pronouns,
                foreignOptions.AppearanceChoices[0],
                background));
        Assert.Throws<ArgumentException>(() =>
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                new CharacterIdentity(new CharacterId("healer"), "Mara"),
                pronouns,
                appearance,
                background));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                null!,
                appearance,
                background));
        Assert.Null(initial.SelectedProfile);
        Assert.Same(options, initial.Options);
    }

    [Fact]
    public void RejectsMissingInitializationInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SeekerCustomizationSelectionStateTransition.Initialize(null!));
    }
}
