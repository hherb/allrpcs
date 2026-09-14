using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact move-sabotage occurrence to one authored destination.</summary>
public sealed class CharacterObjectSabotageMoveDestination
{
    /// <summary>Creates one immutable destination binding without applying an effect.</summary>
    public CharacterObjectSabotageMoveDestination(
        CharacterObjectSabotageOccurrence occurrence,
        Location destination)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        ArgumentNullException.ThrowIfNull(destination);

        if (occurrence.Proposal.ActionKind != CharacterSabotageActionKind.Move)
        {
            throw new ArgumentException(
                "The sabotage occurrence must use the move action kind.",
                nameof(occurrence));
        }

        Occurrence = occurrence;
        Destination = destination;
    }

    /// <summary>Gets the exact move-sabotage occurrence.</summary>
    public CharacterObjectSabotageOccurrence Occurrence { get; }

    /// <summary>Gets the exact authored destination.</summary>
    public Location Destination { get; }
}
