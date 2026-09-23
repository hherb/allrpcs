using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Checks whether one exact completed sabotage turn may reach ordinary scheduling.</summary>
public static class CharacterSabotageTurnSchedulingEligibilityPolicy
{
    /// <summary>Returns true only for the exact current completion with coherent provenance.</summary>
    public static bool CanSchedule(
        CharacterSabotageTurnCompletion? currentCompletion,
        CharacterSabotageTurnCompletion? expectedCompletion)
    {
        if (currentCompletion is null
            || !ReferenceEquals(currentCompletion, expectedCompletion)
            || !ReferenceEquals(
                currentCompletion.Actor,
                currentCompletion.AuthorRequest.Actor)
            || currentCompletion.AuthorRequest.ActionKinds.Count != 1
            || !ReferenceEquals(
                currentCompletion.AuthorRequest.ActionKinds[0],
                currentCompletion.ActionKind))
        {
            return false;
        }

        foreach (CharacterSabotageActionKind canonicalKind in CharacterSabotageActionKind.All)
        {
            if (ReferenceEquals(canonicalKind, currentCompletion.ActionKind))
            {
                return true;
            }
        }

        return false;
    }
}
