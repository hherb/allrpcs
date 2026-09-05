# Allrpcs Handover

## Current state

Phase 2k30 is implemented and reviewed without findings on
`codex/phase-2k30-outcome-change`, based directly on PR #77's merge commit `5a7b16b`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeChangePolicy.WouldChange` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot`. It
delegates applicability to `FocusedSearchOutcomeApplicabilityPolicy`, then delegates exact target
selection to `CharacterPerceptionLookup` and returns true only when the retained target's current
visibility state differs from the outcome's resulting state. It rejects null inputs, treats every
distinct state pair alike, and leaves all inputs and retained instances unchanged. It does not
rank, apply, produce, or consume outcomes or connect them to focused-search completion.

The nine focused tests failed first with `CS0103`/`CS0246` while the policy type was absent. The
sandboxed green rerun compiled but stopped at the known VSTest loopback restriction; the identical
host-shell focused test passed. Host-shell `./scripts/quality.sh` passed with a zero-warning build,
542 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and the
bounded graphical launch. No runtime Godot behavior changed, so no separate manual visual
acceptance was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k31: focused-search outcome perception transition**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicationTransition.Apply` operation accepting
   an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot` and returning a
   `CharacterPerceptionSnapshot`.
2. Delegate the complete decision to `FocusedSearchOutcomeChangePolicy`; throw
   `InvalidOperationException` when the outcome is inapplicable or would not change the target.
3. For an accepted outcome, return a new snapshot retaining the exact existing character
   identifier, stable entry order, and every unrelated entry instance; replace only the exact
   target with a new `WorldObjectVisibility` retaining its exact object identifier and the
   outcome's exact resulting visibility-state instance.
4. Add focused Domain tests for rejected wrong-character, absent-target, and equal-state cases;
   every distinct state pair; target positions among unrelated entries; value-equal distinct
   identifiers; exact instance retention; unchanged success and failure inputs; null rejection
   through the policy; and the intended single-operation pure static API surface.
5. Do not rank visibility states, decide advancement or regression, mutate perception, consume or
   produce outcomes, or connect the transition to focused-search completion.
6. Document Phase 2k31 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add outcome production or lifecycle application, discovery grants, advancement policy,
  events, effects, reason/error taxonomies, progress, duration, automatic lifecycle behavior,
  authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
