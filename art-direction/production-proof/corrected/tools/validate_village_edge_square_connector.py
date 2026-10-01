#!/usr/bin/env python3
"""Bind the accepted connector to its production master and bounded runtime promotion."""

import argparse
import hashlib
import io
import json
import runpy
import zipfile
from xml.etree import ElementTree

from PIL import Image

from check_village_edge_square_connector_alpha import validate as validate_archived
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require

runtime_helpers = runpy.run_path(str(ROOT / "scripts/validate-runtime-terrain.py"))
imported_cache_path = runtime_helpers["imported_cache_path"]
read_import_options = runtime_helpers["read_import_options"]
resource_path = runtime_helpers["resource_path"]

ASSET_ID = "terrain.village_edge_square_connector"
ACCEPTED_HASH = "fb517f9e360c33298ca21c4be60f22af949cb4bb0a3a58661780747b87c82899"
NAME = "village-edge-square-connector.png"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba(path):
    with Image.open(path) as image:
        require(image.mode == "RGBA", f"not an RGBA image: {path}")
        return image.copy()


def validate(check_captures=False, check_promotion_scope=False):
    validate_archived()  # The original scene baseline is intentionally superseded.
    accepted_path = REVIEW / "village-edge-square-connector-alpha-strip.png"
    require(digest(accepted_path) == ACCEPTED_HASH, "accepted connector proof changed")
    accepted = rgba(accepted_path)
    manifest = json.loads((PROOF / "asset-manifest.json").read_text())
    assets = [a for a in manifest["assets"] if a["id"] == ASSET_ID]
    require(len(assets) == 1, "expected exactly one connector asset")
    asset = assets[0]
    require(asset["export"] == f"exports/terrain/{NAME}"
            and asset["source"] == "tools/build_village_edge_square_connector_alpha.py"
            and asset["master_dimensions_pixels"] == {"width": 248, "height": 192, "scale": 2}
            and asset["pivot"] == {"x": 62, "y": 48}
            and asset["contact_baseline"] == 96, "connector manifest geometry/provenance changed")
    export = PROOF / asset["export"]
    require(rgba(export).size == (124, 96)
            and rgba(export).tobytes() == accepted.tobytes(), "export differs from accepted pixels")
    require(digest(export) == asset["sha256"], "connector export hash changed")
    provenance = manifest["village_edge_square_connector_provenance"]
    require(provenance == {
        "accepted_proof": "review/village-edge-square-connector-alpha-strip.png",
        "accepted_proof_sha256": ACCEPTED_HASH,
        "master": "masters/terrain-village-edge-square-connector.ora",
        "source_assets": ["terrain.straight_path", "terrain.village_square_junction"],
        "source_sha256": [
            "3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd",
            "0ba0b2cfa16466936710bb801409ac4c790058449e7d609b0df2cc96d772e2e3",
        ],
        "hidden_rgb_pixels": 84,
        "import_fix_alpha_border": False,
        "master_scaling": "nearest-neighbour 2x of exact accepted export",
    }, "connector provenance changed")
    master = accepted.resize((248, 192), Image.Resampling.NEAREST)
    with zipfile.ZipFile(PROOF / provenance["master"]) as archive:
        merged = rgba(io.BytesIO(archive.read("mergedimage.png")))
        require(merged.size == (248, 192) and merged.tobytes() == master.tobytes(),
                "master flattened pixels differ from the accepted 2x image")
        stack = ElementTree.fromstring(archive.read("stack.xml"))
        require((stack.get("w"), stack.get("h")) == ("248", "192"), "master geometry changed")
        layers = list(stack.find("stack"))
        require(len(layers) == 4, "connector editable layer count changed")
        expected = [
            ("exact east-port blend", (184, 0), (184, 0, 248, 192)),
            ("source-alpha eight-column west RGB blend", (0, 0), (0, 0, 16, 192)),
            ("original columns 8..91", (16, 0), (16, 0, 184, 192)),
        ]
        for layer, (name, origin, box) in zip(layers[:3], expected):
            require(layer.get("name") == name and layer.get("visibility") == "visible"
                    and layer.get("opacity") == "1.0"
                    and layer.get("composite_op") == "svg:src-over"
                    and (int(layer.get("x")), int(layer.get("y"))) == origin,
                    "editable connector layer metadata changed")
            image = rgba(io.BytesIO(archive.read(layer.get("src"))))
            require(image.size == master.crop(box).size
                    and image.tobytes() == master.crop(box).tobytes(), "editable band pixels changed")
        source = rgba(ROOT / "game/art/runtime/terrain/straight-path.png").crop(
            (0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270).resize(
                (248, 192), Image.Resampling.NEAREST)
        reference = layers[3]
        require(reference.get("name") == "approved straight crop reference"
                and reference.get("visibility") == "hidden"
                and rgba(io.BytesIO(archive.read(reference.get("src")))).tobytes() == source.tobytes(),
                "editable source reference changed")

    for directory in ["runtime", "direction-a-import-proof"]:
        texture = ROOT / f"game/art/{directory}/terrain/{NAME}"
        require(not texture.is_symlink() and digest(texture) == asset["sha256"],
                f"{directory} texture lost owned source parity")
        options = read_import_options(texture.with_suffix(".png.import"))
        cache = imported_cache_path(texture)
        require(options.get(("deps", "source_file")) == f'"{resource_path(texture)}"'
                and options.get(("remap", "path")) == f'"{cache}"'
                and options.get(("deps", "dest_files")) == f'["{cache}"]'
                and options.get(("params", "compress/mode")) == "0"
                and options.get(("params", "mipmaps/generate")) == "false"
                and options.get(("params", "process/fix_alpha_border")) == "false"
                and options.get(("params", "process/premult_alpha")) == "false"
                and options.get(("remap", "importer")) == '"texture"'
                and all(options.get(("params", f"process/channel_remap/{channel}")) == str(index)
                        for index, channel in enumerate(["red", "green", "blue", "alpha"]))
                and options.get(("params", "process/size_limit")) == "0",
                f"{directory} import paths or lossless settings changed")
        uid = options.get(("remap", "uid"))
        require(uid is not None and uid.startswith('"uid://') and uid.endswith('"') and sum(
            read_import_options(sidecar).get(("remap", "uid")) == uid
            for sidecar in (ROOT / "game/art").rglob("*.png.import")
        ) == 1, "connector import UID is not unique")

    scene = (ROOT / "game/Main.tscn").read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/village-edge-square-connector.png" id="424_runtime_village_edge_square_connector"]\n'
    binding = 'VillageEdgeSquareConnectorTexture = ExtResource("424_runtime_village_edge_square_connector")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, "connector scene ownership changed")
    if check_promotion_scope:
        restored = scene.replace(resource, "").replace(binding, "").replace(
            "[gd_scene load_steps=341 format=3]", "[gd_scene load_steps=340 format=3]", 1)
        require(hashlib.sha256(restored.encode()).hexdigest()
                == "ab322e2a91acbadf029831893836c4e14e316235cdcbfce0494176f6262c5127",
                "Main.tscn changed outside the two approved connector lines")
        navigation = (ROOT / "game/GreyboxNavigation.cs").read_text()
        tail = navigation[navigation.index("    private static Vector2 FindSquareBoundaryExit("):]
        enter = navigation[navigation.index("    public override void _EnterTree()"):
                           navigation.index("    /// <inheritdoc />", navigation.index("    public override void _EnterTree()"))]
        require(hashlib.sha256(tail.encode()).hexdigest()
                == "0199b0f4edac8a547876ee3754ce82902f625bce46ffbc716d4a75b071521998"
                and hashlib.sha256(enter.encode()).hexdigest()
                == "381762ef06a0c3a7f14fe5373b4f7a5ed71f92038bf029ad47b7234d12097513",
                "physical/navigation authority changed")
        for name, expected in {
            "OpeningRegionLayoutGeometry.cs": "92524c8084675dd76de3105a2e4ed92f7b80b097c8d344593089428ebf277e57",
            "OpeningRegionVillageTransitionGeometry.cs": "ce235645a5c33c60809e74ec4f3d9b705bef09d1f8114d4cc7e0cc7ab876299f",
        }.items():
            require(digest(ROOT / "game" / name) == expected, f"approved geometry changed: {name}")
    if check_captures:
        for zoom in ["0.8", "1.0", "2.0"]:
            for frame, height in [("wide", 720), ("tall", 827)]:
                runtime = rgba(REVIEW / f"village-edge-square-connector-runtime-{zoom}x-{frame}.png")
                proof = rgba(REVIEW / f"village-edge-square-connector-alpha-{zoom}x-{frame}.png")
                require(runtime.size == (1280, height) and runtime.tobytes() == proof.tobytes(),
                        f"ordinary scene differs from accepted {zoom}x {frame} preview")
    print("Village Edge–Square runtime connector valid: accepted pixels, layered 2x master, owned imports.")
    if check_promotion_scope:
        print("Promotion scope valid: only connector binding/presentation changed; physical/navigation authority retained.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-captures", action="store_true")
    parser.add_argument("--check-promotion-scope", action="store_true")
    args = parser.parse_args()
    validate(args.check_captures, args.check_promotion_scope)
