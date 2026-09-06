# Allrpcs Handover

## Current state

Phase 2l16 is implemented on `codex/phase-2l16-world-object-focus-status`, based directly on
PR #112's merge commit `7675189`.

`InteractionFocusController.RefreshFocus` now presents a selected focus's status from the exact
looked-up `WorldObject.Identity.DisplayName`, including when a distinct value-equal replacement
changes the name without changing the selected authored index. Clear focus still presents
`Interaction focus: NONE`. Selection, exact identifier retention, indicators, range, authored-order
tie-breaking, missing-nearest fallback, and all-missing clearing are unchanged; no object description
is inspected or exposed, and no `WorldObject` is retained beyond refresh.

The credential-free world-object interaction-focus smoke proves exact and independently named
value-equal objects, the independently named fallback object, same-index status refresh,
all-missing clearing, restoration, and unchanged surrounding state and input instances.

After code review, host-shell `./scripts/quality.sh` passed with workflow validation, formatting, a
zero-warning build, 661 Domain tests, 107 Application tests, six architecture tests, all fourteen
Godot smokes, and the bounded graphical launch. The sandbox build passed, but VSTest could not bind
its local socket. Ordinary authored names are unchanged, so no manual visual inspection was needed.

Temporary sources, controllers, lifecycle controls, greybox focus, and discovery presentation
remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l17: world-object-derived temporary interaction-marker label presentation**.

Acceptance boundary:

1. Present each existing labelled interaction marker from the exact matching
   `WorldObject.Identity.DisplayName`, not `GreyboxInteractionTarget.DisplayName`; do not inspect or
   expose descriptions. A marker without a matching object must not present an authored object name.
2. Keep marker geometry, visibility eligibility, focus indicators, status, exact selected identifier,
   pure selection, range, authored-order tie-breaking, missing-nearest fallback, and all-missing
   clearing unchanged. Do not overwrite the marker's authored `DisplayName` input.
3. Extend credential-free evidence for independently named distinct value-equal objects, missing and
   restored objects, hidden or disabled markers, and unchanged state outside marker-label
   presentation. Manually inspect any changed ordinary-play appearance.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain or Application contracts; focus eligibility, range, or ordering; world-object
  snapshots or lookup; focus-status or resolved-perception rules; or object descriptions.
- Do not retain or expose a world object beyond refresh, make temporary scene state authoritative,
  or add discovery grants, knowledge, evidence, events, persistence, serialization, providers,
  plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
