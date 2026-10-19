using System.Numerics;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Navigation;

/// <summary>
/// The native owners a <see cref="CharacterIndependentTaskTickCoordinator"/> sequences. Each
/// member is a thin forward to existing node members; none decides anything.
/// </summary>
public interface ICharacterIndependentTaskTickPorts
{
    /// <summary>Gets the live step cadence.</summary>
    CharacterIndependentTaskStepCadence? Cadence { get; }

    /// <summary>Gets the live step clock.</summary>
    CharacterIndependentTaskStepClock? Clock { get; }

    /// <summary>Gets the outbound travel plan.</summary>
    CharacterIndependentTaskTravelPlan? OutboundPlan { get; }

    /// <summary>Gets the outbound travel progress.</summary>
    CharacterIndependentTaskTravelProgress? OutboundProgress { get; }

    /// <summary>Gets the return travel plan, once outbound travel has completed.</summary>
    CharacterIndependentTaskReturnTravelPlan? ReturnPlan { get; }

    /// <summary>Gets the return travel progress, once outbound travel has completed.</summary>
    CharacterIndependentTaskReturnTravelProgress? ReturnProgress { get; }

    /// <summary>Gets the departed character's active task, if any.</summary>
    CharacterIndependentTaskActivity? ActiveActivity { get; }

    /// <summary>Gets the live independent-task activities.</summary>
    CharacterIndependentTaskActivitySnapshot? Activities { get; }

    /// <summary>Gets the live party.</summary>
    CharacterPartySnapshot? Party { get; }

    /// <summary>Gets the live world-event log.</summary>
    WorldEventLogSnapshot? EventLog { get; }

    /// <summary>Gets the live world time.</summary>
    WorldTimeSnapshot? WorldTime { get; }

    /// <summary>Gets the live character-turn transcript.</summary>
    CharacterTurnTranscriptSnapshot? Transcript { get; }

    /// <summary>Creates the authored arrival occurrence for one active task.</summary>
    CharacterIndependentTaskWorldEventOccurrence CreateOccurrence(
        CharacterIndependentTaskActivity activity);

    /// <summary>Advances the round trip by one bounded elapsed step against exact live state.</summary>
    bool TryAdvanceElapsed(
        CharacterIndependentTaskStepCadence cadence,
        CharacterIndependentTaskStepClock clock,
        TimeSpan elapsed,
        CharacterIndependentTaskTravelPlan outboundPlan,
        CharacterIndependentTaskTravelProgress outboundProgress,
        CharacterIndependentTaskReturnTravelPlan? returnPlan,
        CharacterIndependentTaskReturnTravelProgress? returnProgress,
        Vector2 reachedWaypoint,
        CharacterIndependentTaskWorldEventOccurrence? candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript);

    /// <summary>Presents one exact party change on the characters' bodies.</summary>
    bool TryPresentPartyChange(CharacterPartySnapshot previous, CharacterPartySnapshot current);
}
