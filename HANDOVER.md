# Allrpcs Handover

## Current state

`main` includes merged PR #350 at `fe672dc`. Branch
`codex/phase-7-fifth-move-author` continues its exact successor-cycle boundary and implements the
next two bounded seams: one fifth-cycle Move author invocation and one generic request composition.
The reviewed implementation is committed at `eebf435` and published in draft PR #351 targeting
`main`.

The new path:

- adds one source-owned fifth Move candidate whose evidence identifier is distinct from all four
  earlier focused-proof Move candidates;
- accepts that candidate only as the exact caller-supplied reference after revalidating the complete
  successor admission, empty ordinary-turn, continuation recovery, placement, and evidence graph;
- invokes a fresh `CharacterSabotageActionMoveRequestController` once and retains its exact typed
  candidate binding; and
- composes only that exact guarded author output into one fresh generic request containing the exact
  actor and canonical `Move` kind.

Null, rebuilt, malformed, foreign, wrong-kind, prior-cycle, duplicate-evidence, stale-authority, and
repeated inputs remain inert. No consideration is started, no model is supplied or consumed, the
ordinary turn remains empty, and authoritative placement/evidence remain unchanged. No cadence,
provider, credential, persistence, UI/input, navigation, perception, dialogue, timer, or new Domain
behavior was added. Ordinary startup remains inert. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both seams were introduced test-first; each focused game build failed with `CS0246` for its
  deliberately absent coordinator before implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  fifth-cycle exact authoring and composition while authoritative placement/evidence and all five
  existing model-call counts remain unchanged.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Session-wide review found one stale bounded-surface expectation in the temporary Move source smoke
  after the fifth candidate was added. It now verifies that candidate's exact retention, distinct
  evidence, eligibility after all four earlier evidence entries, and closed surface; the focused
  smoke and full quality suite pass after the fix.
- Complete-diff review found no remaining correctness, exact-reference, scene-ordering,
  public-surface, formatting, line-length, or documentation issues. Whitespace validation is clean.
- Draft PR #351's four completed hosted jobs failed with `steps: []`, while its remaining queued job
  also exposed `steps: []`; no hosted product step executed, so this is Actions/account
  infrastructure state rather than contradictory product-test evidence.

## Exact next bounded slice

Explicitly start only the exact fifth-cycle generic Move request on a fresh scene-owned task with a
fresh one-shot model source, stopping before result observation.

Acceptance boundary:

1. Test-first add one distinct start coordinator over only the exact current fifth-cycle composition
   request and its complete retained admission/author/candidate/recovery/empty-turn graph.
2. Add a fresh `CharacterSabotageActionTaskController` and fresh
   `TemporaryCharacterSabotageActionModelSource`; do not reuse or reset an earlier task or source.
3. Preserve the queued model across null, guarded-author-output, rebuilt-request, foreign-actor,
   wrong-kind, stale-graph, replaced-authority, and repeated rejected preflight calls.
4. On the one successful explicit start, retain the exact generic request and consume the queued
   model once; keep all earlier model-call counts at one and the new model-call count at one.
5. Do not observe, bind, acknowledge, select, dispatch, or apply the result; do not start typed Move
   work, reserve the ordinary turn, clear prior provenance, or alter placement/evidence authority.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to observe only the exact completed fifth-cycle generic result without acknowledging it.
