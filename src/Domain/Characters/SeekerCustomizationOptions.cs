namespace Allrpcs.Domain.Characters;

/// <summary>Retains ordered, duplicate-free Seeker appearance and background choices.</summary>
public sealed class SeekerCustomizationOptions
{
    /// <summary>Creates a non-empty set of authored choices in stable display order.</summary>
    public SeekerCustomizationOptions(
        IEnumerable<SeekerAppearanceChoice> appearanceChoices,
        IEnumerable<SeekerBackgroundChoice> backgroundChoices)
    {
        AppearanceChoices = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            appearanceChoices,
            choice => choice.Key,
            nameof(appearanceChoices));
        BackgroundChoices = AuthoredCharacterEntrySet.RequireUniqueOrdered(
            backgroundChoices,
            choice => choice.Key,
            nameof(backgroundChoices));
        if (AppearanceChoices.Count == 0)
        {
            throw new ArgumentException(
                "At least one Seeker appearance choice is required.",
                nameof(appearanceChoices));
        }

        if (BackgroundChoices.Count == 0)
        {
            throw new ArgumentException(
                "At least one Seeker background choice is required.",
                nameof(backgroundChoices));
        }
    }

    /// <summary>Gets asset-neutral appearance choices in stable authored order.</summary>
    public IReadOnlyList<SeekerAppearanceChoice> AppearanceChoices { get; }

    /// <summary>Gets background choices in stable authored order.</summary>
    public IReadOnlyList<SeekerBackgroundChoice> BackgroundChoices { get; }
}
