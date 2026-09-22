# Allrpcs Handover

## Current state

`main` includes merged PR #318 at `2d3dc1a`. Branch
`codex/phase-7-sabotage-action-eligibility-gates` advances Phase 7 through six bounded
request-authoring slices after the merged concealment gate:

- Phase 7oq adds an isolated exact-state `Move` request author.
- Phase 7or adds an isolated exact-state `Substitute` request author.
- Phase 7os adds an isolated exact-state `Contaminate` request author.
- Phase 7ot adds an isolated exact-source `Misquote` request author.
- Phase 7ou adds an isolated exact-current-state `Delay` request author.
- Phase 7ov adds an isolated exact-current-state `Influence` request author.

Each controller is one-shot, retains the exact enemy actor and only its one canonical action-kind
singleton, requires exact source/request and current-state references, and delegates final
eligibility to the existing action-specific Domain policy. Unavailable, ineligible, stale,
distinct value-equal replacement, null, and repeated inputs fail closed where applicable. The
authors remain isolated from `Main.tscn`; they do not combine kinds, start a model or task, observe
or acknowledge results, dispatch or apply actions, author new evidence, or mutate source or
authoritative state.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

Implementation commit `23c572a` is published in draft PR #319 targeting `main`.

## Validation and review

- Every slice was test-first: the focused game build failed only for its missing request-controller
  type, then passed with zero warnings after the implementation.
- Each focused Godot smoke first encountered the known sandbox-only `user://logs` crash, then the
  identical host-local smoke passed.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` passes.
- The sandboxed full quality run reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passes
  restore, formatting, a zero-warning build, 2,728 Domain tests, 586 Application tests, 1,220
  Infrastructure tests, all 8 architecture tests, scenario validation, every credential-free
  Godot smoke, and the Compatibility renderer launch.
- Session-wide review covered exact source/request provenance, current-state authority, existing
  policy delegation, canonical one-kind output, stable failure, one-shot behavior, unchanged
  inputs and state, closed public surfaces, isolation from `Main.tscn`, documentation, file and
  line limits, and whitespace. Tautological unchanged-source smoke assertions were replaced with
  comparisons against retained pre-call references; no unresolved finding remains.
- Draft PR #319 run `35738560598` reports all five hosted jobs failed with `steps: []`; no checkout,
  build, test, smoke, or export step ran, so this is Actions/account infrastructure state rather
  than hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7ow: compose one exact eligible sabotage-action request**.

Acceptance boundary:

1. Test-first add one isolated scene composer that reads only the exact current outputs of the
   seven action-specific request authors and creates at most one fresh
   `CharacterSabotageActionConsiderationRequest` for their same exact enemy actor.
2. Include only currently authored eligible kinds, once each, in canonical roadmap order:
   `Conceal`, `Move`, `Substitute`, `Contaminate`, `Misquote`, `Delay`, `Influence`. Retain the exact
   actor and canonical singleton instances; do not rerun or weaken any action-specific gate.
3. Keep no-eligible-kind, foreign actor, distinct value-equal actor/request, wrong-kind,
   duplicate-kind, stale, null, and repeated composition inert. Prove full and partial eligible
   sets plus unchanged author outputs and authoritative state in one isolated credential-free
   smoke.
4. Do not yet wire these request authors or the composer into `Main.tscn`, start the sabotage-action
   model/task, observe or acknowledge a result, dispatch or apply a selected action, select an
   action-specific candidate, author evidence, persist state, or add input/UI, timing, navigation,
   movement, perception, dialogue delivery, or other world behavior.
5. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to connect the exact composed request to the existing explicit consideration flow without
   dispatching or applying the selected action.
