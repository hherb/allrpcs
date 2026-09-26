# Allrpcs Handover

## Current state

`main` includes merged PR #392 at `c43f6fb`. Branch
`codex/phase-8-journal-testimony-provenance` advances the open Phase 8 journal roadmap item through
three bounded, scene-neutral Application slices. The roadmap item remains open because testimony is
not installed into a journal snapshot and hypotheses, experiments, open questions, scene ownership,
UI, input, and persistence do not exist yet. Draft PR #393 targets `main`.

`JournalSnapshotProjection` composes the existing visibility-safe observations snapshot with its
exact catalogue. It retains the exact catalogue, observations snapshot, and Seeker identifier while
copying the catalogue's exact `testimony`, `hypotheses`, `experiments`, and `open-questions`
definitions into an independent read-only pending collection. Only the catalogue's exact
observations definition is accepted; rebuilt value-equal and foreign definitions fail closed.

`JournalTestimonySectionProjection` binds that safe journal to the exact canonical testimony
definition and selects only exact `DialogueEvent` instances whose recorded audience contains the
Seeker. Selection uses identifier value equality, retains stable source order, copies its result,
and exposes it read-only. Unrelated dialogue and the Seeker's own speech remain outside the result;
empty dialogue is valid.

`JournalTestimonyAuthoredCopyProjection` converts the validated heard events into fresh,
audience-free copies containing only the exact speaker identifier, exact utterance text, and exact
provider-neutral delivery mode. It does not expose listener rosters or source events. No added type
installs testimony into the journal, resolves speaker display names, edits or interprets statements,
owns scene state, presents UI, handles input, persists, mutates gameplay, or calls a provider.

## Validation and review

- Forty-nine focused journal tests pass, including thirteen new tests for exact catalogue, section,
  Seeker, dialogue-event, speaker, text, and delivery-mode references; canonical order; value-equal
  listener identity; unrelated and Seeker-spoken exclusion; rebuilt and foreign observation-section
  rejection; empty inputs; independent read-only collections and fresh copies; null rejection;
  audience-free public surface; caller isolation; and unchanged inputs.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 735 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed all pure checks before the known Godot `user://logs` restriction; the
  identical host run supplied complete Godot evidence.
- Session review covered every added file and the README change, exact-reference and value-equality
  boundaries, heard-dialogue filtering, audience exclusion, stable order, caller isolation, public
  API documentation, 120-character limits, sub-500-line C# files, formatting, and whitespace. The
  review clarified foreign-section coverage; no review issues remain.
- Draft PR #393's credential-free CI run `36234627082` at implementation commit `bf89c24` failed all
  five jobs before any step was instantiated (`steps: []`), so it is Actions/account infrastructure
  and not product-test evidence.

## Exact next bounded slice

Compose the safe testimony binding and its audience-free copies into one immutable testimony-section
snapshot. Do not install it into `JournalSnapshot` or add speaker display names, editing, scene
ownership, UI, persistence, or provider behavior yet.

Acceptance boundary:

1. Add one immutable Application testimony-section snapshot retaining the binding's exact canonical
   testimony definition, exact Seeker identifier, and independent read-only authored copies.
2. Add one pure projection from `JournalTestimonySectionBinding` that delegates copy production to
   `JournalTestimonyAuthoredCopyProjection`, returns a fresh snapshot, and does not expose the source
   journal, dialogue events, audiences, or listener identifiers.
3. Add focused tests for exact section, Seeker, speaker, text, and delivery-mode references; stable
   order; fresh snapshots and copies; independent read-only collections; empty testimony; a safe
   public property surface; null rejection; and unchanged source bindings.
4. Do not resolve speaker names or character state, rank, summarize, edit, deduplicate, or interpret
   testimony, replace the pending testimony definition in `JournalSnapshot`, wire a controller or
   scene, add UI/input, persist, mutate authoritative state, or call a provider.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to replace the pending testimony definition with the exact safe testimony snapshot while
   retaining hypotheses, experiments, and open questions as content-free pending sections.
