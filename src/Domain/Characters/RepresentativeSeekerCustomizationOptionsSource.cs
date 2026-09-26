namespace Allrpcs.Domain.Characters;

/// <summary>Authors the representative opening-scenario Seeker customization choices.</summary>
public static class RepresentativeSeekerCustomizationOptionsSource
{
    /// <summary>Creates stable asset-neutral appearances and the four opening backgrounds.</summary>
    public static SeekerCustomizationOptions Create() => new(
        [
            new SeekerAppearanceChoice("plain-tunic", "Plain tunic"),
            new SeekerAppearanceChoice("travelling-cloak", "Travelling cloak"),
            new SeekerAppearanceChoice("work-apron", "Work apron"),
        ],
        [
            new SeekerBackgroundChoice(
                "forester",
                "Forester",
                new PublicCharacterBackground(
                    "A forester familiar with woodland paths and seasonal signs.")),
            new SeekerBackgroundChoice(
                "scribe",
                "Scribe",
                new PublicCharacterBackground(
                    "A scribe trained to compare written accounts and incomplete records.")),
            new SeekerBackgroundChoice(
                "apothecarys-assistant",
                "Apothecary's Assistant",
                new PublicCharacterBackground(
                    "An apothecary's assistant familiar with remedies, tools, and preparation.")),
            new SeekerBackgroundChoice(
                "traveller",
                "Traveller",
                new PublicCharacterBackground(
                    "A traveller practiced at reading roads, settlements, and local testimony.")),
        ]);
}
