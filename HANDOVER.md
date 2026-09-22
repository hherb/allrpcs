# Allrpcs Handover

## Current state

`main` includes merged PR #317 at `b30383c`. Branch
`codex/phase-7-sabotage-action-concealment-request` advances Phase 7 through one bounded slice:

- Phase 7op adds `CharacterSabotageActionConcealmentRequestController` as an isolated request
  author. It accepts only the existing temporary source's exact enemy actor, physical-access
  snapshot, world-object snapshot, and concealment candidate together with the concealment
  controller's exact current concealment and evidence snapshots. The existing
  `CharacterObjectSabotageConcealmentPolicy` must still accept that complete graph.
- One accepted graph produces one fresh `CharacterSabotageActionConsiderationRequest` retaining
  the exact actor and only the canonical `CharacterSabotageActionKind.Conceal` singleton.
  Unavailable, stale, distinct value-equal replacement, ineligible, null, and repeated inputs are
  inert. Source and authoritative state remain unchanged.

The request author remains isolated and is not wired into `Main.tscn`. It does not combine action
kinds, start a model or task, observe or acknowledge a result, dispatch or apply concealment,
author evidence, persist state, or add input/UI, timing, navigation, movement, perception,
dialogue, or other world behavior. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

Implementation commit `16bea91` is published in draft PR #318 targeting `main`.

## Validation and review

- The test-first game build failed only because
  `CharacterSabotageActionConcealmentRequestController` did not exist, then passed with zero
  warnings after implementation.
- The focused Godot smoke passes outside the sandbox. Its first sandboxed launch hit the known
  `user://logs` crash; the identical host-local launch passed.
- The sandboxed full quality run reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passes
  workflow validation, restore, formatting, a zero-warning build, 2,728 Domain tests, 586
  Application tests, 1,220 Infrastructure tests, all 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covers exact source and current-state authority, existing-policy delegation,
  canonical one-kind request output, unavailable/ineligible/stale/replacement/null/repeated
  rejection, unchanged source and state, closed public surface, scene dependency order,
  documentation, file and line limits, whitespace, and scoped paths. One README wording blemish was
  fixed; no unresolved finding remains.
- Draft PR #318 run `35734033946` reports all five hosted jobs failed with `steps: []`; no checkout,
  build, test, smoke, or export step ran, so this is Actions/account infrastructure state rather
  than hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7oq: author one move-gated sabotage-action request**.

Acceptance boundary:

1. Test-first add one isolated scene controller that receives the exact current enemy actor and
   the existing exact move source/state graph, then exposes a fresh
   `CharacterSabotageActionConsiderationRequest` only when the existing
   `CharacterObjectSabotageMovePolicy` accepts that graph.
2. Retain the exact actor and only the canonical `CharacterSabotageActionKind.Move` singleton in
   the authored request. Reuse the existing physical access, world objects, placements, evidence,
   destination, and candidate instances for eligibility; do not rebuild value-equal substitutes
   or infer access.
3. Keep unavailable, stale, value-equal replacement, ineligible, already-authored, and null inputs
   inert. Prove one exact eligible request plus every fail-closed path in one isolated
   credential-free smoke, including unchanged source and authoritative state.
4. Do not yet wire either request author into `Main.tscn`, combine conceal and move into one
   request, start the sabotage-action model/task, observe or acknowledge a result, add the other
   five action kinds, dispatch or apply movement, author evidence, persist state, or add input/UI,
   timing, navigation, perception, dialogue, or other world behavior.
5. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to add one further action-specific eligibility gate without weakening either exact
   boundary.
