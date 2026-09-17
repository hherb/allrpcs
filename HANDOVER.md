# Allrpcs Handover

## Current state

`main` includes merged PR #297 at `1db4fb8`. Branch
`codex/phase-7-influence-recovery-consideration` advances the exact influence-recovery scene chain
through four bounded slices:

- Phase 7lv adds `CharacterTargetSabotageInfluenceRecoveryCandidateRequestController`, which
  retains one request only from the candidate source's exact current candidate and derives the
  exact relationship-owning target from the retained influence occurrence.
- Phase 7lw dependency-orders that request source after its candidate in `Main.tscn`; only the
  switch-gated influence smoke creates the exact candidate and request.
- Phase 7lx adds
  `CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController`, which starts
  only the request source's exact current request while the recovery task is idle and otherwise
  preserves the queued model.
- Phase 7ly dependency-orders that flow with the existing recovery model/task/state/resolution
  owners. Ordinary startup stays inert; the switch-gated smoke alone initializes recovery
  authority and explicitly proves one exact influence-to-recovery chain.

The branch changes only the two bounded controllers, their isolated smokes, the existing
switch-gated main-scene influence smoke, main-scene dependency order, smoke script, README,
ROADMAP, and this handover. It does not autonomously start recovery, author inverse evidence or
changes, persist relationships, or add UI/input, timing, navigation, movement, perception,
dialogue, or other world behavior. The unrelated untracked `scripts/code_statistics.py` in the
original checkout remains untouched.

## Validation and review

- Test-first focused builds failed only on each absent controller, and the main-scene checks failed
  only on each absent dependency-ordered node before the smallest implementations were added.
- Focused zero-warning builds, both isolated controller smokes, and the switch-gated main-scene
  influence-to-recovery smoke pass host-locally.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 1,028 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact request/candidate/result/relationship/character retention,
  policy delegation, one-shot ownership, queued-model preservation, task/status gating, inert
  startup, main-scene dependency order, unchanged source graphs, closed public surfaces,
  documentation, file size, line length, formatting, and whitespace. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lz: coordinate one explicit recovery-consideration start from exact current influence
state**.

Acceptance boundary:

1. Test-first add one isolated scene controller that accepts the influence controller's exact
   current applied result plus one caller-authored inverse relationship change and composes only the
   existing recovery candidate, request, authority, task, model-source, and exact-request flow.
2. Before consuming a model or publishing state, require the exact influence result/relationship,
   an accepted prospective recovery graph, unused candidate/request sources, uninitialized recovery
   authority, and an idle recovery task; then create the exact candidate and request, initialize
   recovery authority from that exact relationship, and start only that exact request once.
3. Reject unavailable, stale, value-equal rebuilt, foreign, misdirected, wrong-dimension,
   wrong-delta, null, repeated, or already-occupied input without consuming the queued model or
   changing the influence result, relationship, candidate, request, change, reason, occurrence, or
   character graph.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   this coordinator into `Main.tscn`, trigger recovery autonomously, infer or author inverse
   evidence, persist relationships, or add UI/input, timing, navigation, movement, perception,
   dialogue, or other world behavior.
