# Allrpcs Handover

## Current state

Phase 2l18 is implemented on `codex/phase-2l18-independent-world-object-identities`, based directly
on PR #114's merge commit `7f74a30`.

`TemporaryWorldObjectSource` now authors the existing `herb-patch` / `HERB PATCH` and
`stone-marker` / `STONE MARKER` identities itself in the same two-object order and retained
snapshot. Each source ID is value-equal to, but a distinct instance from, the corresponding
`GreyboxInteractionTarget.ObjectIdText` reference. Both descriptions remain empty. Markers no
longer expose or receive `DisplayName`; their `Label` nodes remain presentation-only and still
receive world-object names through `InteractionFocusController` lookup.

Credential-free smoke evidence covers the marker/source ownership split, exact stable source
identities and names, marker property surface, and unchanged marker order, geometry, visibility,
focus, focused-search activity/outcomes/perception, and presentation. After code review,
host-shell `./scripts/quality.sh` passed with workflow and formatting validation, a zero-warning
build, 661 Domain tests, 107 Application tests, six architecture tests, all fourteen Godot smokes,
and the bounded graphical launch.

Temporary sources, controllers, lifecycle controls, greybox focus, and discovery presentation
remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l19: independently authored temporary world-object descriptions**.

Acceptance boundary:

1. Give the existing herb-patch and stone-marker objects distinct non-empty descriptions authored
   only by `TemporaryWorldObjectSource`. Preserve their exact identifiers, display names, order,
   object and identity instances, and one retained snapshot.
2. Extend credential-free source evidence for exact description text and stable description
   instances. Prove that the descriptions are not copied into marker labels, focus status, the
   temporary `IDENTIFIED` indicator, or any focused-search input or result presentation.
3. Preserve independently owned value-equal marker references and all existing focus, search,
   perception, outcome, geometry, visibility, and ordering behavior.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain or Application contracts; marker object-reference text; world-object lookup,
  snapshots, identities, or ordering; focus eligibility, range, selection, labels, or status;
  focused-search lifecycle, perception, outcome, or resolved-presentation rules; or marker geometry.
- Do not make temporary scene state authoritative or add discovery grants, knowledge, evidence,
  events, persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
