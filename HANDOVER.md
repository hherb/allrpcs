# Allrpcs Handover

## Current state

Phase 2l12 is implemented on `codex/phase-2l12-world-object-perception-source`, based directly on
PR #108's verified merge commit `ae46671d`.

`TemporarySeekerPerceptionSource` now derives its initial ordered visibility entries only from the
existing `TemporaryWorldObjectSource.WorldObjects` snapshot. It retains the controller-owned
Seeker identifier and each world object's exact stable identifier, pairs every entry with the
existing `VisibilityState.Noticed` singleton, and produces an empty perception snapshot from an
empty world-object snapshot. The scene still retains one stable perception snapshot.

The source no longer reads either greybox interaction target. Derivation leaves supplied snapshots,
world objects, identities, descriptions, identifiers, controller state, targets, and presentation
unchanged.

The focused smoke failed first because the snapshot-derivation boundary did not exist. After
implementation and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build,
661 Domain tests, 107 Application tests, six architecture tests, all thirteen Godot smokes, and the
bounded graphical launch. No manual visual inspection was needed because authored appearance did
not change.

The temporary sources, controllers, lifecycle controls, and identified-marker presentation remain
non-authoritative.

## Exact next bounded slice

Take **Phase 2l13: world-object-bound temporary focused-search context authoring**.

Acceptance boundary:

1. Make `TemporaryFocusedSearchContextSource` derive its capability- and tool-requirement target
   identifiers from the existing `TemporaryWorldObjectSource.WorldObjects` snapshot rather than
   reading greybox interaction targets directly. Preserve the current first-object capability and
   second-object tool mapping and retain their exact identifier instances.
2. Keep the exact existing capability, tool identifier, requirements, sets, conditions,
   capabilities, and inventory semantics and stable retained aggregate instances. Fail explicitly
   when the temporary snapshot cannot supply both required objects; do not fall back to targets.
3. Leave the world-object snapshot, objects, identities, descriptions, identifiers, Seeker state,
   targets, and presentation unchanged. Extend credential-free evidence with independently supplied
   objects so the world-object dependency and exact nested retention are observable.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain discovery-condition, perception, outcome, completion, or resolution
  contracts; requirement ordering or Boolean semantics; temporary `Noticed` or `Identified`
  semantics; world-object snapshots or lookup; or resolved-perception presentation rules.
- Do not inspect authored object text to select requirements, filter or rank world objects, make
  temporary sources or controllers authoritative, or add knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
