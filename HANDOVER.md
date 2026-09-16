# Allrpcs Handover

## Current state

PR #282 is merged into `main` at `76203eb`. Branch
`codex/phase-7-misquotation-live-exposure-flow` advances Phase 7 through two bounded integration
slices using only the existing main-scene owners:

- Phase 7jz carries the exact completed live-candidate decision through one-time exposure, exact
  witness-view refresh, and explicit result acknowledgement. Repeated observation and premature or
  repeated acknowledgement fail closed; the task returns to idle while live evidence remains.
- Phase 7ka explicitly reviews, selects, renders, and dismisses that exact live evidence through the
  existing truth-neutral UI. The distinct preauthored candidate is rejected, and dismissal retains
  every live occurrence, candidate, request, exposure, view, and dialogue event.

Implementation commit `782d7ec` is published in draft PR #283 targeting `main`.

Only `TemporaryCharacterDialogueMisquotationOccurrenceMainSceneSmokeCheck` changes runtime test
coverage; no production API or scene composition changes. Ordinary startup remains inert. Nothing
automatically starts an occurrence, candidate, request, decision, exposure, review, selection, or
presentation. No dialogue is corrected or redelivered; no transcript, evidence, character, world,
perception, navigation, movement, provider, timing, retry, or persistence behavior changes. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- The rebuilt dedicated main-scene smoke passes the exact live occurrence-to-dismissal chain and
  all retained-state assertions. Its first sandbox attempt hit the known Godot `user://logs` crash;
  the identical host run passed.
- `dotnet build Allrpcs.sln --no-restore --disable-build-servers --maxcpucount:1
  --verbosity:minimal` passes with zero warnings and zero errors.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,562 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. The sandbox-only VSTest `SocketException (13)` was retried
  unchanged on the host.
- Session review found and fixed one smoke-only failure-reporting issue by stopping before UI
  assertions when the required live exposure/view is absent. Formatter verification,
  `git diff --check`, and the 120-column check pass; no unresolved review finding remains.
- Draft PR #283 run `35094911277` failed all five jobs immediately with `steps: []`; no hosted
  product or packaging step ran. Treat this as Actions/account infrastructure and do not retry an
  unchanged run.

## Exact next bounded slice

Take **Phase 7kb: define character-targeted physical and informational access without yet defining
a delay or influence effect**.

Acceptance boundary:

1. Test-first add the smallest Domain-only immutable snapshot/requirement/policy seam analogous to
   object access: one exact actor, a bounded defensive stable set of value-unique target-character
   identifiers currently reachable through ordinary interaction, one exact target character, one
   canonical capability key, and one exact required knowledge identifier.
2. The pure policy accepts only the value-matching actor and target when the interaction snapshot,
   authored capability, and retained knowledge all satisfy the unchanged requirement. Null,
   self-target, missing, mismatched, duplicate, excessive, and distinct unsupported inputs fail
   closed without mutating any input. Add reflection-backed closed-surface tests.
3. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add a
   delay/influence proposal, action selection, occurrence, elapsed-time state, relationship change,
   dialogue delivery, provider, Application, Infrastructure, scene, input, UI, persistence,
   navigation, movement, or world/perception mutation.
