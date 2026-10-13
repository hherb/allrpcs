using System.Numerics;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Audio;

/// <summary>Qualifies anonymous presentation of an already installed exact collection.</summary>
public static class HerbCollectionPresentationProjection
{
    /// <summary>Anonymous wording with no collection, actor or safety information.</summary>
    public const string Phrase = "Soft rustle";
    /// <summary>Independent readable lifetime.</summary>
    public const double CaptionSeconds = 2;
    /// <summary>Fixed nonlooping lossless passage.</summary>
    public const double PassageSeconds = 1;
    /// <summary>Inclusive actual actor distance from the retained patch.</summary>
    public const float ActorPixels = 32;
    /// <summary>Exclusive listener admission radius.</summary>
    public const float ListenerPixels = 192;
    /// <summary>Discontinuity cancels current feedback.</summary>
    public const float TeleportPixels = 96;

    /// <summary>Requires exact result, actor, target, examination and collection definition identities.</summary>
    public static bool IsCommit(CharacterExaminationResult? result, CharacterTurnWorldEventOccurrence? occurrence,
        CharacterId? actor, ObjectId? target, WorldEvent? examined, WorldEvent? collected) =>
        result is not null && occurrence is not null && actor is not null && target is not null
        && examined is not null && collected is not null
        && ReferenceEquals(result.Safety, CharacterExaminationSafety.SafeToCollect)
        && ReferenceEquals(result.Occurrence.WorldEvent, examined)
        && ReferenceEquals(result.TargetObjectId, target)
        && ReferenceEquals(result.Occurrence.TranscriptEntry.CharacterId, actor)
        && ReferenceEquals(occurrence.TranscriptEntry.CharacterId, actor)
        && ReferenceEquals(occurrence.WorldEvent, collected)
        && occurrence.TranscriptEntry.PhysicalAction is { } action
        && action.Type == CharacterIntentionType.CollectObject
        && ReferenceEquals(action.TargetObjectId, target);

    /// <summary>Checks actual finite bodies; coordinates supplied by a caller cannot grant admission.</summary>
    public static bool Local(Vector2 actor, Vector2 listener, Vector2 patch) =>
        Finite(actor) && Finite(listener) && Finite(patch)
        && Vector2.DistanceSquared(actor, patch) <= ActorPixels * ActorPixels
        && Vector2.DistanceSquared(listener, patch) < ListenerPixels * ListenerPixels;
    /// <summary>Only ordinary positive finite frame deltas retain transient feedback.</summary>
    public static bool ValidDelta(double delta) => double.IsFinite(delta) && delta > 0 && delta <= 0.1;
    /// <summary>Invalid positions fail closed.</summary>
    public static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
}
