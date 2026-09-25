namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact sabotage opportunity that exclusively owns the shared turn slot.</summary>
public sealed record CharacterSabotageTurnReservation
{
    /// <summary>Creates one immutable reservation from an existing exact opportunity.</summary>
    public CharacterSabotageTurnReservation(CharacterSabotageTurnOpportunity opportunity)
    {
        ArgumentNullException.ThrowIfNull(opportunity);

        Opportunity = opportunity;
    }

    /// <summary>Gets the exact opportunity that owns the shared turn slot.</summary>
    public CharacterSabotageTurnOpportunity Opportunity { get; }
}
