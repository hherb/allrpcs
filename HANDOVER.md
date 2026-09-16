# Allrpcs Handover

## Current state

PR #285 is merged into `main` at `4494409`. Branch
`codex/phase-7-influence-selection` advances Phase 7 through four bounded Domain-only slices:

- Phase 7ki selects the exact first currently eligible influence candidate in stable caller order
  only after one-pass validation of the complete, at-most-64 candidate collection. Null and
  duplicate exact candidate references are rejected; distinct structurally equivalent candidates
  remain valid.
- Phase 7kj retains the exact actor, interaction-access snapshot, relationship, and a stable bounded
  copy of candidates in one immutable provider-neutral consideration request.
- Phase 7kk admits only an exact-actor abstention or exact currently eligible request-member
  selection, then binds the accepted decision to its exact request and selected candidate.
- Phase 7kl leaves accepted abstention inert and delegates an accepted selection through the
  existing immutable influence transition to fresh relationship state.

Implementation commit `c3294f3` is published in draft PR #286 targeting `main`.

The branch changes only Domain contracts, Domain tests, README, ROADMAP, and this handover. It adds
no Application, Infrastructure, model, binding, task, scene, input, UI, provider, persistence,
evidence, recovery-selection, elapsed-time, delay, navigation, movement, world, perception, or
dialogue behavior. Ordinary startup remains unchanged. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused host runs pass all 22 new selection, request, decision/result, and transition
  tests. The combined influence suite passes all 42 tests.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,633 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. The sandbox run reached the same zero-warning build and stopped
  only at the known VSTest loopback `SocketException (13): Permission denied` restriction.
- Session-wide review verified complete-validation-before-selection, exact-reference duplicate and
  membership checks, stable immutable retention, exact actor identity, current eligibility,
  abstention inertness, and delegation only through the existing influence transition. No
  unresolved finding remains.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, `git diff --check`, and the
  120-column check pass. Every new file is below 500 lines.
- Draft PR #286 run `35152526872` failed all five jobs immediately with `steps: []`; no hosted
  product or packaging step ran. Treat this as Actions/account infrastructure and do not retry an
  unchanged run.

## Exact next bounded slice

Take **Phase 7km: obtain and validate one provider-neutral influence decision**.

Acceptance boundary:

1. Test-first add the smallest Application interface whose `DecideAsync` accepts one exact
   `CharacterTargetSabotageInfluenceConsiderationRequest` plus caller cancellation and returns one
   `CharacterTargetSabotageInfluenceDecision`.
2. Add one pure-orchestration Application controller that rejects nulls and pre-cancellation before
   model invocation, invokes the supplied model exactly once with the exact request and token, and
   constructs `CharacterTargetSabotageInfluenceConsiderationResult` so invalid decisions fail
   closed through the existing Domain policy.
3. Prove exact selection and abstention, exact request/token forwarding, invalid-decision rejection,
   cancellation/exception propagation, unchanged request state, and reflection-locked interface and
   controller surfaces.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add a
   model binding/source, background task owner, OpenAI transport/decoder, scene/controller wiring,
   input/UI, provider settings, persistence, evidence, recovery selection, elapsed-time/delay, or
   other world behavior.
