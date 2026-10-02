"""Preserve frozen path-proof build inputs while allowing approved production additions."""

import ast
import gzip
import hashlib

from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

BUILD_PATH = "art-direction/production-proof/corrected/tools/build.py"


def validate_input(relative, expected, check_live_baseline=False):
    """Check exact inputs; only production build additions may use the archived source."""
    live = (ROOT / relative).read_bytes()
    if relative != BUILD_PATH or check_live_baseline:
        require(hashlib.sha256(live).hexdigest() == expected, f"proof input changed: {relative}")
        return
    archived = gzip.decompress((REVIEW / "path-proof-build-source.py.gz").read_bytes())
    require(hashlib.sha256(archived).hexdigest() == expected, "archived path builder changed")
    # The frozen recipes use this self-contained renderer, which must remain byte-exact.
    def renderer(source):
        text = source.decode()
        matches = [node for node in ast.parse(text).body
                   if isinstance(node, ast.FunctionDef) and node.name == "render_path_leg"]
        require(len(matches) == 1, "expected one path renderer")
        return ast.get_source_segment(text, matches[0])

    require(renderer(live) == renderer(archived), "live path renderer differs from archived proof")
