# Allrpcs Handover

## Current state

Phase 2l19 is implemented on `codex/phase-2l19-world-object-descriptions`, based directly on PR
#115's merge commit `48d9373`.

`TemporaryWorldObjectSource` now authors and retains a distinct non-empty description for each
existing world object: `A cluster of narrow leaves gives off a sharp scent.` for the herb patch and
`Weathered carvings mark one face of the standing stone.` for the stone marker. Their exact
identifiers, display names, order, object and identity instances, and one retained snapshot remain
unchanged.

Credential-free source evidence covers exact description text, distinct stable description
instances, independently owned value-equal marker references, description-free focused-search
inputs, exact name-only marker labels and focus status, unchanged `IDENTIFIED` text and visibility,
unchanged focused-search result presentation, and preserved controller snapshots, geometry,
visibility, focus, and ordering. After code review, `./scripts/quality.sh` passed with workflow and
formatting validation, a zero-warning build, 661 Domain tests, 107 Application tests, six
architecture tests, all fourteen Godot smokes, and the bounded graphical launch.

Temporary sources, controllers, lifecycle controls, greybox focus, descriptions, and discovery
presentation remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l20: world-object-derived temporary resolved-description presentation**.

Acceptance boundary:

1. Extend each existing temporary resolved-perception indicator to present its looked-up world
   object's exact authored description only while that object is `Identified`. Reuse the existing
   indicator label; do not add UI.
2. Clear and hide the description for null or absent objects, null perception, non-matching
   perception, and every visibility state other than the exact `Identified` singleton. Accept a
   distinct value-equal perceived identifier while retaining all supplied objects and snapshots.
3. Extend credential-free presentation and interaction evidence for both authored descriptions,
   description replacement through a distinct value-equal world object, hide-again behavior, and
   unchanged marker labels, focus status, focused-search result text, lifecycle state, perception,
   outcomes, source objects, geometry, visibility, and ordering.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain or Application contracts; source descriptions; marker object-reference text;
  world-object lookup, snapshots, identities, or ordering; focus eligibility, range, selection,
  labels, or status; focused-search lifecycle, perception, outcome, or resolution rules; or marker
  geometry.
- Do not make temporary scene state authoritative or add discovery grants, knowledge, evidence,
  events, persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
