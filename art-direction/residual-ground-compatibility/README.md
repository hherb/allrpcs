# Ground compatibility and sparse dressing audit

Read-only external evidence, 2026-10-04 (Australia/Sydney). Three bounded slices:
the complete native-scale compatibility matrix, larger plain/dressed repeat
comparisons and the [authored interface scope audit](interfaces.md).
No source paint, export, master, import or runtime file changed.
All 102 accepted artwork files across the ground-study and tiling packets and all
35 production-manifest terrain exports are hash-checked before rendering.

`audit.json` binds 155 inputs, including both owner acceptances, complete technical
review packets, the production manifest and label font. It records every tile and
overlay placement, dimensions, scale, alpha composition and direct edge metric.
`dressing.json` binds that audit and records each larger-field placement and phase.
These are diagnostic compositions, not new artwork candidates or owner acceptance.

## All 36 overlay combinations

Each panel repeats a 128px field 2x2 and centres one unchanged overlay at native
size. Six boards show every accepted variation on every new field. Transparent
padding and partial-alpha brush edges remain intact; no thresholds, rotations,
resizing or corrections are applied. The overlay crosses the internal tile joins.
PNG pixels are exact 1:1; a Markdown viewer may scale its preview to fit the window.

![Packed earth combinations](review/overlays-packed-earth-1x.png)
![Pale gravel combinations](review/overlays-pale-gravel-1x.png)
![Mixed meadow combinations](review/overlays-mixed-meadow-1x.png)
![Dry grass combinations](review/overlays-dry-grass-1x.png)
![Garden soil combinations](review/overlays-garden-soil-1x.png)
![Damp mud combinations](review/overlays-damp-mud-1x.png)

Visual observations at this sampled phase:

| Overlay | Observed behaviour |
| --- | --- |
| Pebble scatter | Individual stones remain discernible across all six fields; on gravel their shapes merge more with the field stones |
| Gravel scatter | Clearer on dark soil/mud and packed earth; weaker contrast against pale gravel, meadow and dry grass |
| Damp soil patch | A distinct local material patch on warm/light grounds; blends more closely with damp mud, while its silhouette remains visible |
| Sparse grass patch | More distinct against earth, soil and mud; subdued within meadow and dry grass |
| Worn earth patch | Restrained against packed earth/dry grass; conspicuous warm patch against soil, mud, gravel and meadow |
| Moss flecks | Visible on earth/gravel/soil/mud; subdued against the green meadow and dry grass |

Reduced contrast is retained evidence, not a failed technical composition or a
reason to repaint accepted artwork. Intended density, material placement,
accessibility and use-specific readability remain unverified. No opaque rectangular
padding is visible in these samples. This observation does not establish every
possible subpixel/filtering/placement result.

## Twelve legacy pairings, both join directions

The legacy 256px export is placed left of or above a 2x2 new field. Both preserve
their native repeat periods and phase zero. No seam mask, shared connector or
feathering hides the raw boundary. All 24 direct RGBA ports differ. Edge colour
metrics describe those exact samples; they do not determine visual acceptance.

![Quiet moss vertical joins](review/quiet_moss-vertical-1x.png)
![Quiet moss horizontal joins](review/quiet_moss-horizontal-1x.png)
![Leaf litter vertical joins](review/woodland_leaf_litter-vertical-1x.png)
![Leaf litter horizontal joins](review/woodland_leaf_litter-horizontal-1x.png)

All pairings display a straight material boundary. Quiet moss/meadow and leaf
litter/garden soil have closer average edge colours than their gravel pairings,
but retain different motif scale, paint character and boundary shape. Colour
proximity alone neither supplies a compatible paint port nor selects an interface.
These boards retain the unsuccessful **exact-port comparisons** in full; they are
not newly rejected artwork. The prior unwrapped-repeat and transparent-gutter
failures remain unchanged in their original packages.

## Larger plain and dressed fields

Each board compares identical 512px fields made of sixteen unchanged tiles. The
dressed side uses all six accepted overlays once, in recorded, non-overlapping
native-scale positions with differing tile phases. The two sides share the same
base paint phase; all labels stay outside the field. This is one sparse diagnostic
layout per material, not proposed world geography or a production density recipe.

![Packed earth dressing](review/dressing-packed-earth-1x.png)
![Pale gravel dressing](review/dressing-pale-gravel-1x.png)
![Mixed meadow dressing](review/dressing-mixed-meadow-1x.png)
![Dry grass dressing](review/dressing-dry-grass-1x.png)
![Garden soil dressing](review/dressing-garden-soil-1x.png)
![Damp mud dressing](review/dressing-damp-mud-1x.png)

Overlays add local variation; regular base motifs remain visible, particularly
the repeated clumps/diagonal groupings in meadow, dry grass and mud. Roughly
76–79% of each field's pixels remain unchanged. That fraction measures exact
composited pixel differences, not perceptual success. No new variation artwork
is justified by these diagnostics alone, and the terrain category remains open.

## Connector needs and next slice

The accepted earth/gravel and meadow/soil transition families already serve their
specific new-field pairs. They cannot be assumed to connect a legacy field.
Likewise the retained leaf-litter transition exports remain bound to their own
accepted source paint. A smooth boundary between a selected new field and a
legacy field would need a source-matched connector or a separately scoped
composition technique, with explicit 128px/256px phase handling. A sharp authored
material boundary might instead be intentional.

The live scene still assigns quiet moss to its broad `Ground` sprite. None of the
six new tiling fields or overlays is placed there by this work. The generic
comparisons therefore establish **no required new/legacy connector** and authorize
no terrain placement. Do not invent twelve transition families from this matrix.

The subsequent [interface scope audit](interfaces.md) binds 39 scene terrain-texture
assignments and 828 authored source files. It finds no explicit new-field use and
records exact conditional proof scopes for the retained ground/path/Clearing/Garden/
bank owners. The next slice depends on owner selection of a field and intended
interface/composition, or an explicit request for a placement proposal. Do not
assign new material geography, author connectors, modify artwork, import or promote
assets, or mark terrain complete from these diagnostics.

## Reproduce and verify

Dependencies: Pillow and NumPy; the recorded macOS Arial font. Run from the repo:

```sh
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/audit.py
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/dressing.py
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/interfaces.py
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/audit.py --check
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/dressing.py --check
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/interfaces.py --check
/opt/homebrew/bin/python3 art-direction/residual-ground-compatibility/review_check.py
/opt/homebrew/bin/python3 art-direction/residual-ground-tiling-proof/inspect_package.py
```

Builders write only this diagnostic package. `--check` recomputes complete board
pixels and metadata in memory and rejects changed inputs or outputs without
writing. `session-review.json` records independent pixel checks, rejection probes,
byte reproduction, visual review and limitations. Product/runtime/CI inputs remain
unchanged; retain [full-suite evidence](../woodland-environment-batch/validation.json)
instead of replaying unchanged product tests. Named runtime scope, import checks,
full product validation and owner runtime visual acceptance remain separate gates.
