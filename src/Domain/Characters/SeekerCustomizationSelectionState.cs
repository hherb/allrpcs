namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact representative options and one optional selected Seeker profile.</summary>
public sealed class SeekerCustomizationSelectionState
{
    internal SeekerCustomizationSelectionState(
        SeekerCustomizationOptions options,
        SeekerCustomizationProfile? selectedProfile,
        SeekerCustomizationSelectionState? previousState)
    {
        Options = options;
        SelectedProfile = selectedProfile;
        PreviousState = previousState;
    }

    /// <summary>Gets the exact authored options available to the player.</summary>
    public SeekerCustomizationOptions Options { get; }

    /// <summary>Gets the exact selected profile, or null before selection.</summary>
    public SeekerCustomizationProfile? SelectedProfile { get; }

    /// <summary>Gets the exact empty predecessor, or null for initial state.</summary>
    public SeekerCustomizationSelectionState? PreviousState { get; }
}
