# Proposed whole-channel production/runtime promotion scope

Status: **proposal; no implementation approval recorded**. Prepared on 2026-10-03
against `e976ea00e1e4e121dc726807b8531e455c42d664`. The owner’s “both approved”
accepts the displayed [external overview](../approved-external-compositions/channel/review/overview.png)
under the [hash-bound acceptance record](../forest-path-ending-correction/owner-acceptance.json).
It does not authorize production imports, ordinary runtime wiring or physical changes.
[Source bindings](source-bindings.json) pin the accepted assembly, its local layer inputs
and the live presentation/physical owners used to prepare this proposal.

## Requested approval and checkpoints

Approve **Stage 1 only: a disposable whole-channel runtime presentation preview**, derived
from the accepted sources below. Stage 1 may add a separate proof package, source-derived
transparent layers, editable masters, validators and an explicit capture script. Load the
live Main scene into memory for comparison; never pack/save it or write production assets,
runtime imports, ordinary startup code or existing evidence. Preview imports stay in a
disposable project copy with independent IDs; retained proof files live in the new package.
Do not import into the tracked game project. No implementation begins before named
approval of this document’s Stage 1.

After Stage 1, present its exact outputs, digests, placement, draw order and runtime views.
Prepare a second named **Stage 2 production scope** with the final asset IDs, filenames,
master/export counts and exact allowed code changes. Obtain separate approval of that
packet before production implementation. This two-stage proposal is necessary because the
accepted image is an opaque source assembly, and the retained crossing is a CPU reference.
There is no reviewed transparent runtime asset or GPU comparison to promote yet.

Stage 2 must retain the limits below, run full quality validation and obtain owner runtime
visual confirmation before publication. Approval of either stage grants no physical scope.

## Frozen source composition

The exact 2604×620 assembly has world origin `(-64,212)`, ending at `(2540,832)`
(exclusive). Its export SHA-256 is
`ffcbd8cefe50621d752ae3b2739480e93f3d410bba6d107d2468e3813d679735`;
the accepted overview SHA-256 is
`c9bf16c3c532caa22e1bd119ffdaf891ac8419ac64dd73b620b14bc6a1a5f857`.
The [assembly manifest](../approved-external-compositions/channel/manifest.json)
and [recipe](../approved-external-compositions/tools/channel.py) define the reference:

| Component | World placement and retained constraint |
| --- | --- |
| Western continuation | `(-64,384)`, 1600×448 recipe canvas; absolute-world water/ground phase; 32px source banks BOX-filtered to 16px |
| Outlet | `(0,504)`; exact accepted outlet water and north/south banks |
| B2 | `(432,384)`, 512×448 source crop; 96×288 rail-free deck at retained centre `(685.4545454545455,600)` |
| B1 | `(1088,384)`, 512×448 source crop; 96×336 rail-free deck at retained centre `(1318.1818181818182,600)` |
| Accepted fringe successor | `(1536,384)`, 128×256; last in the reference assembly; replaces only the accepted join footprint |
| Retained Stream | `(1600,384)`, 400×256 reference; original cap/bank recipe resumes at x=1664 |
| Curved spring | `(1900,212)`, 640×576 source crop; 384px basin at `(2192,500)`, exact 32px inlet; preserve all accepted x<2000 pixels |

The leftmost 64px are water-only off-world reference. Preserve the closed physical world
boundary. Proposed east-to-west flow remains art context, with no new scenario authority.

## Stage 1 rendering contract

Derive water, banks, deck and spring detail from the existing independent layer exports
and frozen recipes. Retain source-space coordinates, phase, colours, alpha and filtering.
The accepted fringe’s ground-under-bank correction is a bounded source-derived patch;
retain its exact visible composition, including translucent edge rows. Do not extend it
past x=1664 or repaint the original caps. Preserve accepted local exports and masters.

Do not use the opaque joined canvas as a full-scene sprite, remove its ground by colour
keying, or invent a new shoreline. It contains quiet-moss ground and cropped road tails;
those rectangles must not overwrite Clearing, village, laundry, routes or labels. Assemble
transparent components from their original layers/masks. Do not include duplicated road
layers or extend local road tails. Keep existing complete routes visible underneath their
bounded B1/B2 deck treatment. Ground correction is allowed only where a frozen source
recipe explicitly requires it, with a recorded mask and exact composite parity.

First require decoded RGBA equality with the accepted CPU composition on its quiet-moss
reference background and retained reference roads/crossing. Validate local outlet/B2/spring
views exactly; B1 allows only the already accepted join override. Export transforms must
be documented and deterministic, transparent RGB zeroed, and editable masters exactly 2x.
If extracting layers cannot reproduce the accepted pixels, stop and report the mismatch;
prepare a separate correction for assessment rather than silently changing accepted art.

In the disposable live scene, retain the actual Stream water UVs, crossing polygon colour,
cap mirrors/regions and labels. Do not bake the CPU crossing colour into a replacement
texture. Compare ordinary and candidate GPU captures under identical framing and report
CPU/GPU differences separately. Exact CPU source parity does not imply GPU capture parity.
Suppress duplicate presentation only in the exact overlap masks and only in memory; record
every suppressed node/region and its reason. The live Stream scene source configures its
polygons in `_Ready`, so capture after initialization and at least one explicit rendered frame.

Record an explicit effective z/sibling-order table. Water/banks must cover the appropriate
route crossing area, decks must cover water/banks, complete road approaches must stay visible,
and retained labels/actors must stay above terrain. Preserve all existing route siblings,
including the final ClosedForestPath. Check both bridge ends, alpha seams and spring inlet
in actual captures; do not select z values solely from CPU draw order.

## Physical and production limits

Stage 1 changes no tracked `game/` file. Retain Stream collision polygons, geometry, scene
binding/layout, navigation cells, path centreline/width, outer boundaries, camera defaults,
Clearing walls/gate and Forester access. New western water, spring and bridge decks are
visual only. Existing walkable space can therefore remain walkable beneath newly drawn
water. B1/B2 evidence proves cell-centre feasibility only: fringe cell corners protrude by
about 5.25px/5.56px. This proposal authorizes no actor-clearance or bank-exclusion fix.
Expose these limits in the review, without claiming gameplay or accessibility acceptance.

Stage 2 may propose only production builder/manifest entries, exact 2x editable masters,
matching lossless proof/runtime imports and bounded presentation wiring/smokes necessary
for the approved Stage 1 output. Disable mipmaps, alpha-border processing and premultiplication;
follow runtime terrain filtering. Preserve the 84 existing exports, 35 masters and their bytes.
Do not alter the geometry or collision construction in `OpeningRegionStreamLaundrySceneSource`
or repurpose the CPU reference as the physical Stream owner. Freeze exact output hashes and
allowed presentation nodes before Stage 2 approval; do not infer asset counts from this proposal.

Preserve accepted forest runtime art, all paths, Stream–Laundry publication authority and
the accepted Shrine triangle. No change to scenario, dialogue, clues, discovery, treatment,
saves, providers, UI, audio, release or signing. Capture-only HUD/Seeker hiding and overview
framing stay out of ordinary startup. Historical pending/rejected records remain immutable.

## Required evidence and next handoff

Stage 1 must freeze a new complete predecessor inventory at its approved starting head,
prove preserved historical subsets and bind all source, recipe, import and capture inputs.
Reproduce old validators on their pinned Git trees; never rewrite old hashes/inventories
to accommodate this successor. Include stale-input, changed-source, seam/UV drift, duplicate
road/crossing and out-of-footprint mutation rejections. Ordinary checks must not rebuild
or re-record capture evidence.

Capture ordinary/candidate paired views at outlet, B2, B1, x=1664 interface, retained
crossing and spring, each at 0.8x/1x/2x in 1280×720 and 1280×827 frames: 36 pairs,
72 captures. Add an explicitly framed whole-channel overview pair. Bind actual target
coordinates and visibility settings before capture. Review full-width deck ends, off-world
outlet, all translucent joins and overlap with existing roads/landmarks. Re-run the unchanged
runtime/navigation/Stream/forest smokes and `./scripts/quality.sh` before the Stage 2 packet.

Proposal verification on the clean starting head passed the forest runtime validator’s
asset/import/master checks, all 18 exact runtime comparisons, preservation of 4,170 protected
files, and pinned historical reproduction including the whole-channel recipe. The proposal
itself changes documentation only. No new runtime capture or full quality run is claimed.

The next slice is Stage 1 **after named approval**. End it with a concrete Stage 2 packet
and runtime views for review, preserving all physical and publication checkpoints above.
