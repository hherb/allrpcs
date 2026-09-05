# Allrpcs Handover

## Current state

Phase 2k48 is implemented on `codex/phase-2k48-recorded-outcome-resolution-input`, based directly
on PR #95's verified merge commit `cd0af38`.

The temporary physical R action now selects the first retained outcome in stable recorded order
whose request belongs to the controller's value-equal Seeker identifier. `Main` passes that exact
outcome to `FocusedSearchActivityController.TryResolveOutcome` and presents `RESOLVE: CHANGED` or
`RESOLVE: UNCHANGED` through the existing result label. No matching outcome changes the label only;
all controller snapshots retain their exact instances.

The interaction smoke proves the exact R mapping and TEMP control text; F then E outcome recording;
target-only `Noticed` to `Identified` perception replacement; outcome consumption; exact unrelated
perception-entry instances and order; completed activity absence; repeated-R identity; unchanged
resolution inputs; and perception preservation for released, unrelated, movement, and healer input.

The test-first smoke failed at the missing R mapping. After implementation and code review,
host-shell `./scripts/quality.sh` passed with a zero-warning build, 631 Domain tests, 107 Application
tests, six architecture tests, all eleven Godot smokes, and the bounded graphical launch. The
sandbox-only Godot attempt hit the known `user://logs` crash; the identical host-shell smoke passed.
The graphical launch and exact-text smoke cover the widened temporary panel, but no separate manual
visual inspection was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k49: temporary resolved-perception presentation**.

Acceptance boundary:

1. Add one narrow temporary Godot presentation adapter for the existing herb-patch and stone-marker
   greybox targets. It may receive only each target's exact `ObjectId` plus the controller's current
   Seeker perception and must delegate target selection to `CharacterPerceptionLookup`.
2. Present a small temporary `IDENTIFIED` marker only when that exact target's retained visibility
   is `VisibilityState.Identified`; `Noticed`, absent, null, and unrelated entries remain hidden.
   Do not rank states or expose any other perception entries.
3. Initialize the presentation from controller perception after the existing one-time initialization
   and refresh it only after successful R resolution. Resolution selection and controller ownership
   must remain unchanged and independent of current interaction focus.
4. Extend credential-free smoke coverage to prove both indicators start hidden, each F/E/R sequence
   reveals only its matching target, the unrelated target and exact inputs remain unchanged, and
   failed or repeated R preserves presentation.
5. Document Phase 2k49 compactly, leave the broader focused-search item incomplete, run
   `./scripts/quality.sh`, review every intended path, and keep both planning documents pruned to
   current evidence and one exact next slice.

Explicit exclusions:

- Do not add final interaction UI, automatically complete or resolve searches, select outcomes by
  focus, reveal hidden/private state, grant knowledge or evidence, or add failure reasons.
- Do not add target-specific discovery rules, visibility ranking, coordinates, range policy,
  passive noticing, reinterpretation, authoritative objects or characters, persistence,
  serialization, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
