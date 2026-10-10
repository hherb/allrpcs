using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;

namespace Allrpcs.Application.Actions;

/// <summary>Owns exact transient contact requests and commits without mutating world truth.</summary>
public sealed class StreamWaterContactController
{
    /// <summary>The active-time cooldown after each committed contact.</summary>
    public const double CooldownSeconds = 2;
    /// <summary>Maximum admitted active frame duration; larger frames cannot catch up.</summary>
    public const double MaximumDeltaSeconds = 0.25;
    private StreamWaterContactRequest? pending;

    /// <summary>Starts a new session-local owner bound to the current actor and site.</summary>
    public StreamWaterContactController(Character actor, StreamWaterContactSite site) => State = new(actor, site);
    /// <summary>The current immutable state; previous instances cannot authorize another commit.</summary>
    public StreamWaterContactState State { get; private set; }
    /// <summary>The latest exact accepted commit, or null after reset.</summary>
    public StreamWaterContactCommit? LastCommit { get; private set; }
    /// <summary>Counts isolated observer failures without retrying or reversing accepted actions.</summary>
    public long ObserverFailureCount { get; private set; }
    /// <summary>Raised only after the accepted state replacement and cooldown installation.</summary>
    public event Action<StreamWaterContactCommit>? Committed;

    /// <summary>Issues one exact attempt; copied requests are never owner-issued attempts.</summary>
    public StreamWaterContactRequest? CreateRequest()
    {
        if (State.Cooldown > 0 || State.Sequence == long.MaxValue
            || State.Actor.ControllerAssignment.ControllerType != ControllerType.Human) { return null; }
        return pending ??= new(State.Actor, State.Site, State);
    }

    /// <summary>Atomically admits only the exact current request and newly observed live access.</summary>
    public bool TryCommit(StreamWaterContactRequest? request, Character currentActor, bool liveAccess)
    {
        ArgumentNullException.ThrowIfNull(currentActor);
        if (!liveAccess || request is null || !ReferenceEquals(request, pending)
            || !ReferenceEquals(request.State, State) || !ReferenceEquals(request.Actor, currentActor)
            || !ReferenceEquals(State.Actor, currentActor) || !ReferenceEquals(request.Site, State.Site)
            || currentActor.ControllerAssignment.ControllerType != ControllerType.Human || State.Cooldown > 0
            || State.Sequence == long.MaxValue) { return false; }
        StreamWaterContactCommit commit = new(request, State.Sequence + 1);
        State = new(currentActor, State.Site, commit.Sequence, CooldownSeconds);
        pending = null; LastCommit = commit;
        foreach (Delegate observer in Committed?.GetInvocationList() ?? [])
        {
            try { ((Action<StreamWaterContactCommit>)observer)(commit); }
            catch (Exception) { ObserverFailureCount++; }
        }
        return true;
    }

    /// <summary>Advances only bounded active deltas; invalid time preserves the remaining cooldown.</summary>
    public bool Advance(double delta, bool active)
    {
        if (!double.IsFinite(delta) || delta < 0 || delta > MaximumDeltaSeconds) { return false; }
        if (active && State.Cooldown > 0)
        {
            double remaining = Math.Max(0, State.Cooldown - delta);
            if (remaining < 1e-9) { remaining = 0; }
            State = new(State.Actor, State.Site, State.Sequence, remaining);
        }
        return true;
    }

    /// <summary>Invalidates attempts and presentation tokens after successful load or owner loss.</summary>
    public void Reset()
    {
        State = new(State.Actor, State.Site, State.Sequence); pending = null; LastCommit = null;
    }
}
