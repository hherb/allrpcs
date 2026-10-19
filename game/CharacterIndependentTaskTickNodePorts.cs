using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Game;

/// <summary>Forwards each independent-task tick port to its Main-scene owners.</summary>
internal sealed class CharacterIndependentTaskTickNodePorts(
    CharacterIndependentTaskStepClockController stepClock,
    CharacterIndependentTaskTravelController travel,
    CharacterIndependentTaskReturnTravelController returnTravel,
    CharacterIndependentTaskDepartureController departure,
    TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource,
    CharacterIndependentTaskRoundTripCoordinator roundTrip,
    CharacterIndependentTaskBodyPresentationController bodyPresentation,
    WorldEventController worldEvents,
    CharacterTurnTranscriptController transcript)
    : ICharacterIndependentTaskTickPorts
{
    public CharacterIndependentTaskStepCadence? Cadence => stepClock.Cadence;

    public CharacterIndependentTaskStepClock? Clock => stepClock.Clock;

    public CharacterIndependentTaskTravelPlan? OutboundPlan => travel.Plan;

    public CharacterIndependentTaskTravelProgress? OutboundProgress => travel.Progress;

    public CharacterIndependentTaskReturnTravelPlan? ReturnPlan => returnTravel.Plan;

    public CharacterIndependentTaskReturnTravelProgress? ReturnProgress => returnTravel.Progress;

    public CharacterIndependentTaskActivity? ActiveActivity => departure.ActiveActivity;

    public CharacterIndependentTaskActivitySnapshot? Activities => departure.Activities;

    public CharacterPartySnapshot? Party => departure.Party;

    public WorldEventLogSnapshot? EventLog => worldEvents.EventLog;

    public WorldTimeSnapshot? WorldTime => worldEvents.WorldTime;

    public CharacterTurnTranscriptSnapshot? Transcript => transcript.Transcript;

    public CharacterIndependentTaskWorldEventOccurrence CreateOccurrence(
        CharacterIndependentTaskActivity activity) =>
        occurrenceSource.CreateOccurrence(activity);

    public bool TryAdvanceElapsed(
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
        CharacterTurnTranscriptSnapshot transcript) =>
        roundTrip.TryAdvanceElapsed(
            cadence,
            clock,
            elapsed,
            outboundPlan,
            outboundProgress,
            returnPlan,
            returnProgress,
            reachedWaypoint,
            candidate,
            activities,
            party,
            eventLog,
            worldTime,
            transcript);

    public bool TryPresentPartyChange(
        CharacterPartySnapshot previous,
        CharacterPartySnapshot current) =>
        bodyPresentation.TryPresent(previous, current);
}
