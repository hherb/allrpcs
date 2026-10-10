using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Actions;

/// <summary>Owns exact transient handling requests and commits without mutating world or inventory truth.</summary>
public sealed class CaskCoverHandlingController
{
    /// <summary>The active-time cooldown after each committed handling.</summary>
    public const double CooldownSeconds = 4;
    /// <summary>Maximum admitted active frame duration; larger frames cannot catch up.</summary>
    public const double MaximumDeltaSeconds = 0.25;
    private CaskCoverHandlingRequest? pending;

    /// <summary>Starts a new session-local owner bound to the current actor and material.</summary>
    public CaskCoverHandlingController(Character actor, CaskCoverHandlingMaterial material) => State = new(actor, material);
    /// <summary>The current immutable state; previous instances cannot authorize another commit.</summary>
    public CaskCoverHandlingState State { get; private set; }
    /// <summary>The latest exact accepted commit, or null after reset.</summary>
    public CaskCoverHandlingCommit? LastCommit { get; private set; }
    /// <summary>Counts isolated observer failures without retrying or reversing accepted actions.</summary>
    public long ObserverFailureCount { get; private set; }
    /// <summary>Raised only after the accepted state replacement and cooldown installation.</summary>
    public event Action<CaskCoverHandlingCommit>? Committed;

    /// <summary>Issues one exact attempt; copied requests are never owner-issued attempts.</summary>
    public CaskCoverHandlingRequest? CreateRequest()
    {
        if (State.Cooldown > 0 || State.Sequence == long.MaxValue
            || State.Actor.ControllerAssignment.ControllerType != ControllerType.Human) { return null; }
        return pending ??= new(State.Actor, State.Material, State);
    }

    /// <summary>Atomically admits only the exact current request and newly observed live access.</summary>
    public bool TryCommit(CaskCoverHandlingRequest? request, Character currentActor, WorldObject currentTarget, CaskCoverHandlingMaterial currentMaterial, bool liveAccess)
    {
        ArgumentNullException.ThrowIfNull(currentActor);
        ArgumentNullException.ThrowIfNull(currentTarget); ArgumentNullException.ThrowIfNull(currentMaterial);
        if (!liveAccess || request is null || !ReferenceEquals(request, pending)
            || !ReferenceEquals(request.State, State) || !ReferenceEquals(request.Actor, currentActor)
            || !ReferenceEquals(State.Actor, currentActor) || !ReferenceEquals(request.Material, State.Material)
            || currentActor.ControllerAssignment.ControllerType != ControllerType.Human || State.Cooldown > 0
            || State.Sequence == long.MaxValue || !ReferenceEquals(request.Target, currentTarget)
            || !ReferenceEquals(State.Material, currentMaterial)) { return false; }
        CaskCoverHandlingCommit commit = new(request, State.Sequence + 1);
        State = new(currentActor, State.Material, commit.Sequence, CooldownSeconds);
        pending = null; LastCommit = commit;
        foreach (Delegate observer in Committed?.GetInvocationList() ?? [])
        {
            try { ((Action<CaskCoverHandlingCommit>)observer)(commit); }
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
            State = new(State.Actor, State.Material, State.Sequence, remaining);
        }
        return true;
    }

    /// <summary>Invalidates attempts and presentation tokens after successful load or owner loss.</summary>
    public void Reset()
    {
        State = new(State.Actor, State.Material, State.Sequence); pending = null; LastCommit = null;
    }
}
