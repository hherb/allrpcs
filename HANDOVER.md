# Allrpcs Handover

## Current state

Phase 2l13 is implemented on `codex/phase-2l13-world-object-focused-search-context`, based
directly on PR #109's verified merge commit `9145fb17`.

`TemporaryFocusedSearchContextSource` now derives its capability- and tool-requirement targets
only from `TemporaryWorldObjectSource.WorldObjects`. The first object's exact identifier retains
the existing capability-only mapping, and the second object's exact identifier retains the
existing tool-only mapping. Fewer than two objects fail explicitly without falling back to
greybox targets. The scene initializes the world-object source before this dependent source.

The existing capability, tool identifier, requirements, sets, conditions, capabilities, and
inventory semantics remain unchanged, and the scene retains one stable set of aggregate instances.
Independent-world-object smoke evidence proves exact identifier and nested-input retention,
unchanged supplied objects, and explicit undersized-snapshot failure.

The focused smoke failed first because the world-object context derivation boundary did not exist.
After implementation and code review, host-shell `./scripts/quality.sh` passed with a zero-warning
build, 661 Domain tests, 107 Application tests, six architecture tests, all thirteen Godot smokes,
and the bounded graphical launch. Authored appearance did not change, so no manual visual inspection
was needed.

Temporary sources, controllers, lifecycle controls, and identified-marker presentation remain
non-authoritative.

## Exact next bounded slice

Take **Phase 2l14: world-object-gated temporary focused-search request and start**.

Acceptance boundary:

1. Make `InteractionFocusController.CreateFocusedSearchRequest` receive an existing nullable
   `WorldObject` with the searcher identifier. Require a current focus and a value-equal target
   match; reject null, absent-focus, and mismatched objects, and retain the successful object's exact
   identifier instance without falling back to a greybox target identifier.
2. Make `TryStartFocusedSearch` receive and delegate that existing nullable object. In `Main`'s F
   path, resolve only the current focused identifier through `WorldObjectController`, pass the exact
   returned object, and report `UNCHANGED` without invoking the start path when it is missing.
3. Preserve focus selection and presentation, context aggregates, all controller snapshots,
   world-object/source/target state, and input instances. Extend credential-free evidence for null,
   absent, mismatched, exact, and distinct value-equal objects plus missing-object state identity.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain request, discovery-condition, perception, outcome, completion, or resolution
  contracts; focus range or ordering; requirement semantics; world-object snapshots or lookup; or
  resolved-perception presentation rules.
- Do not inspect world-object identity text or descriptions, filter or rank objects, make temporary
  sources or controllers authoritative, or add knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
