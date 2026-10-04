#!/usr/bin/env python3
"""Inventory authored terrain texture owners without proposing material geography."""

import argparse
import hashlib
import json
import re

from audit import ROOT, REPO, PRODUCTION, digest, inputs, read_json, require

SOURCES = [
    'game/Main.tscn', 'game/GreyboxNavigation.cs',
    'game/OpeningRegionWoodlandClearingSceneSource.cs',
    'game/OpeningRegionStreamLaundrySceneSource.cs', 'game/WholeChannelPresentation.cs',
    'game/RuntimeTerrainGroundSmokeCheck.cs', 'game/RuntimeGardenFoliageSmokeCheck.cs',
    'art-direction/production-proof/corrected/tools/build.py',
    'art-direction/world-layout-proposal/layout-proposal.json',
    'art-direction/approved-external-compositions/channel/manifest.json',
    'art-direction/approved-external-compositions/forest/manifest.json',
    'art-direction/whole-channel-runtime-promotion/owner-acceptance.json',
    'art-direction/forest-path-runtime-promotion/owner-runtime-acceptance.json',
]


def report():
    """Bind scene assignments, runtime bytes, authored recipes and explicit source references."""
    fields, overlays, _, _ = inputs()
    bound = {path: digest(REPO / path) for path in SOURCES}
    scene = (REPO / 'game/Main.tscn').read_text()
    resources = dict((resource_id, path) for path, resource_id in re.findall(
        r'\[ext_resource type="Texture2D" path="([^"]+)" id="([^"]+)"\]', scene))
    manifest = read_json(PRODUCTION / 'asset-manifest.json')
    terrain_by_name = {a['export'].split('/')[-1]: a for a in manifest['assets']
                       if a['id'].startswith('terrain.')}
    assignments = []
    node = None
    for line_number, line in enumerate(scene.splitlines(), 1):
        header = re.match(r'\[node name="([^"]+)" type="([^"]+)"(?: parent="([^"]+)")?', line)
        if header:
            name, kind, parent = header.groups()
            node = {'name': name, 'type': kind, 'parent': parent}
        elif line.startswith('['):
            node = None
        match = re.match(r'(\w+) = ExtResource\("([^"]+)"\)', line)
        if node is None or match is None:
            continue
        property_name, resource_id = match.groups()
        path = resources.get(resource_id, '')
        if not property_name.lower().endswith('texture') or '/art/runtime/terrain/' not in path:
            continue
        runtime = REPO / 'game' / path.removeprefix('res://')
        actual = digest(runtime)
        bound[str(runtime.relative_to(REPO))] = actual
        asset = terrain_by_name.get(runtime.name)
        require(asset is not None and actual == asset['sha256'], f'Runtime/manifest mismatch: {runtime}')
        assignments.append({**node, 'property': property_name, 'resource_path': path,
                            'scene_line': line_number, 'manifest_id': asset['id'], 'sha256': actual})
    require(any(a['name'] == 'Ground' and a['manifest_id'] == 'terrain.quiet_moss'
                for a in assignments), 'Expected retained quiet-moss Ground owner')
    # Bind the complete authored source scan compactly, without claiming dynamic-load impossibility.
    tokens = ['residual-ground-batch', 'residual-ground-tiling-proof']
    tokens += [a['export'].split('/')[-1] for a in fields + overlays]
    paths = sorted(p for p in (REPO / 'game').rglob('*')
                   if p.suffix in ('.cs', '.tscn', '.tres') and
                   not any(part in ('.godot', 'bin', 'obj') for part in p.relative_to(REPO).parts))
    corpus = hashlib.sha256()
    references = []
    for path in paths:
        relative = str(path.relative_to(REPO))
        corpus.update((relative + '\0' + digest(path) + '\n').encode())
        for line_number, line in enumerate(path.read_text().splitlines(), 1):
            if any(token in line for token in tokens):
                references.append({'path': relative, 'line': line_number, 'text': line.strip()})
    return {'schema_version': 1, 'date': '2026-10-04', 'timezone': 'Australia/Sydney',
            'scope': 'Read-only authored terrain interface scope; no new material geography',
            'compatibility_audit_sha256': digest(ROOT / 'audit.json'),
            'dressing_audit_sha256': digest(ROOT / 'dressing.json'),
            'inputs': dict(sorted(bound.items())), 'terrain_texture_assignments': assignments,
            'explicit_new_field_reference_scan': {'file_count': len(paths), 'corpus_sha256': corpus.hexdigest(),
                                                 'tokens': tokens, 'matches': references},
            'conclusion': ('Explicit new-field references need scoped review.' if references else
                           'The inspected authored sources select no explicit new-field interface.') +
                          ' Direct colour differences do not establish a required new connector. '
                          'Dynamic resource synthesis and GPU appearance are outside this read-only scan.',
            'next_boundary': 'Owner selects a new field and intended interface/composition, or explicitly requests a placement proposal, before connector artwork or named runtime promotion scope.'}


def main():
    """Write this source-bound report or compare it without mutating existing evidence."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    result = report()
    if args.check:
        require(read_json(ROOT / 'interfaces.json') == result, 'Authored interface evidence changed')
    else:
        (ROOT / 'interfaces.json').write_text(json.dumps(result, indent=2) + '\n')
    print(f'PASS: {len(result["terrain_texture_assignments"])} terrain assignments; '
          f'{result["explicit_new_field_reference_scan"]["file_count"]} authored source files scanned; '
          f'{len(result["explicit_new_field_reference_scan"]["matches"])} explicit new-field references.')


if __name__ == '__main__':
    main()
