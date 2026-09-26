# Allrpcs Handover

## Current state

`main` includes merged PR #390 at `dc932b1`. Branch
`codex/phase-8-journal-foundation` advances the open Phase 8 journal roadmap item through three
bounded, asset-neutral Application slices. The roadmap item remains open because no journal entries,
scene ownership, UI, input, or persistence exists yet. Draft PR #391 targets `main`.

Application now defines one immutable five-section catalogue containing `observations`, `testimony`,
`hypotheses`, `experiments`, and `open-questions` in stable canonical order. Definitions retain exact
bounded authored keys and display labels. The catalogue copies caller input, exposes it read-only,
retains exact definitions, and rejects null, empty, incomplete, duplicate, reordered, or unknown
layouts. The representative source returns independent fresh catalogues.

`JournalObservationSectionProjection` binds the catalogue's exact observations definition to one
exact Seeker identifier and exact already-filtered perception snapshot. Empty perception is valid;
null, value-equal rebuilt, other-character, or still-`Unnoticed` inputs fail closed. The result retains
the exact snapshot and its stable exact visibility entries without creating journal content.

`JournalObservationWorldProjection` then binds that exact observation provenance to one exact
authoritative world-object snapshot only when every visible perception target resolves by identifier.
Distinct value-equal identifiers are valid, unrelated authoritative objects remain retained, and a
missing target rejects the complete binding without changing either input. No slice interprets a
visibility state, creates observation text or entries, owns scene state, mutates gameplay, presents
UI, handles input, persists, or calls a provider.

## Validation and review

- Twenty-three focused journal tests cover exact authored values and instances, canonical order,
  independent sources, caller isolation, read-only exposure, empty inputs, exact Seeker identity,
  hidden-entry rejection, value-equal world lookup, unrelated world retention, missing targets, and
  every null, malformed, duplicate, reordered, unknown, or overlong path.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 709 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.
- Session review covered the complete diff, immutable and exact-reference boundaries, hidden-state
  filtering, authoritative world coverage, caller isolation, stable order, public API documentation,
  120-character limits, sub-500-line C# files, formatting, and whitespace. No review issues remain.
- Draft PR #391's credential-free CI run `36230445214` at implementation commit `ad2b4a2` failed all
  five jobs before any step was instantiated (`steps: []`), so it is Actions/account infrastructure
  and not product-test evidence.

## Exact next bounded slice

Project the observation-world binding into structural, content-free observation entries. Do not add
text rendering, editable notes, scene ownership, UI, persistence, or provider behavior yet.

Acceptance boundary:

1. Add one immutable Application observation entry retaining one exact filtered
   `WorldObjectVisibility` and its exact matching authoritative `WorldObject`.
2. Add one pure projection from `JournalObservationWorldBinding` that returns entries in perception
   order, resolves identifiers by value, retains exact visibility and world-object instances, copies
   its result, and exposes it read-only. Empty perception returns an empty read-only result.
3. Add focused tests for stable order, exact references, distinct value-equal identifiers, unrelated
   authoritative objects, independent returned collections, empty input, null rejection, and
   unchanged source bindings.
4. Do not derive display names or descriptions, rank or reinterpret visibility, create editable or
   persisted journal state, wire a controller or scene, add UI/input, mutate perception or world
   objects, or call a provider.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice to project safe authored observation copy from the structural entries without UI or
   persistence.
