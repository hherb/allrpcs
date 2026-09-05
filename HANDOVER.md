# Allrpcs Handover

## Current state

Phase 2k46 is implemented on `codex/phase-2k46-controller-perception-initialization`, based
directly on PR #93's verified merge commit `f08f44c`.

`FocusedSearchActivityController` now owns one optional immutable Seeker perception snapshot. Its
new one-time initialization boundary accepts only a snapshot for the controller's value-equal
Seeker identifier, retains the exact supplied snapshot instance, and rejects null,
wrong-character, or repeated initialization without changing activity, outcome, perception, or
presentation state.

During `Main._Ready`, the scene passes the temporary perception source's exact retained snapshot to
the controller once and fails explicitly if the authored scene cannot establish that invariant.
No outcome is resolved, applied, or consumed; visibility and discovery remain unchanged; and no
input or automatic lifecycle behavior was added.

The test-first game build failed with five `CS1061` errors before the controller property and method
existed. After implementation and code review, host-shell `./scripts/quality.sh` passed with a
zero-warning build, 631 Domain tests, 107 Application tests, six architecture tests, all ten Godot
smokes, and the bounded graphical launch. The sandbox-only Godot failure was its known crash while
opening `user://logs`; the identical host-shell smoke passed. No manual visual acceptance was
needed because visible behavior did not change. Hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

Take **Phase 2k47: controller-owned recorded-outcome resolution**.

Acceptance boundary:

1. Extend `FocusedSearchActivityController` with one narrow Boolean boundary accepting an existing
   `FocusedSearchOutcome` for resolution against its initialized perception and recorded outcomes.
2. Require initialized perception and the controller's exact Seeker identifier, then delegate
   complete eligibility and state creation to `FocusedSearchOutcomeResolutionPolicy` and
   `FocusedSearchOutcomeResolutionTransition`. On success, install the exact returned perception
   and outcome snapshot instances while preserving the activity snapshot and presentation.
3. Null, uninitialized, wrong-character, unretained, unequal-result, and unchanged-visibility
   attempts must fail closed while preserving every controller snapshot and presentation.
4. Add focused credential-free smoke coverage for successful replacement, exact unrelated-entry
   retention and stable order, unchanged inputs, every rejection above, and unchanged activity and
   presentation state. Do not connect this boundary to `Main` or ordinary input yet.
5. Document Phase 2k47 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and keep both planning documents pruned to
   current evidence and one exact next slice.

Explicit exclusions:

- Do not add an input, automatic outcome selection or processing, completion-plus-resolution
  shortcut, failure-reason presentation, or final interaction UI.
- Do not produce outcomes, grant discovery, add target-specific rules or visibility ranking, or
  add coordinates, range policy, passive noticing, reinterpretation, authoritative objects or
  characters, persistence, serialization, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
