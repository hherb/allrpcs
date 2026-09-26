using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class SeekerCustomizationOptionsTests
{
    [Fact]
    public void RepresentativeOptionsRetainStableAppearanceAndBackgroundOrder()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();

        Assert.Equal(
            ["plain-tunic", "travelling-cloak", "work-apron"],
            options.AppearanceChoices.Select(choice => choice.Key));
        Assert.Equal(
            ["forester", "scribe", "apothecarys-assistant", "traveller"],
            options.BackgroundChoices.Select(choice => choice.Key));
        Assert.Equal(
            ["Forester", "Scribe", "Apothecary's Assistant", "Traveller"],
            options.BackgroundChoices.Select(choice => choice.DisplayName));
        Assert.All(options.BackgroundChoices, choice =>
            Assert.False(string.IsNullOrWhiteSpace(choice.PublicBackground.Text)));
    }

    [Fact]
    public void ComposesProfileFromExactRetainedChoices()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = options.AppearanceChoices[1];
        SeekerBackgroundChoice background = options.BackgroundChoices[0];

        SeekerCustomizationProfile profile = SeekerCustomizationSelection.Compose(
            options,
            identity,
            pronouns,
            appearance,
            background);

        Assert.Same(identity, profile.Identity);
        Assert.Same(pronouns, profile.Pronouns);
        Assert.Same(appearance, profile.Appearance);
        Assert.Same(background, profile.Background);
    }

    [Fact]
    public void RejectsRebuiltAndForeignChoices()
    {
        SeekerCustomizationOptions options =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = options.AppearanceChoices[0];
        SeekerBackgroundChoice background = options.BackgroundChoices[0];
        SeekerAppearanceChoice rebuiltAppearance = new(
            appearance.Key,
            appearance.DisplayName);
        SeekerBackgroundChoice rebuiltBackground = new(
            background.Key,
            background.DisplayName,
            background.PublicBackground);

        Assert.Throws<ArgumentException>(() => SeekerCustomizationSelection.Compose(
            options,
            identity,
            pronouns,
            rebuiltAppearance,
            background));
        Assert.Throws<ArgumentException>(() => SeekerCustomizationSelection.Compose(
            options,
            identity,
            pronouns,
            appearance,
            rebuiltBackground));
        Assert.Throws<ArgumentException>(() => SeekerCustomizationSelection.Compose(
            RepresentativeSeekerCustomizationOptionsSource.Create(),
            identity,
            pronouns,
            appearance,
            background));
    }

    [Fact]
    public void OptionsRejectEmptyDuplicateAndNullEntries()
    {
        SeekerAppearanceChoice appearance = new("plain-tunic", "Plain tunic");
        SeekerBackgroundChoice background = new(
            "forester",
            "Forester",
            new PublicCharacterBackground("A forester."));

        Assert.Throws<ArgumentException>(() =>
            new SeekerCustomizationOptions([], [background]));
        Assert.Throws<ArgumentException>(() =>
            new SeekerCustomizationOptions([appearance], []));
        Assert.Throws<ArgumentException>(() =>
            new SeekerCustomizationOptions(
                [appearance, new SeekerAppearanceChoice(appearance.Key, "Other")],
                [background]));
        Assert.Throws<ArgumentNullException>(() =>
            new SeekerCustomizationOptions(null!, [background]));
    }
}
