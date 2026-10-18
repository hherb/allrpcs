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
    /// Tree-order pins under Main; the first of each pair must come first. The children share the
    /// default priority on purpose, so their tree order is the contract.
    /// </summary>
    internal static readonly (string Earlier, string Later)[] Dependencies =
    [
        // Main's _Process drives the conversation coordinator before the owners it reads or writes
        // run. The turn and dialogue-input controllers have no per-frame callback today; their pins
        // keep the order should they gain one.
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
        // Not a process dependency: WholeChannelPresentation has no per-frame callback. This pins the
        // examination action's long-standing tree position just before it.
        ("HerbExaminationActionController", "WholeChannelPresentation"),
    ];
}
