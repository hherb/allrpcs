namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact provenance for one pure customized-Seeker projection.</summary>
public sealed class SeekerCustomizedCharacterApplication
{
    internal SeekerCustomizedCharacterApplication(
        SeekerCustomizationSelectionState selectedState,
        Character currentCharacter,
        Character customizedCharacter)
    {
        SelectedState = selectedState;
        CurrentCharacter = currentCharacter;
        CustomizedCharacter = customizedCharacter;
    }

    /// <summary>Gets the exact selected customization state used by the projection.</summary>
    public SeekerCustomizationSelectionState SelectedState { get; }

    /// <summary>Gets the exact current temporary Seeker used by the projection.</summary>
    public Character CurrentCharacter { get; }

    /// <summary>Gets the fresh customized character produced by the projection.</summary>
    public Character CustomizedCharacter { get; }
}
