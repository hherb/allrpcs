#!/usr/bin/env python3
"""Consolidated scoped checks for the owner-accepted external tiling series."""

from contextlib import redirect_stdout
from io import StringIO
import json

from build import ROOT, PAIRS, KINDS, digest
from authoring_check import main as authoring_checks, require, raster, edges, equal

SCOPE = 'complete twelve-module external residual ground tiling feasibility series'


def check_all_oriented_ports():
    """Check every oriented straight/corner connector, including undrawn inset turns."""
    checks = 0
    for pair in PAIRS:
        base = {kind: edges(raster(ROOT / f'exports/{pair}-{kind}.png')) for kind in KINDS}
        # Every oriented port must be an exact orientation of its canonical mask port.
        # RGB paint is world-aligned: compare actual compatible ports on their same axis.
        all_tiles = {}
        for kind in KINDS:
            for turns in range(4):
                path = (f'review/{pair}-{kind}-orientation-{turns}.png' if turns
                        else f'exports/{pair}-{kind}.png')
                all_tiles[kind, turns] = edges(raster(ROOT / path))
        for turns in range(4):
            # Canonical outer top and inset bottom share the straight top/bottom port.
            # A CCW coverage turn moves edge index e to (e-turns) modulo four.
            outer_edge = (0 - turns) % 4
            inset_edge = (2 - turns) % 4
            straight = all_tiles['straight', turns]
            require(equal(all_tiles['outer', turns][outer_edge], straight[outer_edge]),
                    f'Oriented outer connector: {pair}/{turns}')
            require(equal(all_tiles['inset', turns][inset_edge], straight[inset_edge]),
                    f'Oriented inset connector: {pair}/{turns}')
            checks += 2
        require(equal(base['straight'][0], base['straight'][2]), 'Straight repeat')
    return checks


def main():
    """Validate exact acceptance, frozen review inputs and the whole candidate package."""
    acceptance = json.loads((ROOT / 'owner-acceptance.json').read_text())
    manifest = json.loads((ROOT / 'review-manifest.json').read_text())
    require(acceptance['status'] == 'approved' and acceptance['approval_scope'] == SCOPE, 'Owner acceptance scope')
    require(acceptance['owner_statement'] == 'Looks good - all approved', 'Recorded owner statement')
    require(digest(ROOT / 'review-manifest.json') == acceptance['review_manifest_sha256'],
            'Accepted review packet changed')
    artwork = {path: sha256 for path, sha256 in manifest['files'].items() if path.endswith(('.png', '.ora'))}
    require(acceptance['approved_artwork'] == artwork and len(artwork) == 57, 'Complete accepted artwork coverage')
    capture = StringIO()
    with redirect_stdout(capture):
        authoring_checks()
    report = json.loads(capture.getvalue())
    report.update({'status': 'accepted external tiling feasibility package passes scoped checks',
                   'approved_artwork_files': len(artwork), 'oriented_corner_port_checks': check_all_oriented_ports(),
                   'owner_acceptance_sha256': digest(ROOT / 'owner-acceptance.json'),
                   'review_manifest_sha256': digest(ROOT / 'review-manifest.json'),
                   'runtime_promotion': 'not authorized by artwork acceptance'})
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()
