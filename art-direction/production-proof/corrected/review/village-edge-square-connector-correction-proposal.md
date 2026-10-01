# Village Edge–Village Square external connector correction proposal

Status: proposed; obtain separate product-owner approval before implementation. The previous
approval covered only the unchanged straight-path feasibility proof, which failed.

## Exact next bounded slice

Create one external-only `124x96` connector composition in the same world footprint
`x=1820..1944`, `y=852..948`. Keep the middle 60 columns (`32..91`) of the rotated straight-path
crop unchanged. Within the west 32 columns only, blend toward the retained Village Edge greybox
colour at the west boundary. Within the east 32 columns only, blend toward the accepted junction's
exact west-port RGBA column (`x=0`, rows `208..303`) and require the final column to match it.
Use deterministic interpolation and preserve the 96-pixel centreline/opening language. Keep the
original straight path and junction exports, masters, imports, and runtime copies byte-identical.

In the temporary preview instance only, shorten the underlying Square–Edge greybox presentation
line to end at `x=1820` instead of `x=1944`. This removes the exposed rectangular fringe only in
the proposed connector footprint, while retaining the greybox route inside Village Edge. Do not
change navigation, collision, the live route presenter, or `Main.tscn`. Retain the yellow opening
marker and all existing labels, accepted junction and Stream art, and other route approaches.

Validate the exact footprint, unchanged central pixels and source hashes, exact east seam, west
handoff, absence of greybox fringes, and retained opening/marker layering. Generate six
deterministic 0.8x/1.0x/2.0x wide/tall views at 1280x720/827 for owner review. If either transition
remains visibly unsuitable, document it and stop; do not widen the footprint or substitute assets.

This authorizes no ordinary-scene promotion, new production export/master, gameplay, topology,
collision, navigation, camera, UI, audio, scenario, dialogue, clue, discovery, treatment, save,
provider, release, or signing work. A successful external proof would still require separate
visual acceptance and runtime-promotion authorization.
