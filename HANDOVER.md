# Allrpcs Handover

## Current state

`main` includes merged PR #322 at `97780ba`. Branch
`codex/phase-7-sabotage-action-specific-composition` advances Phase 7 through three bounded slices:

- Phase 7pd composes only the acknowledged generic selection's exact strongly typed candidate into
  its existing action-specific provider-neutral request after revalidating the corresponding live
  source, authoritative snapshots, and unchanged policy.
- Phase 7pe starts only that exact composed request once. All seven typed branches revalidate current
  authority before consuming the matching action-specific model, so rejected preflight leaves a
  queued model untouched.
- Phase 7pf observes only the exact terminal action-specific result for that started request and
  explicitly acknowledges it through the branch's existing presentation, exposure, or resolution
  coordinator.

The new seams preserve compile-time types for conceal, move, substitute, contaminate, misquote,
delay, and influence. They use no reflection or untyped candidate casts. Delay and influence authors
retain their exact upstream bounded request internally for later snapshot revalidation. Null,
unavailable, pre-acknowledgement, stale, rebuilt, replaced, foreign, wrong-kind, repeated,
ineligible, running, failed, cancelled, and externally reset states remain inert.

Ordinary startup remains inert. The switch-gated main-scene smoke exercises one live conceal path
through generic selection, binding and acknowledgement, exact concealment-request composition, an
explicit abstaining action-specific model, typed result observation, and acknowledgement. It proves
the whole new chain without applying sabotage or changing authoritative concealment/evidence state.

No slice adds autonomous scheduling, input/UI, persistence, provider transport, credentials,
evidence authorship, recovery selection, navigation, movement, perception, dialogue delivery, or
new Domain effect logic. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- All three slices were test-first: the game build failed in turn on each absent coordinator type
  before its production seam was added.
- The focused switch-gated main-scene smoke passes host-local after the sandbox's known Godot
  `user://logs` denial, including null, pre-acknowledgement, foreign, repeated, exact-start,
  abstention, exact-result, acknowledgement, inert-startup, and unchanged-authority assertions.
- The final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build,
  2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covered all seven typed composition/start/result branches, exact candidate and
  request provenance, action-specific current-authority checks before model consumption, terminal
  result and acknowledgement lifecycle, abstention, repeated/foreign/null failure, ordinary-startup
  inertness, dependency order, unchanged authority, closed typed surfaces, documentation,
  file/line limits, and whitespace. No additional defect remained after the lifecycle smokes.

## Exact next bounded slice

Take **Phase 7pg: bind one acknowledged selected action-specific result to its exact current
authoritative outcome without scheduling recovery**.

Acceptance boundary:

1. Test-first add one explicit coordinator that accepts only Phase 7pf's retained exact typed result
   and acknowledged generic author request. Abstention remains inert and exposes no outcome.
2. For each of the seven typed selected branches, revalidate the exact selected candidate, exact
   action-specific request, and the existing branch owner's exact current post-action authority or
   presentation/resolution token. Retain only typed exact outcome provenance; do not rebuild values.
3. Stale, replaced, foreign, wrong-kind, null, repeated, unacknowledged, abstaining, and
   non-current-authority paths must fail closed without starting or consuming any model.
4. Dependency-order the coordinator in `Main.tscn`. Extend the switch-gated smoke with one selected
   concealment path only if it can prove the existing exact application and presentation without
   weakening the ordinary-startup or recoverability boundaries; otherwise add an isolated exact
   selected-result smoke and keep the main smoke abstaining.
5. Do not start recovery consideration, remove evidence, conceal truth, author new evidence,
   schedule another action, persist state, or add input/UI, provider, timing, navigation, movement,
   perception, dialogue delivery, or other world behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to route the retained exact outcome into an existing recovery/evidence seam only after a
   separately bounded eligibility gate.
