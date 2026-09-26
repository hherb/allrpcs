# Allrpcs Handover

## Current state

`main` includes merged PR #393 at `e7ef904`. Branch
`codex/phase-8-journal-testimony-snapshot` advances the open Phase 8 journal roadmap item through
four bounded, scene-neutral Application slices. The roadmap item remains open because hypotheses
are not yet composed or installed, and experiments, open questions, scene ownership, UI, input, and
persistence do not exist yet.
Draft PR #394 targets `main`.

`JournalTestimonySectionSnapshotProjection` composes the existing exact testimony binding and
audience-free authored-copy projection into a fresh immutable snapshot. It retains only the exact
canonical testimony definition, exact Seeker identifier, and independent read-only copies in heard
dialogue order. It does not expose the source journal, dialogue events, audiences, or listeners.

`JournalTestimonySnapshotProjection` returns a fresh `JournalSnapshot` retaining the exact
catalogue, observations snapshot, and Seeker while replacing only the exact pending testimony
definition with the exact safe testimony snapshot. Rebuilt definitions, a distinct value-equal
Seeker, duplicate installation, and stale pending order fail closed. Hypotheses, experiments, and
open questions remain exact content-free pending definitions.

`JournalHypothesisSectionProjection` binds only a post-testimony journal whose exact next pending
definition is hypotheses to one exact existing `InvestigationHypothesisSnapshot`. The separate
`JournalHypothesisAuthoredCopyProjection` creates fresh independent read-only copies in stable
authored order. Each copy contains only the exact hypothesis and question identifiers plus exact
authored question and statement text; it exposes neither source aggregate and assigns no status.

No added type ranks, summarizes, edits, deduplicates, interprets, tests, or validates hypothesis
content; installs hypotheses into the journal; resolves display names; owns scene state; presents
UI; handles input; persists; mutates authoritative gameplay state; or calls a provider.

## Validation and review

- Sixty-six focused journal tests pass, including seventeen new tests for exact section, Seeker,
  speaker, text, delivery-mode, hypothesis, and question references; canonical order; staged
  pending-section replacement; rebuilt/value-equal/repeated rejection; fresh snapshots, journals,
  and copies; independent read-only collections; empty inputs; safe public surfaces; null
  rejection; and unchanged sources.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 752 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed all pure checks before the known Godot `user://logs` restriction; the
  identical host run supplied complete Godot evidence.
- Session review covered every changed file, exact-reference and staged-order boundaries, safe
  copy surfaces, caller isolation, public API documentation, 120-character limits, sub-500-line C#
  files, formatting, and whitespace. No review issues remain.
- Draft PR #394's credential-free CI run `36235624399` at implementation commit `db5c5a6` failed
  all five jobs before any step was instantiated (`steps: []`), so it is Actions/account
  infrastructure and not product-test evidence.

## Exact next bounded slice

Compose the exact hypotheses binding and its authored copies into one immutable hypotheses-section
snapshot. Do not install it into `JournalSnapshot` or add hypothesis status, evidence evaluation,
editing, scene ownership, UI, persistence, or provider behavior yet.

Acceptance boundary:

1. Add one immutable Application hypotheses-section snapshot retaining the binding's exact
   canonical hypotheses definition, exact Seeker identifier, and independent read-only authored
   copies.
2. Add one pure projection from `JournalHypothesisSectionBinding` that delegates copy production to
   `JournalHypothesisAuthoredCopyProjection`, returns a fresh snapshot, and does not expose the
   source journal, `InvestigationHypothesisSnapshot`, questions, or hypotheses.
3. Add focused tests for exact section, Seeker, hypothesis identifier, question identifier, prompt,
   and statement references; stable order; fresh snapshots and copies; independent read-only
   collections; empty hypotheses; a safe public property surface; null rejection; and unchanged
   source bindings.
4. Do not infer confidence, support, contradiction, truth, completion, ownership, or visibility;
   rank, summarize, edit, deduplicate, or interpret content; replace the pending hypotheses
   definition in `JournalSnapshot`; wire a controller or scene; add UI/input; persist; mutate
   authoritative state; or call a provider.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to replace the pending hypotheses definition with the exact safe hypotheses snapshot while
   retaining experiments and open questions as content-free pending sections.
