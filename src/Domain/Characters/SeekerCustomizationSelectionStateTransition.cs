namespace Allrpcs.Domain.Characters;

/// <summary>Initializes and advances one exact Seeker customization selection state.</summary>
public static class SeekerCustomizationSelectionStateTransition
{
    /// <summary>Creates empty state retaining the exact supplied options.</summary>
    public static SeekerCustomizationSelectionState Initialize(
        SeekerCustomizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new SeekerCustomizationSelectionState(
            options,
            selectedProfile: null,
            previousState: null);
    }

    /// <summary>Returns one selected state composed only from its exact retained options.</summary>
    public static SeekerCustomizationSelectionState Select(
        SeekerCustomizationSelectionState state,
        CharacterIdentity identity,
        SeekerPronouns pronouns,
        SeekerAppearanceChoice appearance,
        SeekerBackgroundChoice background)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(pronouns);
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(background);
        if (state.SelectedProfile is not null)
        {
            throw new InvalidOperationException(
                "Seeker customization can be selected only once.");
        }

        SeekerCustomizationProfile profile = SeekerCustomizationSelection.Compose(
            state.Options,
            identity,
            pronouns,
            appearance,
            background);
        return new SeekerCustomizationSelectionState(state.Options, profile, state);
    }
}
