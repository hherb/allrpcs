# Allrpcs Handover

## Current state

Phase 2l15 is implemented on
`codex/phase-2l15-world-object-gated-interaction-focus`, based directly on PR #111's
merge commit `39ab377`.

`InteractionFocusController.RefreshFocus` now resolves each authored marker identifier through
`WorldObjectController` and makes only a matching existing object eligible for the otherwise
unchanged visible, enabled, in-range selector. The selected focus retains the looked-up object's
exact identifier instance. A missing nearest object falls back under the existing distance and
authored-order rules; no eligible object clears the focus index, identifier, indicators, and status.

A dedicated credential-free smoke covers exact and distinct value-equal objects, exact identifier
retention, missing-nearest fallback, all-missing clearing, restoration, and unchanged focused-search,
perception, context, source, object, and target instances. The existing interaction lifecycle smoke
also remains green.

After code review, host-shell `./scripts/quality.sh` passed with workflow validation, formatting, a
zero-warning build, 661 Domain tests, 107 Application tests, six architecture tests, all fourteen
Godot smokes, and the bounded graphical launch. The sandbox run reached the tests but VSTest could
not bind its local socket. Authored appearance did not change, so no manual visual inspection was
needed.

Temporary sources, controllers, lifecycle controls, greybox focus, and identified-marker
presentation remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l16: world-object-derived temporary interaction-focus status presentation**.

Acceptance boundary:

1. Present a selected focus's status name from the exact looked-up
   `WorldObject.Identity.DisplayName`, not `GreyboxInteractionTarget.DisplayName`; continue to show
   `Interaction focus: NONE` when focus is clear. Do not expose or inspect the object description.
2. Preserve the selected world object's exact identifier instance and the existing pure selector,
   range, authored-order tie-breaking, missing-nearest fallback, and all-missing clearing behavior.
3. Extend credential-free evidence for an independently named distinct value-equal object, the
   fallback object's name, all-missing clearing, restoration, and unchanged state outside focus
   status and indicators.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain request, discovery-condition, perception, outcome, completion, or resolution
  contracts; Application focus selection; focus range or ordering; requirement semantics;
  world-object snapshots or lookup; target marker labels; or resolved-perception presentation rules.
- Do not retain or expose a world object beyond the refresh, make temporary sources or controllers
  authoritative, or add discovery grants, knowledge, evidence, events, persistence, serialization,
  providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
