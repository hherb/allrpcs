# Allrpcs Handover

## Current state

Phases 2m19–2m22 are implemented on `codex/phase-2m19-plus-passive-noticing`, based directly on
PR #123's merge commit `a83abc5`.

The Godot scene now retains one complete temporary Seeker `Character`. Its identity and human
controller assignment reuse the focused-search controller's exact `CharacterId`; all other
required authored character components are explicitly empty. The existing controller also owns an
initially empty immutable passive-noticing candidate snapshot. Its production method accepts only
the controller's Seeker backed by a value-equal authoritative character and world object, delegates
duplicate-free exact append behavior to the Domain production transition, and otherwise preserves
state. Its resolve-next method fails closed without initialized perception or an eligible candidate,
then delegates stable selection and atomic resolution and installs both returned snapshots. It
remains the only live perception owner.

A separate temporary scene source retains one passive-noticing candidate using the exact character
source identity and first authoritative world-object identity. It does not call either controller
method. Ordinary play therefore still retains no candidates and performs no passive-noticing
production or resolution. The current temporary perception also seeds both scene objects at
`Noticed`, so the source candidate is deliberately not yet resolution-eligible. Focused-search,
perception initialization, movement, input, and presentation behavior remain unchanged.

Each slice was test-first: Phase 2m19 first failed with `CS0246` for the absent character source;
Phase 2m20 with `CS1061` for absent candidate ownership/production; Phase 2m21 with `CS1061` for the
absent resolve-next boundary; and Phase 2m22 with `CS0246` for the absent candidate source. All four
targeted host-shell Godot smokes pass. The final host-shell `./scripts/quality.sh` passed workflow
and formatting validation, a zero-warning build, 801 Domain tests, 107 Application tests, six
architecture tests, all eighteen Godot smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 2m23: temporary passive-noticing-eligible Seeker perception seed**.

Acceptance boundary:

1. Change only the temporary Seeker perception derivation so the passive candidate source's exact
   first world-object target begins at `Unnoticed`; retain every other world object at `Noticed` in
   stable source order and retain all exact identifier instances.
2. Resolve the candidate target from `TemporaryPassiveNoticingCandidateSource` rather than copying
   identity text or reading a greybox marker. Fail explicitly if the candidate does not identify an
   existing source world object; do not silently choose a fallback.
3. Extend the opt-in credential-free smoke evidence to prove exact candidate/perception alignment,
   independent derivation behavior, unchanged controller candidate state, and unchanged
   focused-search, movement, input, and presentation state.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not invoke candidate production or resolve-next during ordinary play; do not add polling,
  proximity, distance, line-of-sight, timing, focus, input, or presentation rules.
- Do not change focused-search lifecycle behavior, world-object authoring or ownership, character
  components, movement, input, or UI; and do not create a second live perception owner.
- Do not infer capabilities, inventory, conditions, knowledge, beliefs, relationships, goals,
  environmental state, discovery, or visibility from the temporary character or candidate.
- Do not add Application controllers, events, persistence, serialization, providers, plugins,
  credentials, or final UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
