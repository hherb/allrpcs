# Allrpcs Handover

## Current state

`main` contains merged PR #147 at `b5c298`. The branch
`codex/phase-3-dialogue-panel` continues Phase 3 through these reviewed slices:

- Phase 3k1 adds a temporary bottom dialogue panel and focused `DialogueInputController`. Return or
  the button submits the player's exact nonblank text as a `DialogueUtterance` for the exact scene
  Seeker. Domain validation rejects null, blank, and text above 2,000 UTF-16 code units without
  changing the last accepted utterance.
- Phase 3k2 converts each fresh accepted utterance exactly once into a `DialogueEvent`. The existing
  pure hearing selector receives the exact Seeker and temporary healer identifiers, current scene
  positions, and a named inclusive 192-pixel range. The event controller retains the exact event and
  stable heard audience.
- Phase 3k3 initializes one empty scene `CharacterTurnTranscriptSnapshot` and appends the same exact
  player utterance as a structurally tagged spoken statement. Repeated equal text remains distinct
  ordered occurrences.

`ROADMAP.md` now records unrestricted dialogue input separately from still-incomplete streaming
response display. These slices do not classify the player's text, select a responder, construct or
start a character-turn request, generate or display a response, append healer speech or physical
actions, execute intentions or proposals, add persistence/providers/streaming, or complete the
Phase 3 acceptance scenario.

## Validation

Each slice began with an expected missing-type build failure. Focused host-local dialogue-input,
dialogue-event, and character-turn-transcript Godot smokes pass. The complete host-local
`./scripts/quality.sh` run passes workflow and formatting validation, a zero-warning build, 1,330
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 40 opt-in Godot smokes, and the bounded
graphical launch.

The restricted sandbox still reproduces the known Godot `user://logs` crash, so the executable
Godot evidence above is host-local. The GitHub Actions budget is exhausted: do not manually dispatch
hosted workflows, and treat any zero-step hosted failures as infrastructure/account state rather
than test results.

## Exact next bounded slice

Take **Phase 3k4: derive one explicit temporary healer turn request from the occurred player
dialogue event**.

Acceptance boundary:

1. Add a focused temporary request source that accepts the exact occurred `DialogueEvent`, the exact
   existing healer identifier, and an explicitly supplied `DialogueActType`; it returns no request
   unless that healer is in the event's exact audience.
2. On success, construct one `DialogueAct` retaining the event's exact utterance and one
   `CharacterTurnRequest` retaining the exact event, act, and healer. Prove prompt-shaped text,
   missing-healer rejection, value-equal identifiers, exact-instance retention, unchanged inputs,
   and unrelated-state preservation in an opt-in Godot smoke.
3. Do not infer or classify an act type from text, install the request in the live turn controller,
   generate or stream a response, append healer transcript/event entries, execute actions or
   proposals, replace the temporary deterministic turn source, add persistence/providers, or alter
   movement and discovery behavior.
