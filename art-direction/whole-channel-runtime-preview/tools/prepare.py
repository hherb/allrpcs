#!/usr/bin/env python3
"""Prepare a disposable game copy and independent lossless preview imports."""

import argparse
import json
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]


def prepare(destination):
    """Never write tracked game files or reuse production preview resource identities."""
    destination = Path(destination).resolve()
    if not str(destination).startswith('/private/tmp/') and not str(destination).startswith('/tmp/'):
        raise ValueError('disposable destination must be under /tmp')
    if destination.exists():
        raise ValueError('destination already exists')
    destination.mkdir(parents=True)
    subprocess.run(['rsync', '-a', '--exclude=.DS_Store', '--exclude=logs',
                    str(ROOT / 'game') + '/', str(destination / 'game') + '/'], check=True)
    preview = destination / 'game/preview'
    preview.mkdir()
    manifest = json.loads((HERE / 'manifest.json').read_text())
    shutil.copy2(HERE / 'manifest.json', preview / 'manifest.json')
    shutil.copy2(HERE / 'tools/capture.gd', preview / 'capture.gd')
    (preview / 'config.json').write_text(json.dumps({'output': str(HERE)}) + '\n')
    for layer in manifest['layers']:
        name = layer['filename']
        shutil.copy2(HERE / 'exports' / name, preview / name)
        # Godot creates a fresh UID/cache path; only params are inherited from the template.
        template = (ROOT / 'game/art/runtime/terrain/clearing-closed-forest-path.png.import').read_text()
        params = '[params]' + template.split('[params]', 1)[1]
        (preview / (name + '.import')).write_text('[remap]\n\nimporter="texture"\ntype="CompressedTexture2D"\n\n' + params)
    print(destination / 'game')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('destination')
    prepare(parser.parse_args().destination)
