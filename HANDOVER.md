# Allrpcs Handover

## Current state

`main` contains merged PR #153 at `dc67a98`. The branch
`codex/phase-4-context-assembler` advances Phase 4 through Phase 4b7:

- Phase 4b4 projects only one exact character's authored goal descriptions into fresh `Goal`
  entries, retaining exact string instances and stable authored order without exposing goal keys or
  any other character field.
- Phase 4b5 makes the assembler derive those goals for the exact responding character, rejects a
  mismatched character, and rejects caller-supplied `Goal` entries.
- Phase 4b6 projects exact utterance text only from supplied recent dialogue the character spoke or
  was recorded as hearing. Unrelated dialogue is omitted without changing the source events.
- Phase 4b7 makes the assembler derive recent speech after goals and before the exact untrusted
  player dialogue, and rejects caller-supplied `RecentSpeech` entries.

The Phase 4 context-assembler roadmap item remains open. Filtered perception, relevant knowledge,
memories, and valid affordances are still caller-supplied typed entries; no provider adapter, call,
deadline executor, retry, credential, provider logging, structured-output validation, Godot wiring,
partial intention execution, persistence, or gameplay/world-state mutation has been added.

## Validation

Each production boundary began with an expected focused compile failure. Focused host-local tests
for goal projection, recent-speech projection, and assembler ownership pass. The complete
host-local `./scripts/quality.sh` passes workflow and formatting validation, a zero-warning build,
1,381 Domain tests, 207 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 51 opt-in Godot smokes, and the bounded
graphical launch.

The restricted sandbox reproduces the known .NET IPC/VSTest denial; identical host-local commands
pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4b8: project the currently proven identified perception boundary into model context**.

Acceptance boundary:

1. Add one pure Application projection from an exact responding `Character`, its exact
   `CharacterPerceptionSnapshot`, and an authoritative `WorldObjectSnapshot` to fresh
   `FilteredPerception` entries.
2. Require the perception snapshot's character identifier to match the exact character. In stable
   perception order, project only entries at exactly `VisibilityState.Identified` whose exact
   value-equal world object exists and has non-empty authored description text.
3. Retain the exact `WorldObject.Description.Text` string without formatting or disclosing object
   identifiers/display names. Fail closed on a missing identified object; prove exact text-instance
   retention, value-equal lookup, order, exclusions for every other visibility singleton, empty
   descriptions, nulls, mismatches, unchanged inputs, and no hidden-object leakage.
4. Do not yet combine perception with the assembler, infer semantics for later visibility states,
   project knowledge/memory/affordances, add storage or conversation-window policy, call a provider,
   wire Godot, validate/execute intentions, add retries/timeouts/credentials/logging, persist, or
   mutate gameplay state.
