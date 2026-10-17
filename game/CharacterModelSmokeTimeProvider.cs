namespace Allrpcs.Game;

/// <summary>A smoke-owned clock advanced only after the visible generating-state handshake.</summary>
internal sealed class CharacterModelSmokeTimeProvider : TimeProvider
{
    private readonly List<ManualTimer> timers = [];
    private TimeSpan elapsed;
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch + elapsed;
    public override long GetTimestamp() => elapsed.Ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public int ActiveTimerCount => timers.Count(timer => !timer.Disposed);

    public void Advance(TimeSpan duration)
    {
        elapsed += duration;
        foreach (ManualTimer timer in timers.ToArray())
        {
            timer.Fire(elapsed);
        }
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        ManualTimer timer = new(this, callback, state);
        timer.Change(dueTime, period);
        timers.Add(timer);
        return timer;
    }

    private sealed class ManualTimer(CharacterModelSmokeTimeProvider owner, TimerCallback callback, object? state) : ITimer
    {
        private TimeSpan? due;
        public bool Disposed { get; private set; }
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (Disposed) return false;
            if (period != Timeout.InfiniteTimeSpan) throw new NotSupportedException();
            due = dueTime == Timeout.InfiniteTimeSpan ? null : owner.elapsed + dueTime;
            return true;
        }

        public void Fire(TimeSpan now)
        {
            if (!Disposed && due is { } deadline && now >= deadline)
            {
                due = null;
                callback(state);
            }
        }

        public void Dispose() => Disposed = true;
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
