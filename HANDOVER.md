# Allrpcs Handover

## Current state

PR #128 merged Phases 2n9–2n19 into `main` at `381152b`. The continuation branch
`codex/phase-2n20-plus-observation-reinterpretation` implements Phases 2n20–2n25 in commit
`ebc1db4`; draft PR #129 tracks the branch.

The existing Godot controller now owns an initially empty observation-reinterpretation candidate
snapshot and delegates exact authoritative production plus stable atomic resolve-next behavior to
the completed Domain chain. The temporary Seeker retains one exact trigger-knowledge reference; a
separate source retains one candidate for the second authoritative world object. During
`Main._Ready`, the scene resolves that object through live authority, produces the exact candidate,
then resolves it once. Only the stone marker changes from `Noticed` to `Identified`, the candidate is
consumed, and the existing indicator presents its authored description. Passive noticing,
focused-search state, movement, input, and all source snapshots remain unchanged.

Phases 2n20–2n25 were test-first through three new opt-in Godot smokes plus focused extensions to
the character, world-object, and interaction-focus evidence. After code review, host-shell
`./scripts/quality.sh` passed formatting and workflow validation, a zero-warning build, 928 Domain
tests, 107 Application tests, six architecture tests, all 21 Godot smokes, and the bounded graphical
launch. Knowledge-triggered reinterpretation is complete.

## Exact next bounded slice

Take **Phase 2o1: immutable authoritative world-event log snapshot**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.WorldEvents.WorldEventLogSnapshot` containing existing
   `WorldEventId` entries in stable supplied order.
2. Permit an explicitly empty sequence; copy caller input; expose only a read-only list; retain
   every exact supplied identifier instance; and reject null input, null entries, and value-equal
   duplicate event identifiers.
3. Add focused tests first for empty/populated construction, stable order, exact nested-instance
   retention, source-copy isolation, duplicate/null rejection, unchanged inputs, immutability, and
   the minimal public API surface.
4. Update README and ROADMAP evidence, keep event creation/append authority incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not define event payloads, kinds, actors, targets, timestamps, world time, ordering policy,
  append/record transitions, event production, replay, persistence, serialization, or migration.
- Do not connect events to focused search, passive noticing, reinterpretation, movement, dialogue,
  characters, world objects, Godot, Application, providers, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
