# Allrpcs Handover

## Current state

`main` includes merged PR #397 at `4158cd7`. Branch `codex/phase-8-journal-panel`
continues the Phase 8 journal through three bounded slices: safe plain-text projection and a
read-only panel, explicit modal open/close input, and canonical section traversal. The roadmap
journal checkbox remains open because journal persistence and save-schema integration are absent.
Draft PR #398 targets `main`.

`JournalSectionPresentationTextProjection` converts only one already-safe selected-section snapshot
into its exact authored label and stable plain text. Observations render copied names and
descriptions, testimony renders canonical speaker/delivery identifiers without audiences,
hypotheses render authored questions/statements, experiments render procedures/observed outcomes,
and open questions render prompts. Every empty arm uses the same explicit neutral state.

`JournalSectionPanel` starts hidden and empty. It accepts only the exact snapshot currently retained
by `JournalSectionPresentationController`, preserves rendered state for stale, rebuilt, foreign,
repeated, and null rejection, and replaces content only after an explicit exact refresh. The panel
is centred within the supported 960x540 viewport and exposes no complete journal, catalogue,
authoritative source, dialogue audience, or world event.

The mapped J action explicitly composes the representative journal on first open, selects and
presents the first exact canonical section, captures panel focus, stops Seeker movement, and restores
the exact prior physics-processing state on close. Previous/Next buttons traverse all five existing
definitions in stable order with closed ends. Close or J hides the panel, releases the actual GUI
focus owner, and retains the exact immutable presentation for reopening. Ordinary startup remains
inert.

No automatic source refresh, display-name lookup, editable note, map, journal persistence or save
schema, gameplay mutation, authoritative source exposure, model invocation, provider call, or
credential access was added.

## Validation and review

- Test-first panel and input smoke builds observed the missing scene owners before their
  implementations were added.
- All 11 focused Application text-projection cases passed every non-empty content arm, every neutral
  empty arm, null rejection, and the closed plain-text surface.
- The two switch-gated journal smokes passed hidden inert startup, exact presentation ownership,
  stable text for all five sections, explicit refreshed replacement, rejection preservation,
  J/Close opening and closing, focus capture/release, exact movement restoration, canonical
  closed-end traversal, retained reopening, and journal/source/gameplay/unrelated-UI isolation.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 813 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.
- The restricted quality run passed every pure check before the known Godot `user://logs`
  restriction; the identical host run supplied complete Godot evidence.
- Draft PR #398's credential-free CI run `36243534269` at implementation commit `6f3b48d` failed
  all five jobs before any step was instantiated (`steps: []`), so it is Actions/account
  infrastructure and not product-test evidence.
- Session review covered all new Application values/tests, panel/input controllers and smokes,
  `Main.tscn`, input mapping, smoke registration, README, and this handover. It fixed the initial
  off-viewport panel placement, clipped long content, release of only the Close button rather than
  the actual focus owner, a stale selection/presentation opening edge, rejected-state smoke
  assertions, and the existing exact command-overlay expectation. Exact ownership, fail-closed
  replacement, stable content, modal restoration, inert startup, documentation, formatting,
  whitespace, 120-character limits, and sub-500-line C# files were checked; no review issue remains.

## Exact next bounded slice

Add one pure journal persistence-capture boundary without changing the existing root save document
or restoring journal state yet.

Acceptance boundary:

1. Define one immutable content-safe journal save-state value for a complete `JournalSnapshot` plus
   the optional exact selected canonical section key.
2. Capture all five safe authored section arms in stable order without retaining the complete
   catalogue, perception/world bindings, dialogue audiences, authoritative experiment events,
   investigation aggregates, controller instances, or UI state.
3. Require a complete exact journal and, when supplied, an exact canonical selection from that
   journal. Reject incomplete, foreign, rebuilt, and null inputs without partial output.
4. Prove fresh independent immutable copies, stable order, the explicit no-selection state, and
   closed persistence surfaces with focused Application tests.
5. Do not change the current save root/version, JSON serialization, restoration controllers, scene
   wiring, journal source refresh, panel behavior, editable notes, map, gameplay, or providers.
6. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for integrating the captured journal state into a new versioned save root and canonical JSON.
