#!/usr/bin/env python3
"""Rebuild the complete production packet in a disposable tree and require exact parity."""

import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[3]
PRODUCTION = Path('art-direction/production-proof/corrected')


def sha(path):
    """Bind complete file bytes, including editable archive metadata."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Preserve live files and compare all production outputs and manifest semantics."""
    with tempfile.TemporaryDirectory(prefix='channel-rebuild-') as directory:
        copy = Path(directory)
        ignore = shutil.ignore_patterns('__pycache__', '.DS_Store', '.godot', 'bin', 'obj')
        for folder in ('art-direction', 'game'):
            shutil.copytree(ROOT / folder, copy / folder, ignore=ignore)
        subprocess.run([sys.executable, str(copy / PRODUCTION / 'tools/build.py')], cwd=copy, check=True)
        checked = 0
        for folder in ('exports', 'masters', 'review'):
            for original in (ROOT / PRODUCTION / folder).rglob('*'):
                if original.is_file() and original.name != '.DS_Store':
                    candidate = copy / original.relative_to(ROOT)
                    if sha(original) != sha(candidate):
                        raise ValueError('rebuild changed ' + str(original.relative_to(ROOT)))
                    checked += 1
        manifest = PRODUCTION / 'asset-manifest.json'
        if json.loads((copy / manifest).read_text()) != json.loads((ROOT / manifest).read_text()):
            raise ValueError('manifest rebuild differs')
        print(f'PASS: full production builder reproduces all {checked} exports, masters and review files plus exact manifest')


if __name__ == '__main__':
    main()
