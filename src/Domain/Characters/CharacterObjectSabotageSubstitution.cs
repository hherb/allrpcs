using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact substitute-sabotage occurrence to one authored replacement.</summary>
public sealed class CharacterObjectSabotageSubstitution
{
    /// <summary>Creates one immutable substitution binding without applying an effect.</summary>
    public CharacterObjectSabotageSubstitution(
        CharacterObjectSabotageOccurrence occurrence,
        WorldObject replacement)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(replacement);

        if (occurrence.Proposal.ActionKind != CharacterSabotageActionKind.Substitute)
        {
            throw new ArgumentException(
                "The sabotage occurrence must use the substitute action kind.",
                nameof(occurrence));
        }

        if (ReferenceEquals(occurrence.Target, replacement)
            || occurrence.Target.Identity.Id == replacement.Identity.Id)
        {
            throw new ArgumentException(
                "The replacement must have a distinct world-object identifier.",
                nameof(replacement));
        }

        Occurrence = occurrence;
        Replacement = replacement;
    }

    /// <summary>Gets the exact substitute-sabotage occurrence.</summary>
    public CharacterObjectSabotageOccurrence Occurrence { get; }

    /// <summary>Gets the exact authored replacement.</summary>
    public WorldObject Replacement { get; }
}
