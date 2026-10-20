using System.Numerics;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Navigation;

/// <summary>
/// Advances the departed character's independent-task round trip by one frame: outbound travel
/// toward the task (offering its arrival occurrence), then return travel, then nothing once the
/// return is complete. Missing live state is an invariant violation and throws.
/// </summary>
public sealed class CharacterIndependentTaskTickCoordinator
{
    private readonly ICharacterIndependentTaskTickPorts ports;

    /// <summary>Creates a coordinator over the exact native owners.</summary>
    public CharacterIndependentTaskTickCoordinator(ICharacterIndependentTaskTickPorts ports)
    {
        ArgumentNullException.ThrowIfNull(ports);
        this.ports = ports;
    }

    /// <summary>Advances one frame of <paramref name="delta"/> seconds.</summary>
    public void Process(double delta)
    {
        CharacterIndependentTaskStepCadence cadence = ports.Cadence
            ?? throw new InvalidOperationException(
                "The independent-task step cadence was not initialized.");
        CharacterIndependentTaskStepClock clock = ports.Clock
            ?? throw new InvalidOperationException(
                "The independent-task step clock was not initialized.");
        CharacterIndependentTaskTravelPlan outboundPlan = ports.OutboundPlan
            ?? throw new InvalidOperationException(
                "The independent-task outbound plan was not initialized.");
        CharacterIndependentTaskTravelProgress outboundProgress = ports.OutboundProgress
            ?? throw new InvalidOperationException(
                "The independent-task outbound progress was not initialized.");
        CharacterIndependentTaskReturnTravelPlan? returnPlan = ports.ReturnPlan;
        CharacterIndependentTaskReturnTravelProgress? returnProgress = ports.ReturnProgress;
        if ((returnPlan is null) != (returnProgress is null))
        {
            throw new InvalidOperationException(
                "The independent-task return plan and progress must be jointly present.");
        }

        if (returnProgress?.IsComplete == true)
        {
            return;
        }

        CharacterIndependentTaskWorldEventOccurrence? candidate = null;
        Vector2 reachedWaypoint;
        if (returnPlan is not null && returnProgress is not null)
        {
            reachedWaypoint = returnPlan.Waypoints[returnProgress.CompletedWaypointCount];
        }
        else
        {
            CharacterIndependentTaskActivity activeActivity = ports.ActiveActivity
                ?? throw new InvalidOperationException(
                    "Outbound independent-task travel requires its exact active activity.");
            if (outboundProgress.IsComplete)
            {
                throw new InvalidOperationException(
                    "Completed outbound travel did not initialize exact return travel.");
            }

            candidate = ports.CreateOccurrence(activeActivity);
            reachedWaypoint = outboundPlan.Waypoints[outboundProgress.CompletedWaypointCount];
        }

        CharacterIndependentTaskActivitySnapshot activities = ports.Activities
            ?? throw new InvalidOperationException(
                "Independent-task activities were not initialized.");
        CharacterPartySnapshot party = ports.Party
            ?? throw new InvalidOperationException(
                "Independent-task party state was not initialized.");
        WorldEventLogSnapshot eventLog = ports.EventLog
            ?? throw new InvalidOperationException(
                "World-event log state was not initialized.");
        WorldTimeSnapshot worldTime = ports.WorldTime
            ?? throw new InvalidOperationException(
                "World-time state was not initialized.");
        CharacterTurnTranscriptSnapshot transcript = ports.Transcript
            ?? throw new InvalidOperationException(
                "Character-turn transcript state was not initialized.");
        if (!ports.TryAdvanceElapsed(
                cadence,
                clock,
                BoundedElapsed(delta, cadence),
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
                transcript))
        {
            throw new InvalidOperationException(
                "The exact live independent-task elapsed step could not advance.");
        }

        CharacterPartySnapshot currentParty = ports.Party
            ?? throw new InvalidOperationException(
                "Independent-task party state disappeared after elapsed processing.");
        if (!ReferenceEquals(currentParty, party)
            && !ports.TryPresentPartyChange(party, currentParty))
        {
            throw new InvalidOperationException(
                "The exact independent-task party change could not update body presentation.");
        }
    }

    private static TimeSpan BoundedElapsed(
        double delta,
        CharacterIndependentTaskStepCadence cadence)
    {
        if (!double.IsFinite(delta) || delta < 0.0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delta),
                delta,
                "Independent-task frame time must be finite and non-negative.");
        }

        return TimeSpan.FromSeconds(Math.Min(delta, cadence.Interval.TotalSeconds));
    }
}
