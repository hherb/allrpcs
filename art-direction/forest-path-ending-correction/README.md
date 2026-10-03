# External forest path ending correction

The owner reported “teh forest path sectiom looks odd with a cut-off path section”
after the exact bounded 2x forest view was shown. [owner-request.json](owner-request.json)
binds the statement and its image digest. The original candidate remains intact as
failed visual evidence; this successor is external only and awaits its own assessment.

![Rounded forest end and eased gate edge](result/captures/corrected-gate-2.0x-wide.png)

The first candidate used a finite straight strip and hard horizontal gate clipping.
Its flat forest terminus and clipped gate side left a triangular fragment. This
correction uses the retained `path-end-cap.png` at the forest end and a source-alpha
ease near the gate. Both repository textures remain unchanged; no new painting is added.

The end-cap entry starts 88px before the old endpoint `(650,150)`, with a 16px
premultiplied blend finishing 72px before that endpoint. Its rounded nose extends about
32px past that old flat terminus. This is decorative external endpoint coverage, not
a moved anchor, navigable extension or layout promotion. Source colours and alpha are
interpolated together to avoid doubled opacity or a dark join.

The gate-facing alpha fades smoothly from y=168 to zero at y=196. It has zero slope
at both limits, eliminating the solid horizontal cutoff. All y≥196 pixels remain
fully transparent, preserving the gate band and Clearing territory. The fade changes
4,601 visible source-alpha pixels while retaining their RGB. The route line is hidden
only in the disposable capture, exactly as in the predecessor candidate. The physical
approach outlines and visibly closed gate remain unchanged.

The export canvas covers `(520,32)` through `(1072,416)`, with transparent gutters.
[Manifest](result/manifest.json) binds source/output digests and the exact nearest 2x
editable [OpenRaster master](result/masters/composition.ora). [Evidence](result/evidence.json)
records the geometry and limits. Production remains at 83 exports and 34 masters.

## Verification

```sh
/opt/homebrew/bin/python3 art-direction/forest-path-ending-correction/tools/proof.py --check-predecessors
/opt/homebrew/bin/python3 art-direction/forest-path-ending-correction/tools/check.py
PATH=/opt/homebrew/bin:$PATH ./scripts/quality.sh
```

`proof.py --build` rebuilds only the successor. Run the explicit `tools/capture.gd`
with Godot Compatibility after any render-input change, then `check.py --record` to
bind the new captures. The capture-time manifest and overlay stamps reject stale views.
There are 18 captures: gate, midpoint and closed end; 0.8x/1x/2x; wide/tall frames.

[baseline.json](baseline.json) preserves all 4,155 predecessor files, excluding only
this new folder, the requested handover update and `.DS_Store`. The predecessor wrapper
checks the previous preservation inventory is a subset, then reproduces the frozen
channel and forest proof, its 54 captures, and the earlier accepted outlet/spring/join.
Historical bytes, statuses and inventories are never rewritten.

Review checked source-derived endpoint coverage, premultiplied blending, transparent
gate territory, alpha easing, hidden RGB, gutters, exact master parity, and unchanged
predecessors. Five mutations reject gate intrusion, an opaque fade cutoff, altered or
missing rounded source pixels, and hidden RGB. Independent camera projection confines
all 18 old/new capture differences to the external correction area, with a 3px filter
allowance. The 2x gate view was visually inspected; automated checks are not owner approval.
Full host quality passed: zero-warning build, all 5,033 tests, asset/scenario validators,
all credential-free Godot smokes and Compatibility launch. The host run used the
established workaround for sandbox-restricted Godot log rotation.

## Next checkpoint

Assess this exact new forest view. Whole-channel visual assessment remains pending.
Further corrections must preserve both predecessors and accepted local views. No
runtime/import, layout, physical, scenario, UI, audio, release or signing promotion
is authorized by this request.
