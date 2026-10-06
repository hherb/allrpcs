# Allrpcs Handover

Read [AGENTS.md](AGENTS.md) and [CONTINUATION.json](CONTINUATION.json) before choosing
work. Run `python3 scripts/check-continuation.py`. The ledger is the persistent queue;
this document is its short handoff. Direct owner instructions take precedence.

## Closed checkpoint

Grouped Clearing runtime is accepted and published in `ea63fcc4`: 113 assets / 64
masters; [terminal acceptance](art-direction/woodland-clearing-dressing-runtime-promotion/owner-runtime-acceptance.json),
[quality evidence](art-direction/woodland-clearing-dressing-runtime-promotion/verification.json).
The forest approach audit is complete and recommends no additional apron. The owner
also closed the redundant forest-dressing successor on 2026-10-06. No further automatic
ground/approach/dressing audits, comparisons or approval requests; preserve accepted
pixels and placements. All earlier closed scopes remain in the ledger.

The [loop investigation](CONTINUATION_AUDIT.md) explains the repaired routing and
approval failures. Historical package “pending”/“next” records are not the current queue.

<!-- active-slice:start -->
## Next bounded slice

**phase9-character-animation-import-proof — Import and register the complete approved directional character set** (ready).

Roadmap item: - [ ] Import and register the complete approved directional character animation set in
an isolated proof.

Missing deliverable: The approved new character directional frames have no imported
animation/registration proof. Artwork is approved; runtime registration, cadence and import behavior
remain provisional.

Use art-direction/characters-batch/owner-acceptance.json as the source ledger: 432 new approved
directional frames across nine families (four directions; two idle, two inspect, eight polished walk
frames per direction), plus the 48 unchanged inherited Seeker cloak frames. Build one isolated Godot
animation proof and a deterministic import recipe. Retain source pixels and editable masters; define
foot registration and cadence from measured frames and existing Seeker behavior. Show all
families/directions together with native/2x scale and cycle comparisons. Check alpha, exact decoded
pixels, registration, loop/frame order and lossless imports. Do not regenerate artwork or seek
artwork approval again. Consolidate scoped validation and review; request a decision only for a
genuinely changed proposal or a missing new runtime authority.

Done when: Every approved directional family plays idle, inspect and complete walk loops in the
isolated proof; source/import RGBA parity and measured registration pass, the complete comparison is
retained, and remaining Main integration authority is stated once. Mark this proof closed and choose
one genuinely unfinished roadmap deliverable.

Expected outputs:

- `art-direction/character-animation-import-proof/README.md`
- `art-direction/character-animation-import-proof/registration.json`
- `game/CharacterAnimationImportProof.tscn`

Boundary: Isolated import/animation proof only. Main.tscn, gameplay, controllers,
collision/navigation, scenarios, providers, functional UI, audio and release/signing stay unchanged.
Designs, portraits and sleeping/recovery semantics stay at their existing approved/provisional
scope.
<!-- active-slice:end -->

## Verification and local state

This continuation repair changes documents and a standalone checker only. Run
`python3 scripts/check-continuation.py --self-test` and `git diff --check`; retain
existing product quality evidence. Recheck product/import commands when implementing
the new isolated proof, according to actual changed inputs.

Preserve the unrelated unstaged `game/project.godot` editor rewrite. Use the published
project bytes in a disposable copy when frozen product checks need them. Preserve
Direction A paint/masters, physical owners, accepted scene order, final WholeChannel,
gate/Forester and all recorded terrain origins/depths. Do not edit frozen inventories
or validators to accommodate successors. Exclude `.DS_Store`.
