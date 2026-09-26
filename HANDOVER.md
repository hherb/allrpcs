# Allrpcs Handover

## Current state

`main` includes merged PR #391 at `84a3fcb`. Branch
`codex/phase-8-journal-observation-entries` advances the open Phase 8 journal roadmap item through
three bounded, scene-neutral Application slices. The roadmap item remains open because testimony,
hypotheses, experiments, open questions, scene ownership, UI, input, and persistence do not exist
yet. Draft PR #392 targets `main`.

The existing exact filtered-perception and authoritative-world binding now projects to fresh
structural `JournalObservationEntry` values in perception order. Each entry retains one exact
`WorldObjectVisibility` and its exact value-equal matching authoritative `WorldObject`. The
projection returns an independent read-only collection, accepts empty perception, leaves unrelated
world objects outside the result, and fails explicitly if validated world coverage is ever broken.

`JournalObservationAuthoredCopyProjection` applies the established non-ranking visibility boundary:
only the exact `Identified` singleton yields authored copy. Each fresh copy retains the filtered
visibility's exact object identifier and the matching object's exact display-name and description
strings without exposing authoritative world state. Every other visibility state remains absent,
including later states that this slice deliberately does not reinterpret.

`JournalObservationSectionSnapshotProjection` composes those two projections into a fresh immutable
consumer-safe observations-section snapshot. It exposes only the exact canonical observations
definition, exact Seeker identifier, and independent read-only authored copies. No added type owns
editable journal state, a controller or scene, UI/input, persistence, gameplay mutation, or provider
behavior.

## Validation and review

- Thirty-six focused journal tests pass, including thirteen new tests for stable perception order,
  exact nested references and copied text, value-equal identifier resolution, unrelated-world
  exclusion, every admissible non-identified visibility state, empty descriptions and collections,
  read-only results, safe public snapshot surface, null rejection, and unchanged source bindings.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 722 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed all pure checks before the known Godot `user://logs` restriction; the
  identical host run supplied complete Godot evidence.
- Session review covered the complete diff, exact-reference and value-equality boundaries,
  authoritative-world coverage, hidden-text exclusion, non-ranking visibility behavior, stable
  order, caller isolation, public API documentation, 120-character limits, sub-500-line C# files,
  formatting, and whitespace. Review replaced one null-forgiving invariant with an explicit failure
  and expanded safety coverage to every admissible non-identified state; no review issues remain.
- Draft PR #392's credential-free CI run `36231764074` at implementation commit `a7c1122` failed all
  five jobs before any step was instantiated (`steps: []`), so it is Actions/account infrastructure
  and not product-test evidence.

## Exact next bounded slice

Compose the safe observations snapshot with its exact canonical catalogue while retaining the four
remaining section definitions as content-free pending sections. Do not add testimony or other
section content, editable state, scene ownership, UI, persistence, or provider behavior yet.

Acceptance boundary:

1. Add one immutable Application journal snapshot retaining one exact `JournalSectionCatalogue`,
   exact `JournalObservationSectionSnapshot`, exact Seeker identifier, and the catalogue's exact
   `testimony`, `hypotheses`, `experiments`, and `open-questions` definitions in canonical order as
   pending sections.
2. Add one pure composition that accepts only an observation snapshot whose section is the exact
   catalogue observations definition, retains exact inputs, copies the pending-section result, and
   exposes it read-only. Rebuilt value-equal section definitions fail closed.
3. Add focused tests for exact catalogue, section, Seeker, and pending-definition references;
   canonical order; independent read-only pending collections; empty observations; rebuilt and
   foreign observation-section rejection; null rejection; and unchanged inputs.
4. Do not invent entries for the pending sections, derive testimony or investigation content, add
   notes or editing, wire a controller or scene, add UI/input, serialize state, mutate authoritative
   gameplay state, or call a provider.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to bind safe heard dialogue provenance to the canonical testimony section without UI or
   persistence.
