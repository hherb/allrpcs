namespace Allrpcs.Game;

/// <summary>
/// The same-frame processing contract of Main and its order-dependent children. Godot processes
/// lower priorities first and breaks ties in tree order; <see cref="MainProcessOrderCheck"/>
/// asserts every entry.
/// </summary>
internal static class MainProcessOrder
{
    /// <summary>
    /// After the early audio, water and tool coordinators (-50 to -43) and the -100 check
    /// fixtures; before every default-priority child.
    /// </summary>
    internal const int MainPriority = -40;

    /// <summary>
    /// Node paths under Main; the first of each pair must process first. The children share the
    /// default priority on purpose, so their tree order is the contract.
    /// </summary>
    internal static readonly (string Earlier, string Later)[] Dependencies =
    [
        // Main drives the conversation and focused-search coordinators before their owners run.
        (".", "CharacterTurnActivityController"),
        (".", "DialogueInputController"),
        (".", "CharacterTurnParticipationRoundTaskController"),
        (".", "OrdinaryPlayerDialogueParticipationCoordinator"),
        (".", "CharacterModelGenerationFlowController"),
        (".", "TemporaryCharacterTurnResponseSpeechRecorder"),
        (".", "HerbExaminationActionController"),
        // The coordinator consumes a round result the task finished earlier in the same frame.
        ("CharacterTurnParticipationRoundTaskController", "OrdinaryPlayerDialogueParticipationCoordinator"),
        // Reply speech is recorded before the completion controller ends the Speaking turn.
        ("TemporaryCharacterTurnResponseSpeechRecorder", "TemporaryCharacterTurnCompletionController"),
        ("HerbExaminationActionController", "WholeChannelPresentation"),
    ];
}
