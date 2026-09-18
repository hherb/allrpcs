# Allrpcs Handover

## Current state

`main` includes merged PR #305 at `7ceb984`. Branch `codex/phase-7-delay-recovery`
continues the character-targeted delay path through three bounded pure Domain slices:

- Phase 7mw adds `CharacterTargetSabotageDelayRecoveryCandidate` and its policy. One exact applied
  delay result is recoverable only while its exact event-log and qualitative-time snapshots remain
  current, its exact authoritative advancing delay event remains both the log tail and current-time
  event, and the exact caller-authored recovery event remains authoritative, unrecorded, and
  advancing.
- Phase 7mx adds `CharacterTargetSabotageDelayRecoveryTransition` and its immutable result. Recovery
  delegates to the existing world-event occurrence transition, appends recovery after the complete
  delay history, and returns fresh log/time snapshots without deleting or rewriting delay.
- Phase 7my–7mz add stable first-eligible selection and one provider-neutral consideration request.
  Both validate a complete one-pass collection of at most 64 candidates, reject null and duplicate
  exact references, and retain exact caller order; the request defensively retains the exact actor,
  authoritative event definitions, event log, world time, and candidates without selecting or
  applying them.

Implementation commit `f0d6b59` is published in draft PR #306 targeting `main`.

No slice decides recovery, applies a consideration result, constructs or schedules requests,
invokes a model, owns background work, wires recovery into the scene, authors evidence, persists
state, or adds input/UI, navigation, movement, perception, dialogue, or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Each slice began with a focused compile/test failure for its missing contract. The final focused
  delay-recovery suite passes all 23 tests.
- The sandbox reproduced the known VSTest loopback `SocketException (13)`; identical host-local
  focused runs passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,710 Domain tests, 569 Application tests, 1,091 Infrastructure tests, all 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the final Compatibility launch.
- Session-wide review covers exact applied-result/current-state identity, exact authoritative event
  identity and time effects, log-tail/current-time alignment, one-shot event recordability, immutable
  append-only recovery, stable complete bounded selection, defensive request retention, nulls,
  duplicate exact references, public surfaces, docs, file size, line length, formatting, whitespace,
  and scoped paths. One fail-closed finding was fixed by requiring the delay result's exact original
  event to remain an authoritative advancing definition; its regression test is included. No
  unresolved finding remains.
- Draft PR #306 is open, draft, and mergeable. Its five hosted jobs failed immediately with empty
  step lists, so they are Actions/account infrastructure failures rather than hosted product-test
  evidence; the complete host-local quality result above remains the executable validation.

## Exact next bounded slice

Take **Phase 7na: define one provider-neutral target-delay recovery decision**.

Acceptance boundary:

1. Test-first add the smallest immutable decision and consideration-result vocabulary for either
   one exact request-member recovery candidate or explicit abstention, retaining the exact deciding
   actor identifier, request, decision, and optional selection.
2. Accept only the request actor's exact identifier instance. For a selection, require the exact
   request-member candidate, the candidate delay occurrence's exact target identifier to be the
   request actor, and current eligibility against the request's unchanged authoritative event, log,
   and time state. Reject value-equal rebuilt actors/candidates and stale or ineligible state.
3. Keep accepted abstention referentially inert and leave every request source unchanged. Do not yet
   apply the consideration result, define a model interface/controller, bind a provider, own or
   schedule background work, construct requests in the scene, author evidence, persist state, or add
   input/UI, navigation, movement, perception, dialogue, or other world behavior.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff.
