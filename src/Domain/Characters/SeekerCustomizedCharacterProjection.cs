namespace Allrpcs.Domain.Characters;

/// <summary>Purely projects a selected Seeker profile onto one exact current character.</summary>
public static class SeekerCustomizedCharacterProjection
{
    /// <summary>Returns a fresh customized character and its exact application provenance.</summary>
    public static SeekerCustomizedCharacterApplication Project(
        SeekerCustomizationSelectionState selectedState,
        Character currentCharacter)
    {
        ArgumentNullException.ThrowIfNull(selectedState);
        ArgumentNullException.ThrowIfNull(currentCharacter);

        SeekerCustomizationProfile? profile = selectedState.SelectedProfile;
        if (profile is null
            || !ReferenceEquals(profile.Identity.Id, currentCharacter.Identity.Id)
            || ReferenceEquals(profile.Identity, currentCharacter.Identity))
        {
            throw new InvalidOperationException(
                "Only an unapplied selected profile for the exact current Seeker may be projected.");
        }

        Character customizedCharacter = new(
            profile.Identity,
            profile.Background.PublicBackground,
            currentCharacter.PrivateBackground,
            currentCharacter.Capabilities,
            currentCharacter.Limitations,
            currentCharacter.ControllerAssignment,
            currentCharacter.Conditions,
            currentCharacter.Inventory,
            currentCharacter.Knowledge,
            currentCharacter.Beliefs,
            currentCharacter.Relationships,
            currentCharacter.Goals);

        return new SeekerCustomizedCharacterApplication(
            selectedState,
            currentCharacter,
            customizedCharacter);
    }
}
