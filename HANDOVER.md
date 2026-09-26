# Allrpcs Handover

## Current state

`main` includes merged PR #402 at `5f613df`. Branch `codex/phase-8-map-input` advances the
open Phase 8 map roadmap item through two bounded slices: explicit modal map input and bounded
player-note editing over only already-known locations.

Physical M now opens `SeekerMapPanel`; the temporary healer-move command moved from M to H. First
open initializes `SeekerMapController` once from the exact retained identifier of the current scene
Seeker, projects its empty safe state, captures focus inside the panel, and suspends Seeker movement.
Later opens refresh only after the controller's exact map or note source changes; unchanged state
reuses the exact retained presentation. Close or M restores the exact prior focus and
movement-processing state. The map and journal reject opening over each other. Ordinary startup
remains hidden, uninitialized, unfocused, and movement-neutral.

The map panel lists only safe known presentation entries in canonical order. Explicit Save adds or
replaces one nonblank note within the existing 1,000-UTF-16-code-unit bound, and Remove deletes only
the selected location's exact existing note. Accepted changes synchronously refresh the safe text
and retain the selected known location. Empty-map, whitespace, stale, hidden-location, released,
unrelated, repeated, rebuilt-owner, and null paths preserve map, notes, presentation, panel, focus,
movement, journal, and gameplay state.

No automatic testimony reporting, automatic visit capture, coordinate or topology presentation,
map persistence/schema change, provider call, credential access, release, or signing work was
added. The Phase 8 map roadmap item remains open.

## Validation and review

- Test-first work observed each missing input and editing controller before implementation. The new
  switch-gated smokes cover inert startup, exact current-Seeker initialization, physical M and H
  mappings, exact changed-source refresh, retained unchanged reopen, focus and movement ownership,
  toggle and Close paths, journal exclusion, known-only canonical selection, whitespace rejection,
  inclusive maximum-length save, replacement, removal, stale-editor rejection, closed controller
  surfaces, and unrelated gameplay isolation.
- Session review covered every changed production, smoke, scene, input-map, script, README, and
  handover path. It found and fixed one refresh preflight bug: the panel's old exact snapshot must
  be checked against the presentation controller before refreshing, not against the fresh result.
  It also added first-open stale-presentation preflight, explicit null coverage, repeated-close
  coverage, exact note-editor presentation-source provenance, and unambiguous journal short-circuit
  grouping.
- Every changed C# file remains below 500 lines, every changed C# line is at most 120 characters,
  `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed, and `git diff --check`
  passed.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 864 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The first
  focused Godot run in the restricted sandbox hit only the known `user://logs` restriction; the
  identical host run passed.

## Exact next bounded slice

Add one coordinate-free live opening-region visit-verification flow without testimony reporting or
persistence.

Acceptance boundary:

1. Add one scene coordinator that receives the exact current `SeekerMapController` map, exact
   current scene Seeker identifier and position, exact `OpeningRegionProjectionSource` projection,
   and one exact candidate anchor from that projection.
2. Delegate proximity exclusively to `OpeningRegionLocationVisitProjection` with one named positive
   visit radius, then delegate an accepted coordinate-free visit token to
   `SeekerMapController.TryVerifyVisit` with the exact canonical location paired to that anchor.
3. Keep ordinary startup and ordinary movement inert: do not scan anchors, verify locations, open or
   refresh the map, or mutate notes automatically. The switch-gated smoke is the only activation
   path in this slice.
4. Reject null, uninitialized, stale, rebuilt, foreign-owner, foreign-projection, foreign-anchor,
   mismatched-location, non-finite-position, out-of-range, already-verified, and repeated paths
   without changing map, notes, presentation, panel, focus, movement, journal, or gameplay state.
5. Prove reported-to-verified promotion and hidden-to-verified addition in canonical order while
   preserving all prior immutable snapshots and exact source objects.
6. Do not add automatic testimony reporting, continuous visit polling, new player input, coordinate
   or topology presentation, map persistence/schema changes, provider work, or credentials.
7. Run the focused credential-free smoke and `./scripts/quality.sh`, review the complete diff, update
   `README.md`, and leave one next slice for explicit safe testimony-to-location reporting.
