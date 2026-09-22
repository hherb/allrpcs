# Allrpcs Handover

## Current state

`main` includes merged PR #316 at `f35611c`. Branch
`codex/phase-7-sabotage-action-observation` advances Phase 7 through two bounded slices:

- Phase 7on adds `CharacterSabotageActionSelectionCoordinator`. A caller must explicitly observe
  one exact completed sabotage-action selection or abstention, and duplicate observation is inert.
  Only the same exact currently observed task result can be acknowledged; acknowledgement resets
  the task and clears the observation.
- Phase 7oo dependency-orders that coordinator after the existing sabotage-action task and explicit
  flow in `Main.tscn`. Ordinary startup remains inert. Only the existing switch-gated,
  credential-free smoke explicitly observes and acknowledges one exact selected result before
  verifying a later offline failure remains unexposed.

The chain still only selects one caller-prefiltered sabotage kind or abstains. It does not author
the request from current action-specific eligibility, choose a target or candidate, dispatch or
apply an action, poll or acknowledge automatically, schedule work, author evidence, persist state,
or add input/UI, timing, navigation, movement, perception, dialogue, or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused runs failed only on the absent selection coordinator and then the absent
  `Main.tscn` coordinator node before their implementations were added.
- Focused host-local runs pass a zero-warning game build, the isolated exact selection-coordinator
  smoke, and the switch-gated main-scene flow.
- The sandboxed full quality run reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passes
  workflow validation, restore, formatting, a zero-warning build, 2,728 Domain tests, 586
  Application tests, 1,220 Infrastructure tests, all 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covers the coordinator's explicit-only surface, exact selected and abstaining
  result retention, duplicate observation, stale acknowledgement, exact reset-and-clear behavior,
  idle/running/cancelled/failed/null inertness, unchanged request graph, main-scene dependency
  order, inert startup, documentation, file and line limits, whitespace, and scoped paths. No
  unresolved finding remains.

## Exact next bounded slice

Take **Phase 7op: author one concealment-gated sabotage-action request**.

Acceptance boundary:

1. Test-first add one isolated scene controller that receives the exact current enemy actor and the
   existing exact concealment source/state graph, then exposes a fresh
   `CharacterSabotageActionConsiderationRequest` only when the existing concealment eligibility
   policy accepts that graph.
2. Retain the exact actor and only the canonical `CharacterSabotageActionKind.Conceal` singleton in
   the authored request. Reuse the existing physical access, world objects, concealment, evidence,
   and candidate instances for eligibility; do not rebuild value-equal substitutes or infer access.
3. Keep unavailable, stale, value-equal replacement, ineligible, already-authored, and null inputs
   inert. Prove one exact eligible request plus every fail-closed path in one isolated
   credential-free smoke, including unchanged source and authoritative state.
4. Do not yet wire the request author into `Main.tscn`, start the sabotage-action model/task,
   observe or acknowledge a result, add the other six action kinds, dispatch or apply concealment,
   author evidence, persist state, or add input/UI, timing, navigation, movement, perception,
   dialogue, or other world behavior.
5. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to add one further action-specific eligibility gate without weakening this exact boundary.
