# Allrpcs Handover

## Current state

Phase 2k49 is implemented on `codex/phase-2k49-temporary-resolved-perception-presentation`, based
directly on PR #96's verified merge commit `9799296`.

Each visible herb-patch and stone-marker target now owns the same narrow temporary presentation
adapter. `Main` passes only the target's exact retained `ObjectId` and the controller's current
Seeker perception. The adapter delegates exact target selection to `CharacterPerceptionLookup` and
shows its small authored `IDENTIFIED` label only for the exact `VisibilityState.Identified`
singleton. It fails closed for null, absent, unrelated, `Noticed`, and every other state without
ranking or exposing perception entries.

Presentation initializes immediately after the existing one-time controller-perception
initialization and refreshes only after successful R resolution. Failed and repeated resolution,
F/C/E lifecycle input, movement, healer input, and interaction-focus changes do not refresh or alter
the markers. Resolution selection and controller snapshot ownership remain unchanged and
focus-independent.

The test-first build failed because the presentation adapter did not exist. After implementation
and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 631 Domain
tests, 107 Application tests, six architecture tests, all eleven Godot smokes, and the bounded
graphical launch. The interaction smoke covers both complete F/E/R target sequences, exact unrelated
state retention, null and non-identified hiding, successful target-only reveal, and failed/repeated
R preservation. The sandbox-only quality attempt reached the known VSTest loopback denial; the
identical host-shell gate passed. No separate manual visual inspection was performed. Hosted desktop
outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2l1: immutable world-object snapshot**.

Acceptance boundary:

1. Add one sealed `Allrpcs.Domain.WorldObjects.WorldObjectSnapshot` that accepts an existing
   sequence of `WorldObject` instances, copies it exactly once, and exposes the exact retained
   instances in stable supplied order through a read-only list.
2. Permit an empty snapshot. Reject a null sequence, null entries, and entries whose
   `WorldObject.Identity.Id` values are duplicates, including distinct value-equal identifier
   instances. Do not compare display names or descriptions for identity.
3. Add focused tests first for stable order and exact-instance retention, caller-list mutation,
   empty input, every null boundary, value-equal duplicate rejection, and the minimal sealed public
   API.
4. Document Phase 2l1 compactly, leave the broader authoritative-world and focused-search roadmap
   items incomplete, run `./scripts/quality.sh`, review every intended path, and keep both planning
   documents pruned to current evidence and one exact next slice.

Explicit exclusions:

- Do not add lookup, selection, mutation, replacement, events, location or item association,
  perception filtering, discovery rules, visibility ranking, Godot adapters, runtime sources,
  persistence, serialization, providers, plugins, credentials, or UI.
- Do not replace the temporary focused-search targets or presentation, grant knowledge or evidence,
  or wire the new snapshot into ordinary play.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
