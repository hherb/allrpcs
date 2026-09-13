using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains the immutable task and authoritative-world results of one occurrence.</summary>
public sealed record CharacterIndependentTaskWorldEventOccurrenceResult
{
    /// <summary>Creates one result retaining the exact supplied values.</summary>
    public CharacterIndependentTaskWorldEventOccurrenceResult(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterTurnWorldEventOccurrenceResult worldEventResult)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(worldEventResult);

        Candidate = candidate;
        Activities = activities;
        WorldEventResult = worldEventResult;
    }

    /// <summary>Gets the exact validated independent-task occurrence candidate.</summary>
    public CharacterIndependentTaskWorldEventOccurrence Candidate { get; }

    /// <summary>Gets the resulting lifecycle snapshot containing the completed task.</summary>
    public CharacterIndependentTaskActivitySnapshot Activities { get; }

    /// <summary>Gets the resulting authoritative world-event and transcript state.</summary>
    public CharacterTurnWorldEventOccurrenceResult WorldEventResult { get; }
}
