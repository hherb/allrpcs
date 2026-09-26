# Allrpcs Handover

## Current state

`main` includes merged PR #394 at `7b8abee`. Branch
`codex/phase-8-journal-hypotheses-snapshot` completes the scene-neutral safe-journal composition
chain through hypotheses, experiments, and open questions. The Phase 8 journal roadmap item remains
open because scene ownership, ordinary source composition, UI, input, and persistence do not exist.
Draft PR #395 targets `main`.

`JournalHypothesisSectionSnapshotProjection` composes the existing exact hypotheses binding and
authored-copy projection into a fresh immutable section snapshot. `JournalHypothesisSnapshotProjection`
installs only that exact canonical section and exact Seeker into a fresh journal, leaving experiments
and open questions pending.

`JournalExperimentSectionProjection` binds only a post-hypotheses journal whose exact next pending
definition is experiments to one exact existing `ExperimentJournalSnapshot`. Fresh authored copies
retain only the exact experiment identifier, procedure text, and observed-outcome text in stable
recorded order; authoritative `WorldEvent` instances remain behind the binding. The section snapshot
and installation projections retain only the exact section, exact Seeker, and independent read-only
copies before replacing that one pending definition.

`JournalOpenQuestionSectionProjection` performs the corresponding final binding for one exact
`OpenInvestigationQuestionSnapshot`. Fresh authored copies retain only the exact question identifier
and prompt in stable authored order. Its section snapshot and installation projection replace only
the final exact pending definition, producing a fresh complete `JournalSnapshot` with an independent
empty pending-section collection.

All staged projections reject null, rebuilt or foreign sections, distinct value-equal Seeker
identifiers, premature or repeated installation, and stale pending order without changing their
inputs. No added type infers hypothesis confidence or truth, experiment efficacy or causality,
question answers or completion; resolves display names; exposes source aggregates or authoritative
events; owns scene state; presents UI; handles input; persists; mutates gameplay state; or calls a
provider.

## Validation and review

- One hundred nine focused journal tests pass, including forty-three new tests for exact section,
  Seeker, hypothesis, question, and experiment references; stable order; fresh snapshots, journals,
  entries, and independent read-only collections; empty inputs; safe public surfaces; canonical
  staged replacement; rebuilt/value-equal/repeated rejection; null rejection; and unchanged sources.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 795 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed every pure check before the known Godot `user://logs` restriction; the
  identical host run supplied complete Godot evidence.
- Draft PR #395's credential-free CI run `36239372961` at implementation commit `d0f5f66` failed
  all five jobs before any step was instantiated (`steps: []`), so it is Actions/account
  infrastructure and not product-test evidence.
- Session review covered all thirty-four changed files, exact-reference and canonical-order gates,
  safe-copy surfaces, authoritative-event exclusion, source isolation, null and stale paths, public
  API documentation, 120-character limits, sub-500-line C# files, formatting, and whitespace. No
  review issues remain.

## Exact next bounded slice

Add one scene-owned controller for a caller-supplied already-complete safe `JournalSnapshot`. Do not
compose ordinary journal sources or add UI, input, persistence, mutation, or provider behavior yet.

Acceptance boundary:

1. Add one Godot `JournalController` with a nullable exact `JournalSnapshot` and one public
   `TryInitialize` method. Initialization succeeds once only for a complete journal whose testimony,
   hypotheses, experiments, and open questions are installed and whose pending sequence is empty.
2. Retain the exact supplied journal instance. Reject incomplete, repeated, and null initialization
   without replacing current state; null remains an argument error.
3. Add the controller to `Main.tscn` in dependency order while leaving ordinary startup uninitialized
   and inert.
4. Add one switch-gated credential-free smoke that composes a complete empty journal through the
   existing projections, proves exact one-shot ownership, incomplete/repeated/null rejection, closed
   public surface, and unchanged unrelated scene state.
5. Do not add a temporary ordinary journal source, refresh/update transitions, display-name lookup,
   section selection, presentation, UI/input, save schema, authoritative mutation, or provider work.
6. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to compose and initialize a representative journal from existing exact scene sources while
   ordinary startup remains content-neutral and without UI.
