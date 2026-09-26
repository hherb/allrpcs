# Allrpcs Handover

## Current state

`main` includes merged PR #395 at `13997f1`. Branch
`codex/phase-8-journal-controller` continues the Phase 8 journal work through exact scene ownership
and one explicit representative composition from existing scene sources. The roadmap journal item
remains open because section selection, presentation, input, refresh, and persistence do not exist.
Draft PR #396 targets `main`.

`JournalController` starts with a null journal and retains one caller-supplied exact
`JournalSnapshot` only when testimony, hypotheses, experiments, and open questions are installed and
the pending-section sequence is empty. Incomplete, repeated, replacement, and null initialization
cannot replace scene state. `Main.tscn` owns the controller in dependency order but ordinary startup
never initializes it.

`RepresentativeJournalCompositionCoordinator` is an explicitly invoked, provider-neutral scene
boundary. It reads the current exact filtered Seeker perception, initialized world objects, latest
dialogue event when present, first-patient investigation plans and confirmed conclusions, and the
universal experiment journal. It runs the existing safe section projections in canonical order,
keeps confirmed questions out of open questions, and initializes `JournalController` once. Missing,
unsafe, or repeated composition fails closed without changing any source or presentation state.

The installed journal contains only safe authored copies: identified observation text, audience-free
heard testimony, authored hypothesis text, experiment procedure and observed-outcome text without
authoritative events, and unconfirmed authored questions. It does not retain perception, world
objects, dialogue events or audiences, investigation aggregates, authoritative experiment events,
or confirmed conclusions through its public surface.

Ordinary startup remains content-neutral and UI-free. No new facts, dialogue, hypotheses,
experiments, or questions are authored; no journal action is mapped to input; and no display-name
lookup, section selection, presentation, refresh/update transition, save schema, authoritative
mutation, model invocation, provider call, or credential access was added.

## Validation and review

- The focused game build passed with zero warnings, and both switch-gated journal smokes passed:
  exact complete one-shot ownership plus explicit representative composition from live scene sources.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 795 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.
- The restricted quality run passed every pure check before the known Godot `user://logs`
  restriction; the identical host run supplied complete Godot evidence.
- Draft PR #396's credential-free CI run `36240638678` at implementation commit `0746a29` failed
  all five jobs before any step was instantiated (`steps: []`), so it is Actions/account
  infrastructure and not product-test evidence.
- Session review covered all new controllers and smokes, `Main.tscn`, `scripts/smoke-godot.sh`,
  `README.md`, and this handover. Exact ownership, canonical order, safe-copy surfaces, source
  isolation, null/incomplete/repeated paths, inert startup, public API documentation, 120-character
  limits, sub-500-line C# files, formatting, and whitespace were checked. No review issue remains.
- The Phase 8 journal roadmap checkbox remains open because this branch adds safe scene state and
  composition only, not a player-usable journal.

## Exact next bounded slice

Add one scene-owned exact journal-section selection boundary without presenting section content yet.

Acceptance boundary:

1. Add one `JournalSectionSelectionController` whose selection is initially null and whose only
   selection operation requires the exact journal retained by `JournalController` plus one exact
   canonical `JournalSectionDefinition` from that journal's catalogue.
2. Permit explicit replacement with another exact canonical section while rejecting an
   uninitialized journal, stale or foreign journals, rebuilt or foreign value-equal sections,
   repeated selection, and null without changing the current selection.
3. Wire the controller into `Main.tscn` in dependency order while leaving ordinary startup with no
   journal and no selected section.
4. Add one switch-gated credential-free smoke that explicitly composes the representative journal,
   selects exact sections, proves replacement and all rejection paths, checks a closed public
   surface, and preserves journal/source/UI state.
5. Do not add a section content union or view model, label/panel presentation, journal input action,
   automatic opening, refresh/update transition, display-name lookup, editable notes, persistence or
   save schema, gameplay mutation, authoritative source exposure, or provider work.
6. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to project the selected exact section into a content-safe presentation value without UI.
