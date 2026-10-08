using System.Numerics;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Audio;

/// <summary>Pure spatial presentation rules; never grants hearing or world knowledge.</summary>
public static class OpeningRegionPositionalAudioProjection
{
    /// <summary>World-pixel falloff for the existing composite waterway bed.</summary>
    public const float WaterwayRange = 768;
    /// <summary>Discontinuity that cancels transient reply presentation.</summary>
    public const float TeleportDistance = 96;
    /// <summary>Optional reply caption lifetime in presentation seconds.</summary>
    public const double CaptionSeconds = 3;
    private static readonly string[] Directions =
        ["above", "above/right", "right", "below/right", "below", "below/left", "left", "above/left"];

    /// <summary>Rejects nonfinite spatial input before it reaches a player or caption.</summary>
    public static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);

    /// <summary>Clamps to supplied retained rectangular water geometry; invalid geometry has no source.</summary>
    public static Vector2? NearestWaterPoint(Vector2 position, IReadOnlyList<Vector2> polygon)
    {
        if (!Finite(position) || polygon.Count != 4 || polygon.Any(point => !Finite(point))) { return null; }
        float left = polygon.Min(point => point.X), right = polygon.Max(point => point.X);
        float top = polygon.Min(point => point.Y), bottom = polygon.Max(point => point.Y);
        if (left >= right || top >= bottom
            || !polygon.ToHashSet().SetEquals([new(left, top), new(right, top), new(right, bottom), new(left, bottom)]))
        { return null; }
        return new Vector2(Math.Clamp(position.X, left, right), Math.Clamp(position.Y, top, bottom));
    }

    /// <summary>Expected player distance factor, independently of category gain and camera zoom.</summary>
    public static double DistanceGain(Vector2 source, Vector2 listener, float range) =>
        Finite(source) && Finite(listener) && float.IsFinite(range) && range > 0
            ? Math.Max(0, 1 - Vector2.Distance(source, listener) / (double)range) : 0;

    /// <summary>Allows only an already heard exact recorded delivery; proximity can only remove admission.</summary>
    public static bool Admitted(DialogueEvent? occurred, CharacterTurnTranscriptSnapshot? transcript,
        CharacterId? seeker, CharacterId? registeredSpeaker, Vector2 listener, Vector2 source, float range)
    {
        if (occurred is null || transcript is null || seeker is null || registeredSpeaker is null
            || occurred.Utterance.SpeakerCharacterId != registeredSpeaker || seeker == registeredSpeaker
            || !occurred.Audience.ListenerCharacterIds.Contains(seeker)
            || !transcript.Entries.Any(entry => ReferenceEquals(entry.Delivery, occurred.Delivery))
            || !Finite(listener) || !Finite(source)) { return false; }
        float expected = occurred.Delivery.Mode == DialogueDeliveryMode.Whispering ? 48 : 192;
        return range == expected && Vector2.DistanceSquared(source, listener) <= range * range;
    }

    /// <summary>Provides inclusive viewport admission for presentation, never for hearing.</summary>
    public static bool OnScreen(Vector2 point, Vector2 viewport) =>
        Finite(point) && Finite(viewport) && viewport.X > 0 && viewport.Y > 0
        && point.X >= 0 && point.Y >= 0 && point.X <= viewport.X && point.Y <= viewport.Y;

    /// <summary>Names one of eight screen bearings with clockwise ties and no source identity.</summary>
    public static string Caption(string prefix, Vector2 screenPoint, Vector2 viewport)
    {
        if (OnScreen(screenPoint, viewport)) { return prefix == "Conversation" ? "Conversation nearby" : prefix; }
        if (!Finite(screenPoint) || !Finite(viewport) || viewport.X <= 0 || viewport.Y <= 0) { return prefix; }
        Vector2 offset = screenPoint - viewport / 2;
        if (offset.LengthSquared() == 0) { return prefix; }
        double clockwise = Math.Atan2(offset.X, -offset.Y);
        int sector = ((int)Math.Floor(clockwise / (Math.PI / 4) + 0.5) % 8 + 8) % 8;
        return prefix + " " + Directions[sector];
    }
}

/// <summary>Bounded transient generic cue history and a presentation-time rate limit.</summary>
public sealed class PositionalCueHistory
{
    /// <summary>Maximum retained accessible records; unrelated world history is unchanged.</summary>
    public const int Capacity = 16;
    private readonly List<string> entries = [];
    private long sequence;
    private double sinceClick = 1;
    /// <summary>A detached ordered view so UI cannot mutate retained history.</summary>
    public IReadOnlyList<string> Entries => entries.ToArray();
    /// <summary>Advances only the click limiter, without world time.</summary>
    public void Advance(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) { throw new ArgumentOutOfRangeException(nameof(seconds)); }
        sinceClick = Math.Min(1, sinceClick + seconds);
    }
    /// <summary>Records each admitted event even if its optional click is rate limited.</summary>
    public bool Record(string caption)
    {
        entries.Add($"{++sequence}. {caption}");
        if (entries.Count > Capacity) { entries.RemoveAt(0); }
        bool start = sinceClick >= 1;
        if (start) { sinceClick = 0; }
        return start;
    }
    /// <summary>Load/teardown discards transient cue ownership, without replay.</summary>
    public void Clear() { entries.Clear(); sequence = 0; sinceClick = 1; }
}
