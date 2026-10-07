#!/usr/bin/env python3
"""Validate the retained audio snapshot before and after its files enter Git."""

import argparse
import hashlib
import json
from pathlib import Path
import runpy
import subprocess

ROOT = Path(__file__).resolve().parents[1]
DOCUMENTS = {'CONTINUATION.json', 'HANDOVER.md', 'ROADMAP.md'}
PREDECESSOR_CHANGES = DOCUMENTS | {
    'game/Main.tscn', 'game/OptionalOverlayVisibilityController.cs', 'game/project.godot',
}


def git(*arguments):
    """Read Git state without modifying the index or checkout."""
    return subprocess.check_output(['git', *arguments], cwd=ROOT)


def digest(path):
    """Hash a retained repository file."""
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def require(condition, message):
    """Reject changes outside the reviewed publication snapshot."""
    if not condition:
        raise ValueError(message)


def main():
    """Check protected predecessors and reviewed additions, optionally running product quality."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--quality', action='store_true', help='Run the retained product checks after validation')
    arguments = parser.parse_args()
    publication = json.loads((ROOT / 'audio-direction/publication-authority.json').read_text())
    binding = publication['verification_binding']
    require(digest(binding['path']) == binding['sha256'], 'Retained verification changed')
    verification = json.loads((ROOT / binding['path']).read_text())
    for entry in verification['source_inputs'] + verification['retained_evidence']:
        require(digest(entry['path']) == entry['sha256'], 'Reviewed input changed: ' + entry['path'])
        if 'archived_path' in entry:
            require(digest(entry['archived_path']) == entry['sha256'], 'Archived input changed: ' + entry['path'])
    for name in ('authority', 'baseline'):
        entry = verification[name]
        require(digest(entry['path']) == entry['sha256'], 'Retained binding changed: ' + name)
    baseline = json.loads((ROOT / verification['baseline']['path']).read_text())
    revision = baseline['commit']
    predecessors = set(git('ls-tree', '-r', '--name-only', revision).decode().splitlines())
    changed = set(git('diff', '--name-only', revision).decode().splitlines())
    changed |= set(git('ls-files', '--others', '--exclude-standard').decode().splitlines())
    require(changed & predecessors <= PREDECESSOR_CHANGES, 'Unapproved predecessor changes')
    require(changed - predecessors <= set(publication['publication_paths']), 'Unreviewed new publication files')
    require('game/project.godot' not in publication['publication_paths'], 'Editor rewrite entered publication')
    prior_main = git('show', revision + ':game/Main.tscn')
    require(hashlib.sha256(prior_main).hexdigest() == baseline['main_sha256'], 'Predecessor Main changed')
    prior_overlay = git('show', revision + ':game/OptionalOverlayVisibilityController.cs')
    require(hashlib.sha256(prior_overlay).hexdigest() == baseline['optional_overlays_sha256'], 'Predecessor overlay changed')
    # Preserve the frozen comparator; its original whole-diff check assumed additions were untracked.
    comparator = runpy.run_path(str(ROOT / 'scripts/validate-audio-runtime.py'))['validate_main']
    live = (ROOT / 'game/Main.tscn').read_text()
    comparator(live, prior_main)
    for mutation in (live + '\n[node name="DuplicateMixer" type="Node" parent="."]\n',
                     live.replace('id="audio_settings"', 'id="renamed_audio"', 1),
                     live.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)', 1)):
        require(mutation != live, 'Ineffective boundary probe')
        try:
            comparator(mutation, prior_main)
        except ValueError:
            continue
        raise ValueError('Boundary accepted a changed composition')
    # A clean clone retains the committed predecessor; this checkout keeps its original editor rewrite.
    project_hashes = {baseline['preserved_editor_project_sha256'],
                      hashlib.sha256(git('show', revision + ':game/project.godot')).hexdigest()}
    require(digest('game/project.godot') in project_hashes, 'Project changed beyond retained editor exception')
    subprocess.run(['python3', 'scripts/check-continuation.py'], cwd=ROOT, check=True)
    print('PASS: reviewed audio hashes, protected predecessors, publication membership and 3 rejection probes', flush=True)
    if arguments.quality:
        # Keep the recorded script bytes intact; replace only the superseded boundary invocation in memory.
        script = (ROOT / 'scripts/quality-audio.sh').read_text()
        boundary = 'python3 scripts/validate-audio-runtime.py\n'
        require(script.count(boundary) == 1, 'Unexpected retained quality entry point')
        subprocess.run(['sh', '-c', script.replace(boundary, '', 1), str(ROOT / 'scripts/quality-audio.sh')],
                       cwd=ROOT, check=True)


if __name__ == '__main__':
    main()
