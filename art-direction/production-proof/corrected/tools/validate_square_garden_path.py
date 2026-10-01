#!/usr/bin/env python3
"""Bind the approved Square–Garden asset/master/imports and runtime captures."""

import argparse
import hashlib
import io
import json
import runpy
import zipfile
from xml.etree import ElementTree

from PIL import Image

from check_square_garden_path_feasibility import validate as validate_archived
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require

helpers = runpy.run_path(str(ROOT / "scripts/validate-runtime-terrain.py"))
HASH = "04c9ea2ea7a609dadcfcff268dc445300104f3e466fc003b304252feb411c8de"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba(path):
    with Image.open(path) as image:
        require(image.mode == "RGBA", f"not RGBA: {path}")
        return image.copy()


def validate(check_captures=False, check_promotion_scope=False):
    validate_archived()  # The external proof's optional scene baseline is superseded.
    manifest = json.loads((PROOF / "asset-manifest.json").read_text())
    assets = [a for a in manifest["assets"] if a["id"] == "terrain.square_garden_path"]
    require(len(assets) == 1, "expected one Square–Garden asset")
    asset = assets[0]
    require(asset["export"] == "exports/terrain/square-garden-path.png"
            and asset["source"] == "tools/build_square_garden_path_correction.py"
            and asset["pivot"] == {"x": 100, "y": 110}
            and asset["master_dimensions_pixels"] == {"width": 400, "height": 440, "scale": 2}
            and asset["export_rect_pixels"] == {"x": 0, "y": 0, "width": 200, "height": 220}
            and asset["logical_rect"] == {"x": 0, "y": 0, "width": 200, "height": 220}
            and asset["contact_baseline"] == 220 and asset["godot_import_proof"] is True,
            "Square–Garden manifest geometry/provenance changed")
    export = PROOF / asset["export"]
    require(digest(export) == asset["sha256"] == HASH, "accepted export bytes changed")
    accepted = rgba(REVIEW / "square-garden-path-correction-overlay.png")
    require(rgba(export).tobytes() == accepted.tobytes(), "accepted pixels changed")
    provenance = manifest["square_garden_path_provenance"]
    require(provenance == {
        "accepted_proof": "review/square-garden-path-correction-overlay.png",
        "accepted_proof_sha256": HASH,
        "master": "masters/terrain-square-garden-path.ora",
        "source_asset": "terrain.straight_path",
        "source_sha256": "3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd",
        "world_origin": [2200, 1140], "world_position": [2300, 1250],
        "heading_degrees": 75.963757, "endpoint": [2300, 1300], "overlap_rows": 16,
        "import_fix_alpha_border": False,
        "master_scaling": "nearest-neighbour 2x of exact accepted export",
    }, "Square–Garden provenance changed")
    master = accepted.resize((400, 440), Image.Resampling.NEAREST)
    with zipfile.ZipFile(PROOF / provenance["master"]) as archive:
        merged = rgba(io.BytesIO(archive.read("mergedimage.png")))
        require(merged.size == master.size and merged.tobytes() == master.tobytes(),
                "master flattened pixels changed")
        stack = ElementTree.fromstring(archive.read("stack.xml"))
        require((stack.get("w"), stack.get("h")) == ("400", "440"), "master geometry changed")
        layers = list(stack.find("stack"))
        require(len(layers) == 3, "master layer count changed")
        for layer, name, y, box in zip(layers[:2],
                                     ["continuation clipped at Garden endpoint", "exact junction overlap"],
                                     [32, 0], [(0, 32, 400, 440), (0, 0, 400, 32)]):
            image = rgba(io.BytesIO(archive.read(layer.get("src"))))
            require(layer.get("name") == name and layer.get("visibility") == "visible"
                    and layer.get("opacity") == "1.0" and layer.get("composite_op") == "svg:src-over"
                    and (layer.get("x"), layer.get("y")) == ("0", str(y))
                    and image.size == master.crop(box).size
                    and image.tobytes() == master.crop(box).tobytes(), "editable layer changed")
        from build import render_path_leg

        reference = render_path_leg(rgba(ROOT / "game/art/runtime/terrain/straight-path.png"),
                                    (200, 220), (0, -240), 75.963757, (100**2 + 400**2)**0.5)
        require(layers[2].get("visibility") == "hidden"
                and layers[2].get("name") == "unclipped junction-relative continuation reference"
                and (layers[2].get("x"), layers[2].get("y")) == ("0", "0")
                and layers[2].get("opacity") == "1.0"
                and layers[2].get("composite_op") == "svg:src-over"
                and rgba(io.BytesIO(archive.read(layers[2].get("src")))).tobytes()
                == reference.resize((400, 440), Image.Resampling.NEAREST).tobytes(),
                "unclipped master reference changed")
    uids = []
    for directory in ["runtime", "direction-a-import-proof"]:
        texture = ROOT / f"game/art/{directory}/terrain/square-garden-path.png"
        require(not texture.is_symlink() and digest(texture) == HASH, "owned import bytes changed")
        options = helpers["read_import_options"](texture.with_suffix(".png.import"))
        cache = helpers["imported_cache_path"](texture)
        require(options.get(("deps", "source_file")) == f'"{helpers["resource_path"](texture)}"'
                and options.get(("remap", "path")) == f'"{cache}"'
                and options.get(("deps", "dest_files")) == f'["{cache}"]'
                and options.get(("remap", "importer")) == '"texture"'
                and options.get(("params", "compress/mode")) == "0"
                and options.get(("params", "mipmaps/generate")) == "false"
                and options.get(("params", "process/fix_alpha_border")) == "false"
                and options.get(("params", "process/premult_alpha")) == "false"
                and options.get(("params", "process/size_limit")) == "0"
                and all(options.get(("params", f"process/channel_remap/{c}")) == str(i)
                        for i, c in enumerate(["red", "green", "blue", "alpha"])),
                "import path or lossless/alpha settings changed")
        uid = options.get(("remap", "uid"))
        require(uid is not None and uid.startswith('"uid://') and uid.endswith('"')
                and sum(helpers["read_import_options"](p).get(("remap", "uid")) == uid
                        for p in (ROOT / "game/art").rglob("*.png.import")) == 1,
                "import UID is missing or not unique")
        uids.append(uid)
    require(len(set(uids)) == 2, "imports share an identity")
    scene = (ROOT / "game/Main.tscn").read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/square-garden-path.png" id="425_runtime_square_garden_path"]\n'
    binding = 'SquareGardenPathTexture = ExtResource("425_runtime_square_garden_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, "ordinary scene ownership changed")
    if check_promotion_scope:
        restored = scene.replace(resource, "").replace(binding, "").replace(
            "[gd_scene load_steps=342 format=3]", "[gd_scene load_steps=341 format=3]", 1)
        require(hashlib.sha256(restored.encode()).hexdigest()
                == "7b65792c0ba787d971ceb5c7f0491a5df9c1ea6aee14c86918537da0ad0a3a4d",
                "Main scene changed beyond approved binding")
        nav = (ROOT / "game/GreyboxNavigation.cs").read_text()
        tail = nav[nav.index("    private static Vector2 FindSquareBoundaryExit("):]
        enter = nav[nav.index("    public override void _EnterTree()"):
                    nav.index("    /// <inheritdoc />", nav.index("    public override void _EnterTree()"))]
        require(hashlib.sha256(tail.encode()).hexdigest()
                == "0199b0f4edac8a547876ee3754ce82902f625bce46ffbc716d4a75b071521998"
                and hashlib.sha256(enter.encode()).hexdigest()
                == "381762ef06a0c3a7f14fe5373b4f7a5ed71f92038bf029ad47b7234d12097513",
                "physical/navigation authority changed")
        for name, expected in {
            "OpeningRegionLayoutGeometry.cs": "92524c8084675dd76de3105a2e4ed92f7b80b097c8d344593089428ebf277e57",
            "OpeningRegionVillageTransitionGeometry.cs": "ce235645a5c33c60809e74ec4f3d9b705bef09d1f8114d4cc7e0cc7ab876299f",
        }.items():
            require(digest(ROOT / "game" / name) == expected, f"retained geometry changed: {name}")
    if check_captures:
        for zoom in ["0.8", "1.0", "2.0"]:
            for frame, height in [("wide", 720), ("tall", 827)]:
                runtime = rgba(REVIEW / f"square-garden-path-runtime-{zoom}x-{frame}.png")
                proof = rgba(REVIEW / f"square-garden-path-correction-{zoom}x-{frame}.png")
                require(runtime.size == (1280, height) and runtime.tobytes() == proof.tobytes(),
                        f"runtime differs from accepted {zoom}x {frame} proof")
    print("Square–Garden promotion valid: exact approved export/master/imports and scene ownership.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-captures", action="store_true")
    parser.add_argument("--check-promotion-scope", action="store_true")
    args = parser.parse_args()
    validate(args.check_captures, args.check_promotion_scope)
