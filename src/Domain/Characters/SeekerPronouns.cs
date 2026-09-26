namespace Allrpcs.Domain.Characters;

/// <summary>Retains the player's authored third-person pronoun forms for the Seeker.</summary>
public sealed record SeekerPronouns
{
    /// <summary>Gets the maximum supported length of each authored pronoun form.</summary>
    public const int MaximumFormLength = 40;

    /// <summary>Creates exact subject, object, and possessive-determiner forms.</summary>
    public SeekerPronouns(
        string subject,
        string objective,
        string possessiveDeterminer,
        string possessivePronoun,
        string reflexive)
    {
        Subject = AuthoredCharacterText.RequireNonBlank(
            subject,
            MaximumFormLength,
            nameof(subject));
        Objective = AuthoredCharacterText.RequireNonBlank(
            objective,
            MaximumFormLength,
            nameof(objective));
        PossessiveDeterminer = AuthoredCharacterText.RequireNonBlank(
            possessiveDeterminer,
            MaximumFormLength,
            nameof(possessiveDeterminer));
        PossessivePronoun = AuthoredCharacterText.RequireNonBlank(
            possessivePronoun,
            MaximumFormLength,
            nameof(possessivePronoun));
        Reflexive = AuthoredCharacterText.RequireNonBlank(
            reflexive,
            MaximumFormLength,
            nameof(reflexive));
    }

    /// <summary>Gets the exact subject form, such as they.</summary>
    public string Subject { get; }

    /// <summary>Gets the exact object form, such as them.</summary>
    public string Objective { get; }

    /// <summary>Gets the exact possessive determiner, such as their.</summary>
    public string PossessiveDeterminer { get; }

    /// <summary>Gets the exact possessive pronoun, such as theirs.</summary>
    public string PossessivePronoun { get; }

    /// <summary>Gets the exact reflexive form, such as themself.</summary>
    public string Reflexive { get; }
}
