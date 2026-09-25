namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact enemy actor and the cadence provenance for a possible sabotage turn.</summary>
public sealed record CharacterSabotageTurnOpportunity
{
    /// <summary>Creates one immutable opportunity from existing exact provenance.</summary>
    public CharacterSabotageTurnOpportunity(
        Character actor,
        CharacterSabotageTurnOpportunityCadenceSignal signal)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(signal);

        Actor = actor;
        Signal = signal;
    }

    /// <summary>Gets the exact scene-owned enemy actor.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact authoritative cadence signal.</summary>
    public CharacterSabotageTurnOpportunityCadenceSignal Signal { get; }
}
