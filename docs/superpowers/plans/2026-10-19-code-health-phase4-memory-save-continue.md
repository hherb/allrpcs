# Phase 4 item 2: memory, save and continue

Owner authority: `owner-playable-phase4-2026-10-19` in `CONTINUATION.json`, under program Phase 4
item 2 in [the program plan](2026-10-17-code-health-and-playable-slice.md). Closes #452.

## Measured state (at `b588899c`)

- The save root is schema 8 (`SaveSchemaVersion.FirstPatientRecovery`), written to
  `user://save-v1.json` by `SaveDocumentStorageController` through `SaveSnapshotJsonController`.
  Capture/restoration are `SaveSnapshotCaptureController` and `SaveSnapshotRestorationController`
  (atomic `RestoreParticipantRegistry`: every owner prepares, then all commit).
- Character memory is already in the save: `CharacterMemoryController.Memory` is captured and
  restored, and `CharacterMemoryJsonWriter/Reader` round-trip each character's recent-conversation
  window (the completed turns: what was heard and the reply) and episodic memories. But nothing
  ever writes memory: it stays the empty snapshot, so every save holds no transcript.
- The LLM sees only memory commitments (`CharacterModelCommitmentMemoryContextProjection`) plus
  `DialogueEventController.History`, which a successful load clears. A loaded character forgets
  everything said before.
- The save omits the Seeker profile and body positions (Seeker and Healer are the only
  `CharacterBody2D`s; the Sage is a marker). Customization applies once per session
  (`SeekerCustomizationApplicationCoordinator.TryApply` requires an unselected state).
- Since item 1 a plain launch opens customization, which blocks world input, so L cannot load
  until the player customizes again, possibly with another background (#452).

## Change

1. **Memory writes (Application).** `CharacterMemoryRecordingTransition` appends a completed turn
   to the responder's recent-conversation window and records one episodic memory of engine facts
   only (who spoke, the dialogue act, whether the character replied, the turn); a second entry
   point records an observation episode (the healer's committed herb examination and its safety).
   Summaries never quote speech: memory reaches the model as trusted engine context, so player or
   model text there would be a prompt-injection path. Episodic memories evict oldest-first at the domain bound, along
   with index entries that reference them. `CharacterTurnActivityController` raises `Completed`
   for ordinary and autonomous completions; `CharacterMemoryController` records from it and from
   `HerbExaminationActionController`'s committed result.
2. **Memory reaches the model.** `CharacterModelEpisodicMemoryContextProjection` adds the
   responder's newest episodes as optional `Memory` context next to commitments. After a load,
   `CharacterConversationHistory.RestoreFrom` offers each character its saved recent-conversation
   window ahead of later speech, through the existing untrusted recent-speech channel, so a loaded
   character still knows what was said.
3. **Schema 9 `Continuation` (Domain/Infrastructure).** `SaveContinuationState` holds the selected
   Seeker profile as text and choice keys (`SeekerCustomizationSaveState`, or none) and exact body
   positions (`CharacterPositionSaveState`). Schema 8 migrates to 9 with no continuation state.
   The JSON is strict like every other section; unknown versions stay rejected.
4. **Restoring customization (Application).** `SeekerCustomizationRestorationPolicy` decides,
   from the session's selection state and the saved profile: keep (nothing saved, or the same
   profile), apply (session unselected: rebuild the selection from the option keys), or reject
   (a different Seeker is already customized, or an unknown key). The coordinator's checks move
   into `Prepare`, which restoration runs in its preflight; the commit only installs the prepared
   plan, so a rejected profile rejects the whole load instead of half-applying it. Loading while the customization menu is open closes it (the loaded game
   replaces the new one).
5. **Positions.** Capture records the Seeker and Healer positions; restoration moves both bodies
   and zeroes their velocity.
6. **Continue on launch.** When a save exists, the new-game start shows a start panel with
   Continue and New game instead of customization. Continue loads; on success it hides, and opens
   customization only if the loaded save had no profile. A failed load stays on the panel with a
   status. New game opens customization. The saved guidance now says to choose Continue.
7. **Proof.** `SaveContinueRelaunchCheck` (standalone scene, `ALLRPCS_SAVE_CONTINUE_CHECK`)
   customizes Forester, completes a dialogue turn and a movement, saves, frees Main, instantiates a
   fresh Main, chooses Continue, and requires the same memory JSON, profile, open forest gate and
   positions, with customization not reopened. Registered in `scripts/smoke-godot.sh`.

Out of scope: items 3–6; NPC task state, independent-task progress and in-flight turns stay
session-only (the existing resets); no new sabotage kind; no provider qualification.

## Verification

`dotnet format`/`build`/the four test projects (new xUnit tests for schema 9, its JSON, memory
recording, the episodic projection and the customization policy), the new check, the save/load,
restoration, customization, new-game and conversation launches, then the full smoke suite.
