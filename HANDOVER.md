# Allrpcs Handover

## Current state

Phase 2k11 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k11-focused-search-activity-indicator`, based directly on the PR #58 merge at
`bc86b47`. Feature commit `8426c65` is pushed, and draft PR #59
(`https://github.com/hherb/allrpcs/pull/59`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivityIndicatorPolicy.ShouldShow` is the single pure Application-layer operation.
It delegates per-character selection to `FocusedSearchActivityProjection` and returns true exactly
when the new projected snapshot contains at least one activity, including multiple distinct
matching requests. It returns false for empty and unrelated snapshots, matches character
identifiers by value through the projection, rejects null inputs, and never mutates its inputs.

The test-first build failed only because the Application discovery namespace and policy did not
exist. All seven focused tests then passed on the host. The sandboxed runner built successfully but
could not start VSTest because local loopback binding was denied. Host-shell
`./scripts/quality.sh` passed with 480 Domain tests, 107 Application tests, six architecture tests,
four Godot smokes, and the bounded graphical launch. No manual play acceptance is required for
this engine-independent policy slice. `Application -> Domain` and `Domain -> []` remain intact;
hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k12: focused-search activity indicator
presentation adapter**.

Acceptance boundary:

1. Add one small Godot-side `FocusedSearchActivityIndicator` adapter and one obvious temporary
   in-world indicator authored under the existing Seeker.
2. Give the adapter one presentation operation accepting an existing `CharacterId` and
   `FocusedSearchActivitySnapshot`; delegate the decision to
   `FocusedSearchActivityIndicatorPolicy.ShouldShow` and change only the indicator's visibility.
3. Keep the indicator hidden until explicitly presented. Do not construct or inject a fake active
   search during ordinary play, and do not add a production source of focused-search activity in
   this adapter slice.
4. Add a credential-free Godot smoke proving hidden-by-default behavior; empty and unrelated
   snapshots remain hidden; one and multiple matching requests show exactly the one indicator;
   subsequent empty presentation hides it again; and supplied identifiers, activities, and
   snapshots are not mutated.
5. Preserve `Godot -> Application -> Domain` and `Domain -> []`; document the temporary adapter in
   `README.md`, record Phase 2k12 in `ROADMAP.md`, leave the broader bounded-focused-search item
   incomplete, and replace this file with current evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice. Treat headless scene-state checks separately from native visual
   acceptance; claim the latter only if it is actually performed.

Explicit exclusions:

- Do not add display text, status values, player input, animation, progress, duration, timing,
  outcomes, events, effects, reasons, or final UI/art.
- Do not create, start, cancel, complete, schedule, or otherwise transition focused searches; do
  not change existing Domain snapshots, policies, transitions, or the Application indicator
  policy.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, location, coordinates, range, radius, interaction focus, or other UI
  state.
- Do not add search-condition evaluation, discovery grants, visibility changes, passive noticing,
  reinterpretation, configurable Boolean expressions, scenario content, serialization,
  persistence, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
