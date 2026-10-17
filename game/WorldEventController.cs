using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns and atomically updates the scene's authoritative world-event state.</summary>
public partial class WorldEventController : Node
{
    /// <summary>Gets the initialized immutable world-event snapshot, if any.</summary>
    public WorldEventSnapshot? WorldEvents { get; private set; }

    /// <summary>Gets the initialized immutable world-event identifier log, if any.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the initialized immutable qualitative world-time state, if any.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <summary>Initializes this controller once from exact existing world-event state.</summary>
    public bool TryInitialize(
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);

        if (WorldEvents is not null || EventLog is not null || WorldTime is not null)
        {
            return false;
        }

        WorldEvents = worldEvents;
        EventLog = eventLog;
        WorldTime = worldTime;
        return true;
    }

    /// <summary>Atomically records one eligible event and applies its qualitative-time effect.</summary>
    public bool TryOccur(WorldEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        WorldEventSnapshot? worldEvents = WorldEvents;
        WorldEventLogSnapshot? eventLog = EventLog;
        WorldTimeSnapshot? worldTime = WorldTime;
        if (worldEvents is null
            || eventLog is null
            || worldTime is null
            || !WorldEventOccurrencePolicy.CanOccur(eventId, worldEvents, eventLog, worldTime))
        {
            return false;
        }

        WorldEventOccurrenceResult result = WorldEventOccurrenceTransition.Occur(
            eventId,
            worldEvents,
            eventLog,
            worldTime);
        EventLog = result.EventLog;
        WorldTime = result.WorldTime;
        return true;
    }

    /// <summary>Restores exact eligible event-log and qualitative-time save snapshots.</summary>
    public bool TryRestoreWorldEventState(WorldEventSaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        WorldEventSnapshot? worldEvents = DefinitionsFor(state);
        if (worldEvents is null
            || !WorldEventSaveStateRestorationPolicy.CanRestore(state, worldEvents))
        {
            return false;
        }

        InstallPreparedRestoration(state, worldEvents);
        return true;
    }

    internal void InstallPreparedRestoration(WorldEventSaveState state, WorldEventSnapshot worldEvents)
    {
        WorldEvents = worldEvents;
        EventLog = state.EventLog;
        WorldTime = state.WorldTime;
    }

    /// <summary>Finds the exact initialized world event with the supplied identifier.</summary>
    public WorldEvent? FindWorldEvent(WorldEventId eventId)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        WorldEventSnapshot? worldEvents = WorldEvents;
        return worldEvents is null
            ? null
            : WorldEventLookup.FindById(eventId, worldEvents);
    }

    // Preflight is pure: a rejected load never changes the authoritative catalog.
    internal WorldEventSnapshot? DefinitionsFor(WorldEventSaveState state)
    {
        if (WorldEvents is null) { return null; }
        try { return HerbExaminationEventCatalog.Expand(WorldEvents, state.EventLog); }
        catch (ArgumentException) { return null; }
    }

    /// <summary>Commits one arrived examination and its exact action transcript atomically.</summary>
    internal CharacterTurnWorldEventOccurrence? TryOccurExamination(AcceptedCharacterExamination execution,
        CharacterTurnTranscriptController transcriptController)
    {
        if (WorldEvents is null || EventLog is null || WorldTime is null
            || transcriptController.Transcript is not { } transcript) { return null; }
        WorldEvent next = HerbExaminationEventCatalog.Next(WorldEvents, EventLog);
        WorldEventSnapshot definitions = WorldEventLookup.FindById(next.EventId, WorldEvents) is null
            ? new(WorldEvents.Entries.Append(next)) : WorldEvents;
        CharacterTurnWorldEventOccurrence occurrence = new(next, execution.CreateTranscriptEntry());
        if (!CharacterTurnWorldEventOccurrencePolicy.CanOccur(occurrence, definitions, EventLog, WorldTime))
        { return null; }
        var result = CharacterTurnWorldEventOccurrenceTransition.Occur(
            occurrence, definitions, EventLog, WorldTime, transcript);
        WorldEvents = definitions;
        InstallCharacterTurnOccurrence(result.WorldEventResult);
        transcriptController.InstallCharacterTurnOccurrence(result.Transcript);
        return occurrence;
    }

    internal void InstallCharacterTurnOccurrence(WorldEventOccurrenceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        EventLog = result.EventLog;
        WorldTime = result.WorldTime;
    }

    internal void InstallFirstPatientAdministration(
        FirstPatientAdministrationOccurrenceResult result)
    {
        EventLog = result.EventLog;
        WorldTime = result.WorldTime;
    }
}
