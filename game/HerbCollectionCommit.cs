using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Game;

/// <summary>Producer-owned exact installed references; rebuilding this record creates no authority.</summary>
public sealed record HerbCollectionCommit(
    TemporaryCharacterExaminationFlowController Owner,
    CharacterExaminationResult Result,
    CharacterTurnWorldEventOccurrence Occurrence,
    WorldEventLogSnapshot EventLog,
    WorldTimeSnapshot WorldTime,
    CharacterTurnTranscriptSnapshot Transcript);
