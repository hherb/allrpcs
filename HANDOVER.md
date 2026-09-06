# Allrpcs Handover

## Current state

Phase 2l17 is implemented on `codex/phase-2l17-world-object-marker-label`, based directly on
PR #113's merge commit `d1a969e`.

`InteractionFocusController.RefreshFocus` now presents every existing labelled interaction marker
from the exact matching `WorldObject.Identity.DisplayName`. Missing objects clear their marker
labels, an unlabelled hidden marker remains unlabelled, and a matching disabled marker receives its
world-object name without becoming focus eligible. The marker's exported `DisplayName`, geometry,
visibility, selection, exact selected identifier, focus indicators, status, range, and ordering are
unchanged. No description is inspected or exposed, and no world object is retained beyond refresh.

The credential-free world-object interaction-focus smoke proves exact and independently named
distinct value-equal objects, missing and restored objects, hidden and disabled markers, cleared
missing-object labels, stable label-node and authored-input instances, and unchanged surrounding
state. After code review, host-shell `./scripts/quality.sh` passed with workflow and formatting
validation, a zero-warning build, 661 Domain tests, 107 Application tests, six architecture tests,
all fourteen Godot smokes, and the bounded graphical launch. The sandbox build passed but VSTest
could not bind its local socket. A separately rendered gameplay frame confirmed the two backed
marker names and the expected blank disabled marker without layout regressions.

Temporary sources, controllers, lifecycle controls, greybox focus, and discovery presentation
remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l18: independently authored temporary world-object identities**.

Acceptance boundary:

1. Make `TemporaryWorldObjectSource` author the existing herb-patch and stone-marker object
   identifiers and display names itself rather than reading them from `GreyboxInteractionTarget`.
   Preserve the exact values, two-object order, distinct objects, empty descriptions, and one stable
   retained snapshot.
2. Remove the now-unused `GreyboxInteractionTarget.DisplayName` input and its scene assignments;
   marker `Label` nodes remain presentation-only and continue to derive visible text through the
   Phase 2l17 world-object lookup. Keep `ObjectIdText` as the marker's referential input and require
   its parsed identifiers to remain value-equal to, but independently owned from, source objects.
3. Extend credential-free evidence for independent source and marker identifier instances, stable
   source identities and names, unchanged ordinary presentation, and unchanged focus, search,
   perception, outcome, geometry, visibility, and ordering state.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain or Application contracts; marker object-reference text; world-object lookup,
  snapshots, descriptions, or ordering; focus eligibility, range, selection, or status;
  focused-search lifecycle, perception, outcome, or resolved-presentation rules; or marker geometry.
- Do not make temporary scene state authoritative or add discovery grants, knowledge, evidence,
  events, persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
