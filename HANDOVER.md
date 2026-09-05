# Allrpcs Handover

## Current state

Phase 2k47 is implemented on `codex/phase-2k47-controller-outcome-resolution`, based directly on
PR #94's verified merge commit `3105cc4`.

Slice commit `91883e9` is pushed, and draft PR #95
(`https://github.com/hherb/allrpcs/pull/95`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivityController.TryResolveOutcome` now accepts one existing
`FocusedSearchOutcome` only when controller-owned Seeker perception is initialized, the outcome
belongs to the controller's value-equal Seeker identifier, and the existing pure resolution policy
accepts it. The controller delegates state creation to the existing resolution transition and
installs its exact returned perception and outcome snapshots. Activity and presentation state are
untouched.

The dedicated credential-free Godot smoke proves successful resolution of a value-equal recorded
outcome; target-only perception replacement; exact unrelated perception and outcome entry
retention in stable order; unchanged input snapshots; and unchanged activity, indicator, and result
text. Null, uninitialized, wrong-character, unretained, unequal-result, and unchanged-visibility
attempts fail closed without changing any controller snapshot or presentation.

Test-first compilation failed only at the four missing `TryResolveOutcome` call sites. After the
small controller implementation and code review, host-shell `./scripts/quality.sh` passed with a
zero-warning build, 631 Domain tests, 107 Application tests, six architecture tests, all eleven
Godot smokes, and the bounded graphical launch. The sandbox-only Godot failure was its known crash
while opening `user://logs`; the identical host-shell smoke passed. No manual visual acceptance was
needed because visible behavior did not change. Hosted desktop outputs remain unsigned development
evidence, not releases.

No input or `Main` wiring was added. Outcomes are still resolved only through the new explicit
controller method; ordinary play records an `Identified` outcome on E but does not apply or consume
it.

## Exact next bounded slice

Take **Phase 2k48: temporary recorded-outcome resolution input**.

Acceptance boundary:

1. Add one temporary physical R action for resolution and expose it in the existing TEMP control
   text.
2. On R, select the first retained outcome for the controller's Seeker in stable recorded order,
   pass that exact outcome to `TryResolveOutcome`, and present `RESOLVE: CHANGED` or
   `RESOLVE: UNCHANGED` through the existing result label. With no matching outcome, fail closed
   without changing controller state.
3. Extend the credential-free interaction smoke to prove F then E records an outcome, R changes
   only the matching perception entry and consumes that outcome, unrelated entries retain exact
   instances and order, the completed activity stays absent, and a repeated R is unchanged.
4. Prove the R mapping is exact and that released, unrelated, movement, and healer inputs preserve
   perception as well as activity, outcomes, and prior result text.
5. Document Phase 2k48 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and keep both planning documents pruned to
   current evidence and one exact next slice.

Explicit exclusions:

- Do not combine completion and resolution, automatically process outcomes, choose by current
  focus, produce new outcomes, grant discovery, or add failure-reason presentation or final
  interaction UI.
- Do not add target-specific rules, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, authoritative objects or characters, persistence, serialization,
  providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
