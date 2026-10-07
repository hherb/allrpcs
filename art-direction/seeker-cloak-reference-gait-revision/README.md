# Seeker cloak: reference-based natural gait

The owner supplied `reference/gait.jpeg` and asked to implement the described natural
support/swing sequence. `owner-request.json` records that instruction. This separate
west/east successor preserves all four prior candidates and all sixteen accepted
front/back exports and editable masters byte for byte.

The side cycle has sixteen 45ms poses. Each fixed anatomical leg progresses through
heel contact/loading, planted stance, trailing toe-off, lifted bent-knee swing,
knee advance and foot extension into the next contact. The other leg follows half
a cycle later. The 60/40 stance/swing split is an authoring choice from the diagram,
not a clinical measurement. Brief double support occurs at both support handovers.
The complete cycle remains 720ms; front/back retains eight 90ms accepted poses.

Both thigh roots stay at the centre of the painted pelvis, with the retained rounded
front contour. Two fixed-length bones use a backward-only knee branch. Grounded
support soles determine the common pelvis height; the swing foot lifts, passes and
extends. Lowest substantial foot alpha confirms heel contact and trailing toe roll. A stays anatomical
left and B stays anatomical right throughout, including occlusion and crossing.

## Results

- `review/production-sides-2x.webp`: actual imported Godot side views at normal speed.
- `review/old-vs-new-{1,2}x.webp`: all four views beside the retained prior candidate.
- `review/knee-foot-transfer-slow-2x.webp`: half-speed authoring overlay, red A/blue B.
- `review/all-48-phases.png`: every accepted/revised pose at native scale.
- `captures/full-cohort-{1,2}x.png`: corrected cloak with nine unchanged families.
- `masters/`: 48 editable 2x OpenRaster masters with torso/cloak and named leg layers.

`verification.json` and `authoring-check.json` record actual painted foot exchange,
knee-before-foot ordering in both half-cycles, actual heel/toe contacts, grounded double support, lifted swing,
fixed lengths, backward hinges, exact PNG/native-master replay and preservation.
The previous single-support timing fails the new double-support requirement.
Cold Godot imports decode exactly to all 48 PNGs. Playback observes every frame in
order, fixed pivots and at least two loops. Native/2x comparisons retain every side
pose. Lossless decoded WebP pixels and 45ms/90ms durations are checked explicitly.
All nine other family rows have zero changed RGBA pixels at both scales. Original
product/CI hashes, frozen proof and the local project editor rewrite are preserved.
The sandbox could not save global editor settings; asset import completed and
host Compatibility rendering/playback checks passed.

## Reproduction and boundary

Run `/opt/homebrew/bin/python3 art-direction/seeker-cloak-reference-gait-revision/check_gait.py`
and `python3 scripts/check-continuation.py`. The generation/review scripts refuse to
overwrite a retained `candidate.json`; reproduce generation in a separate copy.
For Godot, copy `game` to a disposable project excluding `.godot`, `.DS_Store`, `bin`
and `obj`, and use the published `project.godot` bytes there. Import with
`godot --headless --path <copy>/game --editor --import`, then run
`SeekerCloakReferenceGaitCheck.gd` with Compatibility rendering, fixed FPS 60 and
`-- --output=<new empty captures directory>`. `check-cohort.gd` provides the isolated
full-cohort capture check; its output path names this package and must be redirected
when replaying elsewhere.

The isolated scenes are `game/SeekerCloakReferenceGaitProof.tscn` and
`game/CharacterAnimationImportProofReferenceGaitCloak.tscn`. Main has not been changed.
Upper-body/arm motion remains the inherited cutout paint. This candidate is presented
for visual review; implementation authority does not imply artwork acceptance or
Git publication. Existing front/back acceptance needs no additional selection.
