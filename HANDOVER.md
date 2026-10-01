# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete; Direction A is selected. Phase 9.2 has 73 manifest-bound
exports and 24 editable OpenRaster masters. `main` at `1eb3897` contains the owner-approved
Village Edge–Square connector promotion (acceptance of the six external alpha previews).

See [runtime result](art-direction/production-proof/corrected/review/village-edge-square-connector-runtime-result.md)
for geometry, the six views, provenance, scope, prior review, and reproduction commands. On 2026-10-01,
the continuation displayed all six archived runtime views and revalidated captures and promotion
scope successfully. A separate human runtime response is still pending. Run with a Pillow-enabled
Python; this host's default `python3` lacks Pillow, while `/opt/homebrew/bin/python3` works:

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/validate_village_edge_square_connector.py --check-captures --check-promotion-scope
```

The optional scope check freezes the promotion's scene/code baseline; regular quality omits it.
Earlier proofs' `--check-live-baseline` hashes are superseded. The promotion session passed review,
negative corruption checks, and host-local `./scripts/quality.sh` (5,033 tests, zero-warning build,
all validators and Godot smokes, Compatibility launch). This continuation changed documentation
only and reran the focused validator; it did not rerun full quality or regenerate captures.

## Exact next bounded slice

Obtain the owner's ordinary-scene visual checkpoint for the six runtime views: west handoff,
fringe removal, east seam, and marker/label layering. Record only bounded acceptance or feedback;
do not advance past this pending checkpoint or infer acceptance from generic continuation.
No additional runtime promotion is authorized. Preserve physical/gameplay authority,
topology, collision, navigation, camera, scenario, dialogue, clue, discovery, treatment, save,
provider, UI, audio, release, and signing. Do not infer pacing, usability, or accessibility acceptance.
