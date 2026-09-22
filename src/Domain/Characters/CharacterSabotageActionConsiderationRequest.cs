namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact actor and caller-supplied sabotage kinds for consideration.</summary>
public sealed record CharacterSabotageActionConsiderationRequest
{
    /// <summary>Creates one provider-neutral request without selecting or applying an action.</summary>
    public CharacterSabotageActionConsiderationRequest(
        Character actor,
        IEnumerable<CharacterSabotageActionKind> actionKinds)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(actionKinds);

        List<CharacterSabotageActionKind> retainedActionKinds = [];
        HashSet<CharacterSabotageActionKind> distinctActionKinds = new();
        foreach (CharacterSabotageActionKind? actionKind in actionKinds)
        {
            if (actionKind is null)
            {
                throw new ArgumentException(
                    "Sabotage action kinds cannot contain null values.",
                    nameof(actionKinds));
            }

            if (!distinctActionKinds.Add(actionKind))
            {
                throw new ArgumentException(
                    "Sabotage action kinds cannot contain duplicate defined instances.",
                    nameof(actionKinds));
            }

            retainedActionKinds.Add(actionKind);
        }

        Actor = actor;
        ActionKinds = retainedActionKinds.AsReadOnly();
    }

    /// <summary>Gets the exact character considering one sabotage action kind.</summary>
    public Character Actor { get; }

    /// <summary>Gets stable caller-supplied action kinds in caller order.</summary>
    public IReadOnlyList<CharacterSabotageActionKind> ActionKinds { get; }
}
