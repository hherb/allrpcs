#!/usr/bin/env python3
"""Bind the approved Southern Road–Wagon asset/master/imports and runtime captures."""

import argparse
import hashlib
import io
import json
import runpy
import zipfile
from xml.etree import ElementTree

from PIL import Image

from check_southern_road_wagon_path_trim import validate as validate_archived
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require

helpers = runpy.run_path(str(ROOT / "scripts/validate-runtime-terrain.py"))
HASH = "34f1c1c28c5ee9f654953ba4d07ef2ac9d1fe6fe3f206fd04e68022395741799"


def digest(path):
    """Return the exact owned file digest."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba(path):
    """Load and copy one RGBA image without normalizing its pixels."""
    with Image.open(path) as image:
        require(image.mode == "RGBA", f"not RGBA: {path}")
        return image.copy()


def validate(check_captures=False, check_promotion_scope=False):
    """Verify the accepted promotion and optional session-specific scope/captures."""
    validate_archived()  # Retain archived evidence; the approved promotion supersedes its live baseline.
    manifest = json.loads((PROOF / "asset-manifest.json").read_text())
    assets = [a for a in manifest["assets"] if a["id"] == "terrain.southern_road_wagon_path"]
    require(len(assets) == 1, "expected one Southern Road–Wagon asset")
    asset = assets[0]
    require(asset["export"] == "exports/terrain/southern-road-wagon-path.png"
            and asset["source"] == "tools/build_southern_road_wagon_path_trim.py"
            and asset["pivot"] == {"x": 280, "y": 216}
            and asset["master_dimensions_pixels"] == {"width": 1120, "height": 864, "scale": 2}
            and asset["export_rect_pixels"] == {"x": 0, "y": 0, "width": 560, "height": 432}
            and asset["logical_rect"] == {"x": 0, "y": 0, "width": 560, "height": 432}
            and asset["contact_baseline"] == 432 and asset["godot_import_proof"] is True,
            "Southern Road–Wagon manifest geometry/provenance changed")
    export = PROOF / asset["export"]
    require(digest(export) == asset["sha256"] == HASH, "accepted export bytes changed")
    accepted = rgba(REVIEW / "southern-road-wagon-path-trim-overlay.png")
    require(rgba(export).tobytes() == accepted.tobytes(), "accepted pixels changed")
    provenance = manifest["southern_road_wagon_path_provenance"]
    require(provenance == {
        "accepted_proof": "review/southern-road-wagon-path-trim-overlay.png",
        "accepted_proof_sha256": HASH,
        "master": "masters/terrain-southern-road-wagon-path.ora",
        "source_asset": "terrain.straight_path",
        "source_sha256": "3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd",
        "recipe_sha256": "b1ceeff545529512f6116cfb1c4c4b23903a9d43730879352e91430319dc45f5",
        "world_origin": [1400, 1280], "world_position": [1680, 1496],
        "road_points": [[1500, 1400], [1820, 1580], [1900, 1640]],
        "backward_overlap_pixels": 48, "incoming_extension_pixels": 16,
        "master_partition_column": 360, "import_fix_alpha_border": False,
        "master_scaling": "nearest-neighbour 2x of exact accepted export",
    }, "Southern Road–Wagon provenance changed")
    master = accepted.resize((1120, 864), Image.Resampling.NEAREST)
    with zipfile.ZipFile(PROOF / provenance["master"]) as archive:
        merged = rgba(io.BytesIO(archive.read("mergedimage.png")))
        require(merged.size == master.size and merged.tobytes() == master.tobytes(),
                "master flattened pixels changed")
        stack = ElementTree.fromstring(archive.read("stack.xml"))
        require((stack.get("w"), stack.get("h")) == ("1120", "864"), "master geometry changed")
        layers = list(stack.find("stack"))
        require(len(layers) == 3, "master layer count changed")
        composed = Image.new("RGBA", master.size)
        for layer, name, box in zip(layers[:2],
                                   ["accepted bent road continuation", "accepted junction and incoming tip"],
                                   [(360, 0, 1120, 864), (0, 0, 360, 864)]):
            image = rgba(io.BytesIO(archive.read(layer.get("src"))))
            require(layer.get("name") == name and layer.get("visibility") == "visible"
                    and layer.get("opacity") == "1.0" and layer.get("composite_op") == "svg:src-over"
                    and (layer.get("x"), layer.get("y")) == (str(box[0]), "0")
                    and image.size == master.crop(box).size
                    and image.tobytes() == master.crop(box).tobytes(), "editable layer changed")
            composed.alpha_composite(image, (box[0], 0))
        require(composed.tobytes() == master.tobytes(), "visible layers do not reconstruct accepted pixels")
        reference = rgba(REVIEW / "southern-road-wagon-path-correction-overlay.png").resize(
            (1120, 864), Image.Resampling.NEAREST)
        require(layers[2].get("visibility") == "hidden"
                and layers[2].get("name") == "untrimmed external correction reference"
                and (layers[2].get("x"), layers[2].get("y")) == ("0", "0")
                and layers[2].get("opacity") == "1.0"
                and layers[2].get("composite_op") == "svg:src-over"
                and rgba(io.BytesIO(archive.read(layers[2].get("src")))).tobytes()
                == reference.tobytes(), "untrimmed master reference changed")
    uids = []
    for directory in ["runtime", "direction-a-import-proof"]:
        texture = ROOT / f"game/art/{directory}/terrain/southern-road-wagon-path.png"
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
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/southern-road-wagon-path.png" id="428_runtime_southern_road_wagon_path"]\n'
    binding = 'SouthernRoadWagonPathTexture = ExtResource("428_runtime_southern_road_wagon_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, "ordinary scene ownership changed")
    if check_promotion_scope:
        restored = scene.replace(resource, "").replace(binding, "").replace(
            "[gd_scene load_steps=345 format=3]", "[gd_scene load_steps=344 format=3]", 1).replace(
            "points = PackedVector2Array(572, 460, 1500, 1400)\n",
            "points = PackedVector2Array(572, 460, 1500, 1400, 1820, 1580, 1900, 1640)\n", 1)
        require(hashlib.sha256(restored.encode()).hexdigest()
                == "9e6db17fd63d751b0d6f8b5bb0b314b2d0eba9f313cc19ecb24abe56ed3ff186",
                "Main scene changed beyond approved texture binding and presentation fill")
        retained_manifest = {key: value for key, value in manifest.items()
                             if key != "southern_road_wagon_path_provenance"}
        retained_manifest["assets"] = [entry for entry in manifest["assets"]
                                       if entry["id"] != "terrain.southern_road_wagon_path"]
        require(hashlib.sha256((json.dumps(retained_manifest, indent=2) + "\n").encode()).hexdigest()
                == "f695d93546ba5b1b2c8339ba9cd133587a4f7dc46aecd88ce529a7aa5936fbf7",
                "manifest changed beyond the named promotion")
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
            "OpeningRegionSouthernRoadWagonGeometry.cs": "ce7a4508af1c9fac446b34f35e8d93ea67a5a8257dfad7a3a3574b7f639f3bdc",
            "OpeningRegionSouthernRoadWagonSceneSource.cs": "a0d8e4952163f54a76d01232511526ad6688975dd0186545404de1ffa7b71bb8",
            "project.godot": "0ce582324fb3bde4781052556f934f5c9bf7d3dc8ff58ad6b070ccf3db459c77",
        }.items():
            require(digest(ROOT / "game" / name) == expected, f"retained geometry changed: {name}")
    if check_captures:
        for zoom in ["0.8", "1.0", "2.0"]:
            for frame, height in [("wide", 720), ("tall", 827)]:
                runtime_path = REVIEW / f"southern-road-wagon-path-runtime-{zoom}x-{frame}.png"
                proof_path = REVIEW / f"southern-road-wagon-path-trim-{zoom}x-{frame}.png"
                require(digest(runtime_path) == digest(proof_path),
                        f"runtime PNG bytes differ from accepted {zoom}x {frame} proof")
                runtime = rgba(runtime_path)
                proof = rgba(proof_path)
                require(runtime.size == (1280, height) and runtime.tobytes() == proof.tobytes(),
                        f"runtime differs from accepted {zoom}x {frame} proof")
    print("Southern Road–Wagon promotion valid: exact approved export/master/imports and scene ownership.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-captures", action="store_true")
    parser.add_argument("--check-promotion-scope", action="store_true")
    args = parser.parse_args()
    validate(args.check_captures, args.check_promotion_scope)
