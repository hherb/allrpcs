using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the immutable discovery snapshots required for later save restoration.</summary>
public sealed record DiscoverySaveState
{
    /// <summary>Creates discovery save state from exact existing snapshots.</summary>
    public DiscoverySaveState(
        CharacterPerceptionSnapshot perception,
        FocusedSearchActivitySnapshot focusedSearchActivities,
        FocusedSearchOutcomeSnapshot focusedSearchOutcomes,
        PassiveNoticingCandidateSnapshot passiveNoticingCandidates,
        ObservationReinterpretationCandidateSnapshot observationReinterpretationCandidates)
    {
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(focusedSearchActivities);
        ArgumentNullException.ThrowIfNull(focusedSearchOutcomes);
        ArgumentNullException.ThrowIfNull(passiveNoticingCandidates);
        ArgumentNullException.ThrowIfNull(observationReinterpretationCandidates);

        Perception = perception;
        FocusedSearchActivities = focusedSearchActivities;
        FocusedSearchOutcomes = focusedSearchOutcomes;
        PassiveNoticingCandidates = passiveNoticingCandidates;
        ObservationReinterpretationCandidates = observationReinterpretationCandidates;
    }

    /// <summary>Gets the exact retained character-perception snapshot.</summary>
    public CharacterPerceptionSnapshot Perception { get; }

    /// <summary>Gets the exact retained focused-search activity snapshot.</summary>
    public FocusedSearchActivitySnapshot FocusedSearchActivities { get; }

    /// <summary>Gets the exact retained focused-search outcome snapshot.</summary>
    public FocusedSearchOutcomeSnapshot FocusedSearchOutcomes { get; }

    /// <summary>Gets the exact retained passive-noticing candidate snapshot.</summary>
    public PassiveNoticingCandidateSnapshot PassiveNoticingCandidates { get; }

    /// <summary>Gets the exact retained observation-reinterpretation candidate snapshot.</summary>
    public ObservationReinterpretationCandidateSnapshot ObservationReinterpretationCandidates
    {
        get;
    }
}
