# Allrpcs Handover

## Current state

Phase 2k9 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k9-focused-search-completion-transition`, based directly on the PR #56 merge at
`3170987`. Feature commit `dda0bf2` is pushed, and draft PR #57
(`https://github.com/hherb/allrpcs/pull/57`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchCompletionTransition.CompleteSearch` delegates eligibility to
`FocusedSearchCompletionPolicy`, rejects an ineligible request, and otherwise returns a new
snapshot without the activity for the complete value-equal request. It retains every nonmatching
activity instance in stable order and never mutates the request, activities, or input snapshot.
Completion remains separate from cancellation even though both transitions currently have the
same request-removal shape.

The test-first build failed only because `FocusedSearchCompletionTransition` did not exist. All 12
focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 473
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. No manual play acceptance is required for this engine-independent Domain slice.
`Domain -> []` remains intact, and hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k10: pure per-character focused-search
activity projection**.

Acceptance boundary:

1. Add a static `FocusedSearchActivityProjection` in `Allrpcs.Domain.Discovery` with one pure
   `ForCharacter(CharacterId, FocusedSearchActivitySnapshot)` operation returning a new
   `FocusedSearchActivitySnapshot`.
2. Include exactly the activities whose request has a value-equal searcher character identifier;
   permit no matches and all valid matching requests rather than assuming snapshots were created
   only through the start transition.
3. Retain matching existing activity instances in stable order, ignore nonmatching activities,
   reject null inputs, and never mutate the identifier, existing activities, or input snapshot.
4. Add focused tests for no, one, and multiple matches; value equality; stable order and exact
   instance retention; unrelated entries; null rejection; non-mutation; and the deliberately
   limited static public surface.
5. Preserve `Domain -> []`; document the pure projection in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add presentation text, status values, progress, duration, timing, outcomes, events,
  effects, reasons, discovery grants, visibility changes, or in-place collection mutation.
- Do not change snapshot construction, start eligibility, or the existing start, cancellation,
  and completion transitions; the projection must tolerate multiple distinct requests for one
  character because the snapshot itself does.
- Do not define target exclusivity, range, radius, location, coordinates, movement, animation,
  search-condition evaluation, discovery grants, visibility changes, or presentation policy.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, Godot scene, or UI state.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, new discovery
  requirement types, scenario content, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
