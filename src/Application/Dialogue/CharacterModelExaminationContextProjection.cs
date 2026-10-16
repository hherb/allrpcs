using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>Supplies only an actor's exact committed examination result as engine context.</summary>
public static class CharacterModelExaminationContextProjection
{
    /// <summary>Rejects foreign actors and results absent from the current exact transcript.</summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        CharacterId actor,
        CharacterExaminationResult? result,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(transcript);
        if (result is null || result.Occurrence.TranscriptEntry.CharacterId != actor
            || !transcript.Entries.Any(entry => ReferenceEquals(entry, result.Occurrence.TranscriptEntry)))
        {
            return Array.Empty<CharacterModelContextEntry>();
        }

        return Array.AsReadOnly(new[]
        {
            new CharacterModelContextEntry(CharacterModelContextEntryKind.ActionOutcome,
                $"Completed examination: actor={actor.Value};target={result.TargetObjectId.Value};"
                    + $"safety={result.Safety.Value};event={result.Occurrence.WorldEvent.EventId.Value}. "
                    + "This records examination only; it does not establish collection or inventory."),
        });
    }
}
