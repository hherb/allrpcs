# Allrpcs Handover

## Current state

`main` includes merged PR #351 at `08dcc26`. Branch
`codex/phase-7-fifth-move-start` advances the exact fifth Move cycle through two bounded seams:
one explicit generic-request start and one exact completed-result observation. The reviewed
implementation is committed at `689d89b` and published in draft PR #352 targeting `main`.

The new path:

- starts only the exact current fifth-cycle generic Move request on a fresh scene-owned
  `CharacterSabotageActionTaskController` with a fresh
  `TemporaryCharacterSabotageActionModelSource`;
- revalidates the complete successor admission, author, candidate, continuation recovery,
  placement, evidence, and empty-turn graph before consuming the one-shot model;
- preserves that queued model across rejected preflight calls; and
- observes only the exact completed task-installed result for the exact started request and
  canonical `Move` selection.

Null, guarded-author-output, rebuilt-request/result, foreign-actor, wrong-kind, running,
stale-graph, replaced-authority, and repeated inputs remain inert. The result is not acknowledged,
the completed generic task is not reset, and no candidate is bound. No typed Move request is
composed or started; no action is dispatched or applied; the ordinary turn remains empty; and
placement, evidence, earlier model-call counts, and all retained provenance remain unchanged. No
cadence, provider, credential, persistence, UI/input, navigation, perception, dialogue, timer, or
new Domain behavior was added. Ordinary startup remains inert. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both seams were introduced test-first; each focused game build failed with `CS0246` for its
  deliberately absent coordinator before implementation.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including the exact fifth-cycle start and completed-result observation while authoritative
  placement/evidence and all five earlier model-call counts remain unchanged.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- Session-wide review found the focused Move smoke's success text still described the pre-start
  boundary. It now reports the exact fifth-cycle start and result observation; the focused proof
  passes after the fix.
- Complete-diff review found no remaining correctness, exact-reference, scene-ordering,
  public-surface, formatting, line-length, or documentation issues. Whitespace validation is clean.
- Draft PR #352's five hosted jobs completed as failures with `steps: []`; no hosted product step
  executed, so this is Actions/account infrastructure state rather than contradictory product-test
  evidence.

## Exact next bounded slice

Bind only the exact observed fifth-cycle generic Move result to the exact candidate retained by its
guarded author, stopping before acknowledgement or task reset.

Acceptance boundary:

1. Test-first add one distinct result-binding coordinator over only the exact current observed
   result, fifth-cycle composition, author candidate binding, completed task, and retained graph.
2. Accept only the exact observed result and exact author-owned binding for the source-owned fifth
   Move candidate, canonical `Move` kind, and exact author request.
3. Reject null, rebuilt-result, foreign-actor, wrong-kind, wrong-candidate, stale-graph,
   replaced-authority, and repeated inputs without changing any owner.
4. Retain the exact result and candidate binding once while leaving the observation present and the
   generic task completed with its exact result.
5. Do not acknowledge or reset the result, compose or start typed Move work, dispatch/apply an
   action, reserve the ordinary turn, clear prior provenance, or alter placement/evidence authority.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to acknowledge only that exact bound result while retaining its provenance for typed Move
   composition.
