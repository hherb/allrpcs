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

The [complete character import proof](art-direction/character-animation-import-proof/README.md)
is finished locally: 480 unchanged approved frames, 120 isolated animations, measured
fixed pivots, exact cold-import RGBA and six lossless native/2× comparisons. Its
[verification](art-direction/character-animation-import-proof/verification.json)
records technical import fidelity; Main integration remains outside that scope.
The complete cloak gait correction is now owner-accepted and closed. The owner
said on 2026-10-07: “very good - accepted. This is our reference gait model henceforth”.
[Terminal acceptance](art-direction/seeker-cloak-reference-gait-revision/owner-acceptance.json)
binds the exact 48-frame candidate and isolated proof. The
[reference gait model](art-direction/reference-gait-model.md) governs future walking
animation work. Accepted front/back and all predecessors remain unchanged.
Historical pending fields are superseded; no more unchanged-view gait gates.
The deferred audio-policy slice is restored below.
The owner authorized commit and push of this accepted work on 2026-10-07.
Publication includes the character proof, retained gait revisions, accepted
reference model and documentation; preserve the unrelated project editor rewrite.

<!-- active-slice:start -->
## Next bounded slice

**phase9-audio-direction-dynamic-range-policy — Establish the audio direction and dynamic range policy** (ready).

Roadmap item: - [ ] Establish the audio direction and dynamic range policy.

Missing deliverable: Phase 9.3 has no bounded audio direction or dynamic-range policy specifying cue
hierarchy, quiet investigation, ambience/music roles and accessible independent controls.

Read the settled game direction and existing audio constraints, then write one concrete Phase 9.3
policy covering sonic palette, foreground/background cue hierarchy, meaningful off-screen cues,
restrained music, provisional loudness/headroom and ducking targets, future voice coexistence,
accessibility and the required independent buses/controls. Keep numerical targets explicitly
provisional until measured against real audio. Identify asset classes and a finite later
audition/implementation boundary. Prepare the policy for owner review; record only the authority
actually granted. Do not generate, purchase or install audio, add runtime buses or settings,
implement footsteps/music/voice, change accepted visuals or reopen completed
character/ground/dressing scopes.

Done when: One coherent audio direction and dynamic-range policy, with explicit provisional targets,
cue/bus responsibilities, accessibility constraints and a finite next implementation boundary, is
retained and reviewed; any needed owner choice is stated once. Close the policy slice when its
deliverable is complete and select one named unfinished roadmap item.

Expected outputs:

- `audio-direction/README.md`
- `audio-direction/dynamic-range-policy.json`

Boundary: Documentation and reviewable audio-policy proposal only. Main, gameplay, runtime
audio/assets/buses/settings, providers, voice implementation, accepted visual scopes,
release/signing and Git publication stay outside this slice.
<!-- active-slice:end -->

## Verification and local state

Retain the passing original character proof and preceding 5,033-test product evidence;
ordinary product/CI inputs remain bound to the original proof's boundary snapshot.
Use the retained passing product and gait evidence; documentation-only maintenance
needs `python3 scripts/check-continuation.py --self-test` and `git diff --check`.
The accepted gait package retains exact native-master replay, contact/transfer
checks, cold import/playback and native/2× captures; do not regenerate its bound
files or rerun unchanged visual selections. Full product checks are needed only
when product inputs or unresolved defects warrant them.

Preserve the unrelated unstaged `game/project.godot` editor rewrite. Use the published
project bytes in a disposable copy when frozen product checks need them. Preserve
Direction A paint/masters, physical owners, accepted scene order, final WholeChannel,
gate/Forester and all recorded terrain origins/depths. Do not edit frozen inventories
or validators to accommodate successors. Exclude `.DS_Store`.
