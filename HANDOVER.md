# Allrpcs Handover

## Current state

`main` includes merged PR #401 at `cc25d93`. Branch `codex/phase-8-map-controller` advances
the open Phase 8 map roadmap item through four bounded slices: exact scene ownership, safe
presentation projection, explicit presentation refresh, and a read-only panel.

`SeekerMapController` owns one fresh exact opening-region catalogue, current
`SeekerMapSnapshot`, and current `SeekerMapNoteSnapshot`. Ordinary startup leaves all three null.
Explicit initialization creates an empty map and empty notes atomically for one exact Seeker.
Testimony reporting, coordinate-free visit verification, note set or replacement, and note removal
delegate to the existing pure boundaries and replace only exact expected current state. Null,
uninitialized, stale, rebuilt, foreign, cross-owner, repeated, hidden-location, and failed
downstream paths do not partially mutate state.

`SeekerMapPresentationProjection` exposes only known locations in canonical order with their
reported or verified status and optional bounded note text. It does not expose the hidden catalogue,
coordinates, anchors, or topology. Its plain-text projection has one neutral empty state.
`SeekerMapPresentationController` projects or refreshes only the map controller's exact current
map and notes. `SeekerMapPanel` starts hidden and empty, then renders only that controller's exact
safe presentation through explicit calls; it has no ordinary input path yet.

No automatic testimony refresh or visit capture, map input action, editable note widget,
persistence/schema change, gameplay mutation, provider call, credential access, release, or signing
work was added. The Phase 8 map roadmap item remains open.

## Validation and review

- Test-first work observed each missing controller, presentation, text, and panel boundary before
  implementation. The focused map suite passes 38 tests covering safe projection, canonical order,
  status, notes, neutral empty text, immutability, closed surfaces, and null, hidden, foreign,
  cross-owner, stale, rebuilt, repeated, and invalid rejection.
- Three switch-gated main-scene smokes pass exact controller mutation, presentation refresh, and
  hidden read-only panel rendering while ordinary startup and unrelated state remain inert.
- Session review covered every production, test, scene, script, and documentation path; exact
  reference authorization; atomic replacement; safe public data; canonical ordering; immutable
  prior values; ordinary-startup inertness; sub-500-line C# files; 120-character lines; formatting;
  and whitespace. The only focused-test correction was an assertion that initially expected caller
  order instead of the existing canonical map order.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 864 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The first
  focused Godot run in the restricted sandbox hit only the known `user://logs` restriction; the
  identical host run passed.

## Exact next bounded slice

Add one explicit map open/close input boundary without note editing or persistence.

Acceptance boundary:

1. Add one `map_toggle` action mapped to physical M plus one map-panel Close control. Ordinary
   startup remains hidden, uninitialized, unfocused, and movement-neutral.
2. On first explicit open, initialize `SeekerMapController` once from the exact current scene
   Seeker, project its empty current state, and present it. Never initialize from rebuilt identity.
3. On later explicit opens, refresh only when the presentation controller's exact source map or
   notes changed; otherwise reopen the exact retained presentation.
4. Capture focus inside the panel, suspend Seeker movement, prevent overlap with the journal panel,
   and restore the exact prior movement-processing and focus state on Close or M.
5. Reject released, unrelated, stale, rebuilt, unavailable, repeated, and null paths without
   changing map, notes, presentation, panel, focus, movement, journal, or gameplay state.
6. Do not add automatic testimony reporting, automatic visit capture, note editing, coordinate or
   topology presentation, persistence/schema changes, gameplay mutation, providers, or credentials.
7. Prove the exact input flow with a credential-free switch-gated smoke, run
   `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one next slice for
   explicit bounded note editing over already-known locations.
