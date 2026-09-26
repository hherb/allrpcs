# Allrpcs Handover

## Current state

`main` includes merged PR #396 at `9d7e025`. Branch
`codex/phase-8-journal-selection` continues the Phase 8 journal item through exact section
selection plus content-safe UI-free projection and explicit refresh. The roadmap journal checkbox
remains open because no journal panel, input/opening flow, or persistence exists.

`JournalSectionSelectionController` starts null and accepts only the exact journal retained by
`JournalController` plus one exact canonical definition from that journal. It permits explicit
replacement by a different exact canonical section while uninitialized, stale, foreign, rebuilt,
value-equal, repeated, and null inputs cannot change the current selection.

`JournalSectionPresentationProjection` accepts only a complete exact journal and one of its exact
canonical sections. Its immutable `JournalSectionPresentationSnapshot` exposes that definition and
exactly one already-safe observation, testimony, hypothesis, experiment, or open-question snapshot.
It does not expose the complete journal, catalogue, perception, world objects, dialogue audiences,
investigation aggregates, authoritative experiment events, or confirmed conclusions.

`JournalSectionPresentationController` starts null and explicitly owns one projection of the current
exact selection. A later exact selection replacement remains stale until `TryRefresh` is explicitly
called; unchanged, unprojected, repeated, or invalid refreshes preserve the prior immutable value.
`Main.tscn` owns both new controllers in dependency order. Ordinary startup still does not compose a
journal, select a section, project content, or change any existing UI or source state.

No journal label or panel, input action, automatic opening, display-name lookup, editable note,
map, source-content refresh, persistence or save schema, gameplay mutation, authoritative source
exposure, model invocation, provider call, or credential access was added.

## Validation and review

- Test-first checks observed the missing selection controller, presentation value/projection, and
  refresh operation before their implementations were added.
- Focused Application tests cover all five safe content arms plus incomplete, foreign, rebuilt, and
  null rejection and the closed immutable public surface. All 7 focused cases passed.
- The three switch-gated Godot smokes passed exact selection/replacement, safe one-arm projection,
  explicit refresh, immutable prior values, every rejection path, closed controller surfaces, inert
  startup, and journal/source/UI isolation.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 802 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.
- The restricted quality run passed every pure check before the known Godot `user://logs`
  restriction; the identical host run supplied complete Godot evidence.
- Session review covered all new Application values and tests, scene controllers and smokes,
  `Main.tscn`, `scripts/smoke-godot.sh`, `README.md`, and this handover. Exact ownership,
  fail-closed replacement, immutable safe surfaces, source isolation, null and repeated paths,
  inert startup, documentation, formatting, whitespace, 120-character limits, and sub-500-line C#
  files were checked. A reflective content-arm invariant was replaced with direct typed checks; no
  review issue remains.

## Exact next bounded slice

Add one read-only journal-section panel that explicitly renders the current exact content-safe
presentation snapshot, without adding an opening input flow yet.

Acceptance boundary:

1. Add one hidden-by-default panel whose explicit presentation operation requires the exact snapshot
   currently retained by `JournalSectionPresentationController`.
2. Render the exact canonical section label and stable safe content for all five arms: observation
   names/descriptions, audience-free testimony, authored hypothesis question/statements, experiment
   procedure/observed outcomes, and open-question prompts. Present an explicit neutral empty state.
3. Permit explicit replacement only after the presentation controller has explicitly refreshed to a
   different exact snapshot. Reject stale, rebuilt, foreign, repeated, and null values without
   changing the currently rendered panel.
4. Wire the panel into `Main.tscn` without automatic composition, selection, projection, visibility,
   focus capture, movement suspension, or input mapping.
5. Add one switch-gated credential-free smoke that explicitly composes the representative journal,
   visits all five exact sections through selection/projection/refresh, verifies safe stable text and
   replacement/rejection behavior, and preserves journal, source, gameplay, and unrelated UI state.
6. Do not add a journal open/close action, automatic refresh from changed sources, display-name
   lookup, editable notes, the map, persistence/save schema, gameplay mutation, authoritative source
   exposure, or provider work.
7. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for explicit journal open/close input with focus and movement suspension.
