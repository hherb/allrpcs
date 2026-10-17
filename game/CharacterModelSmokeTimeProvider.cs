namespace Allrpcs.Game;

/// <summary>A smoke-owned clock advanced only after the visible generating-state handshake.</summary>
internal sealed class CharacterModelSmokeTimeProvider : TimeProvider
{
    // Timers may be created on thread-pool continuations while a test thread advances the clock.
    private readonly object gate = new();
    private readonly List<ManualTimer> timers = [];
    private TimeSpan elapsed;
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch + Elapsed;
    public override long GetTimestamp() => Elapsed.Ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public int ActiveTimerCount
    {
        get { lock (gate) return timers.Count(timer => !timer.Disposed); }
    }

    private TimeSpan Elapsed
    {
        get { lock (gate) return elapsed; }
    }

    public void Advance(TimeSpan duration)
    {
        List<ManualTimer> due;
        lock (gate)
        {
            elapsed += duration;
            due = timers.Where(timer => timer.TakeDue(elapsed)).ToList();
        }
        foreach (ManualTimer timer in due)
        {
            timer.Fire();
        }
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        ManualTimer timer = new(this, callback, state);
        timer.Change(dueTime, period);
        lock (gate) timers.Add(timer);
        return timer;
    }

    private sealed class ManualTimer(CharacterModelSmokeTimeProvider owner, TimerCallback callback, object? state) : ITimer
    {
        private TimeSpan? due;
        public bool Disposed { get; private set; }
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (period != Timeout.InfiniteTimeSpan) throw new NotSupportedException();
            lock (owner.gate)
            {
                if (Disposed) return false;
                due = dueTime == Timeout.InfiniteTimeSpan ? null : owner.elapsed + dueTime;
                return true;
            }
        }

        /// <summary>Clears and reports an expired due time; the caller holds the owner's lock.</summary>
        public bool TakeDue(TimeSpan now)
        {
            if (Disposed || due is not { } deadline || now < deadline) return false;
            due = null;
            return true;
        }

        public void Fire() => callback(state);

        public void Dispose()
        {
            lock (owner.gate) Disposed = true;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
