#!/usr/bin/env python3
"""Bind the approved Square–Southern Road asset/master/imports and runtime captures."""

import argparse
import hashlib
import io
import json
import runpy
import zipfile
from xml.etree import ElementTree

from PIL import Image

from check_square_southern_road_path_feasibility import validate as validate_archived
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require

helpers = runpy.run_path(str(ROOT / "scripts/validate-runtime-terrain.py"))
HASH = "4225a6c49fd7a2c5fbcd2ec21b6190f537f6583df74fb80d39d885ca67b194d5"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba(path):
    with Image.open(path) as image:
        require(image.mode == "RGBA", f"not RGBA: {path}")
        return image.copy()


def validate(check_captures=False, check_promotion_scope=False):
    validate_archived()  # The external proof's optional scene baseline is superseded.
    manifest = json.loads((PROOF / "asset-manifest.json").read_text())
    assets = [a for a in manifest["assets"] if a["id"] == "terrain.square_southern_road_path"]
    require(len(assets) == 1, "expected one Square–Southern Road asset")
    asset = assets[0]
    require(asset["export"] == "exports/terrain/square-southern-road-path.png"
            and asset["source"] == "tools/build_square_southern_road_path_correction.py"
            and asset["pivot"] == {"x": 260, "y": 208}
            and asset["master_dimensions_pixels"] == {"width": 1040, "height": 832, "scale": 2}
            and asset["export_rect_pixels"] == {"x": 0, "y": 0, "width": 520, "height": 416}
            and asset["logical_rect"] == {"x": 0, "y": 0, "width": 520, "height": 416}
            and asset["contact_baseline"] == 416 and asset["godot_import_proof"] is True,
            "Square–Southern Road manifest geometry/provenance changed")
    export = PROOF / asset["export"]
    require(digest(export) == asset["sha256"] == HASH, "accepted export bytes changed")
    accepted = rgba(REVIEW / "square-southern-road-path-correction-overlay.png")
    require(rgba(export).tobytes() == accepted.tobytes(), "accepted pixels changed")
    provenance = manifest["square_southern_road_path_provenance"]
    require(provenance == {
        "accepted_proof": "review/square-southern-road-path-correction-overlay.png",
        "accepted_proof_sha256": HASH,
        "master": "masters/terrain-square-southern-road-path.ora",
        "source_asset": "terrain.straight_path",
        "source_sha256": "3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd",
        "world_origin": [1440, 1024], "world_position": [1700, 1232],
        "heading_degrees": 144.462322, "endpoint": [1500, 1400], "overlap_columns": 16,
        "import_fix_alpha_border": False,
        "master_scaling": "nearest-neighbour 2x of exact accepted export",
    }, "Square–Southern Road provenance changed")
    master = accepted.resize((1040, 832), Image.Resampling.NEAREST)
    with zipfile.ZipFile(PROOF / provenance["master"]) as archive:
        merged = rgba(io.BytesIO(archive.read("mergedimage.png")))
        require(merged.size == master.size and merged.tobytes() == master.tobytes(),
                "master flattened pixels changed")
        stack = ElementTree.fromstring(archive.read("stack.xml"))
        require((stack.get("w"), stack.get("h")) == ("1040", "832"), "master geometry changed")
        layers = list(stack.find("stack"))
        require(len(layers) == 3, "master layer count changed")
        for layer, name, y, box in zip(layers[:2],
                                     ["continuation clipped at Southern Road endpoint", "exact junction overlap columns"],
                                     [0, 0], [(0, 0, 1008, 832), (1008, 0, 1040, 832)]):
            image = rgba(io.BytesIO(archive.read(layer.get("src"))))
            require(layer.get("name") == name and layer.get("visibility") == "visible"
                    and layer.get("opacity") == "1.0" and layer.get("composite_op") == "svg:src-over"
                    and (layer.get("x"), layer.get("y")) == (str(box[0]), str(y))
                    and image.size == master.crop(box).size
                    and image.tobytes() == master.crop(box).tobytes(), "editable layer changed")
        from build import render_path_leg

        reference = render_path_leg(rgba(ROOT / "game/art/runtime/terrain/straight-path.png"),
                                    (520, 416), (760, -124), 144.462322, (700**2 + 500**2)**0.5)
        require(layers[2].get("visibility") == "hidden"
                and layers[2].get("name") == "unclipped junction-relative continuation reference"
                and (layers[2].get("x"), layers[2].get("y")) == ("0", "0")
                and layers[2].get("opacity") == "1.0"
                and layers[2].get("composite_op") == "svg:src-over"
                and rgba(io.BytesIO(archive.read(layers[2].get("src")))).tobytes()
                == reference.resize((1040, 832), Image.Resampling.NEAREST).tobytes(),
                "unclipped master reference changed")
    uids = []
    for directory in ["runtime", "direction-a-import-proof"]:
        texture = ROOT / f"game/art/{directory}/terrain/square-southern-road-path.png"
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
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/square-southern-road-path.png" id="427_runtime_square_southern_road_path"]\n'
    binding = 'SquareSouthernRoadPathTexture = ExtResource("427_runtime_square_southern_road_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, "ordinary scene ownership changed")
    if check_promotion_scope:
        restored = scene.replace(resource, "").replace(binding, "").replace(
            "[gd_scene load_steps=344 format=3]", "[gd_scene load_steps=343 format=3]", 1)
        require(hashlib.sha256(restored.encode()).hexdigest()
                == "28ce71db623e7d85caf3e9717d577d869c886854f837e00594db9082b0b09180",
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
                runtime_path = REVIEW / f"square-southern-road-path-runtime-{zoom}x-{frame}.png"
                proof_path = REVIEW / f"square-southern-road-path-correction-{zoom}x-{frame}.png"
                require(digest(runtime_path) == digest(proof_path),
                        f"runtime PNG bytes differ from accepted {zoom}x {frame} proof")
                runtime = rgba(runtime_path)
                proof = rgba(proof_path)
                require(runtime.size == (1280, height) and runtime.tobytes() == proof.tobytes(),
                        f"runtime differs from accepted {zoom}x {frame} proof")
    print("Square–Southern Road promotion valid: exact approved export/master/imports and scene ownership.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-captures", action="store_true")
    parser.add_argument("--check-promotion-scope", action="store_true")
    args = parser.parse_args()
    validate(args.check_captures, args.check_promotion_scope)
