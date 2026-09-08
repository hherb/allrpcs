# Allrpcs Handover

## Current state

`main` contains merged PR #146 at `7b71767`. The branch
`codex/phase-3-runtime-turn-controller` continues Phase 3 through these reviewed slices:

- Phase 3i1 adds a focused Godot `CharacterTurnActivityController` that initializes once from one
  exact immutable activity and delegates awaiting-response and speaking cancellation to Domain.
- Phases 3i2-3i7 add pure exact-request response-reception and speaking-completion policies and
  transitions, then delegate both from the runtime controller. Rejections preserve exact state.
- Phase 3j1 adds a temporary deterministic turn source retaining one exact awaiting activity and
  its already-complete deterministic response without installing or executing that response.
- Phases 3j2-3j4 initialize the scene controller and route the explicit `V` player action to
  cancellation. Fresh-process smokes prove both awaiting and speaking cancellation without
  changing world events/time, world objects, discovery, movement, or existing presentation.

`ROADMAP.md` now marks player interruption/cancellation complete. Response proposals still have no
execution authority. Dialogue submission/display, transcript and event runtime orchestration,
physical-action execution, persistence, providers, streaming, and the Phase 3 acceptance scenario
remain incomplete.

## Validation

Every new contract or runtime seam was added after an expected missing-type, missing-member, or
pre-wiring smoke failure. Focused host-local evidence passes: 1,330 Domain tests and the controller,
temporary-source, awaiting-input, and speaking-input Godot smokes. The complete host-local
`./scripts/quality.sh` run passes workflow and formatting validation, a zero-warning build, 1,330
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 37 opt-in Godot smokes, and the bounded
graphical launch.

The restricted sandbox still reproduces the known VSTest loopback denial and Godot `user://logs`
failure; host-local runs provide executable evidence. The GitHub Actions budget remains exhausted,
so do not manually dispatch hosted workflows and do not describe local results as hosted-platform
proof.

## Exact next bounded slice

Take **Phase 3k1: add the dialogue panel's exact player-utterance submission boundary**.

Acceptance boundary:

1. Add one focused Godot dialogue-input controller and temporary panel that submit the player's
   exact nonblank text as a `DialogueUtterance` for the exact Seeker identifier, delegating existing
   Domain length and validity rules rather than interpreting or normalizing text.
2. Expose only the exact last submitted immutable utterance and prove ordinary prompt-shaped input,
   null/blank/oversized rejection, repeated submission, and unrelated-state preservation in an
   opt-in Godot smoke.
3. Do not yet select an audience, construct a character-turn request, generate or stream a
   response, append transcript/event entries, execute actions or proposals, add persistence or
   providers, or replace the temporary deterministic turn source.
