# Allrpcs Handover

## Current state

Phase 2l14 is implemented on `codex/phase-2l14-world-object-gated-focused-search`, based
directly on PR #110's merge commit `f9fa86e`.

`InteractionFocusController.CreateFocusedSearchRequest` now receives an existing nullable
`WorldObject`. It rejects null, absent-focus, and target-mismatched objects, accepts exact or
distinct value-equal identifiers, and retains the successful object's exact identifier instance
with the controller-owned Seeker identifier. `TryStartFocusedSearch` delegates that same object and
invokes the existing condition-aware start path only for a created request.

The temporary F path resolves only the current focused identifier through
`WorldObjectController.FindWorldObject` and passes its exact result into the start boundary. A
missing object reports `UNCHANGED` without invoking the start path. Focus selection and
presentation, context aggregates, focused-search snapshots, world-object source and controller
state, target state, perception, and input instances otherwise remain unchanged.

Credential-free smoke evidence covers null, absent-focus, mismatched, exact, and distinct
value-equal objects, successful delegation for both visible targets, exact identifier retention,
and missing-object state identity. After code review, host-shell `./scripts/quality.sh` passed with
a zero-warning build, 661 Domain tests, 107 Application tests, six architecture tests, all thirteen
Godot smokes, and the bounded graphical launch. The sandbox attempt reached the build but VSTest
could not bind its local socket; no product code was changed for that environment restriction.
Authored appearance did not change, so no manual visual inspection was needed.

Temporary sources, controllers, lifecycle controls, greybox focus, and identified-marker
presentation remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l15: world-object-gated temporary interaction-focus availability**.

Acceptance boundary:

1. Make `InteractionFocusController.RefreshFocus` consider an otherwise visible, enabled, in-range
   greybox target eligible only when its identifier resolves through `WorldObjectController` to an
   existing matching object. Use identifier equality only; do not inspect identity text or
   descriptions.
2. Retain the selected world object's exact identifier instance as `FocusedTargetObjectId`. When
   the nearest marker's object is absent, select the next eligible in-range marker in existing
   authored-order/distance semantics; when none exist, clear focus, its indicator, and status.
3. Preserve the pure selector, range and tie-breaking rules, world-object snapshots and objects,
   all focused-search snapshots, context and perception, and every supplied nested instance. Extend
   credential-free evidence for exact and distinct value-equal objects, missing-nearest fallback,
   all-missing clearing, restoration, and unchanged state outside focus presentation.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain request, discovery-condition, perception, outcome, completion, or resolution
  contracts; Application focus selection; focus range or ordering; requirement semantics;
  world-object snapshots or lookup; or resolved-perception presentation rules.
- Do not filter or rank world objects beyond existing focus eligibility, make temporary sources or
  controllers authoritative, or add discovery grants, knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
