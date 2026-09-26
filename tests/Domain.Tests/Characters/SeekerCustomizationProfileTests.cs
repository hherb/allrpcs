using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class SeekerCustomizationProfileTests
{
    [Fact]
    public void RetainsExactIdentityPronounsAppearanceAndBackground()
    {
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("she", "her", "her", "hers", "herself");
        SeekerAppearanceChoice appearance = new(
            "travelling-cloak",
            "Travelling cloak");
        PublicCharacterBackground publicBackground = new(
            "A forester familiar with woodland paths and seasonal signs.");
        SeekerBackgroundChoice background = new(
            "forester",
            "Forester",
            publicBackground);

        SeekerCustomizationProfile profile = new(
            identity,
            pronouns,
            appearance,
            background);

        Assert.Same(identity, profile.Identity);
        Assert.Same(pronouns, profile.Pronouns);
        Assert.Same(appearance, profile.Appearance);
        Assert.Same(background, profile.Background);
        Assert.Same(publicBackground, profile.Background.PublicBackground);
    }

    [Fact]
    public void PronounsPreserveCustomFormsAndEnforceTheirBounds()
    {
        SeekerPronouns pronouns = new("  ze  ", "hir", "  hir  ", "hirs", "hirself");

        Assert.Equal("  ze  ", pronouns.Subject);
        Assert.Equal("hir", pronouns.Objective);
        Assert.Equal("  hir  ", pronouns.PossessiveDeterminer);
        Assert.Equal("hirs", pronouns.PossessivePronoun);
        Assert.Equal("hirself", pronouns.Reflexive);
        Assert.Throws<ArgumentException>(() =>
            new SeekerPronouns(" ", "them", "their", "theirs", "themself"));
        Assert.Throws<ArgumentException>(() => new SeekerPronouns(
            new string('p', SeekerPronouns.MaximumFormLength + 1),
            "them",
            "their",
            "theirs",
            "themself"));
        Assert.Throws<ArgumentNullException>(() =>
            new SeekerPronouns(null!, "them", "their", "theirs", "themself"));
    }

    [Fact]
    public void AppearanceAndBackgroundChoicesRequireCanonicalKeysAndBoundedLabels()
    {
        Assert.Throws<ArgumentException>(() =>
            new SeekerAppearanceChoice("Not Canonical", "Appearance"));
        Assert.Throws<ArgumentException>(() =>
            new SeekerBackgroundChoice(
                "not canonical",
                "Background",
                new PublicCharacterBackground(string.Empty)));
        Assert.Throws<ArgumentException>(() =>
            new SeekerAppearanceChoice("appearance", string.Empty));
        Assert.Throws<ArgumentException>(() =>
            new SeekerBackgroundChoice(
                "background",
                new string('b', SeekerBackgroundChoice.MaximumDisplayNameLength + 1),
                new PublicCharacterBackground(string.Empty)));
    }

    [Fact]
    public void ProfileRejectsMissingComponents()
    {
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = new("plain-tunic", "Plain tunic");
        SeekerBackgroundChoice background = new(
            "scribe",
            "Scribe",
            new PublicCharacterBackground("A scribe trained to compare written accounts."));

        Assert.Throws<ArgumentNullException>(() =>
            new SeekerCustomizationProfile(null!, pronouns, appearance, background));
        Assert.Throws<ArgumentNullException>(() =>
            new SeekerCustomizationProfile(identity, null!, appearance, background));
        Assert.Throws<ArgumentNullException>(() =>
            new SeekerCustomizationProfile(identity, pronouns, null!, background));
        Assert.Throws<ArgumentNullException>(() =>
            new SeekerCustomizationProfile(identity, pronouns, appearance, null!));
        Assert.Throws<ArgumentException>(() =>
            new SeekerCustomizationProfile(
                new CharacterIdentity(new CharacterId("healer"), "Mara"),
                pronouns,
                appearance,
                background));
    }
}
