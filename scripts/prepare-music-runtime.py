#!/usr/bin/env python3
"""Install two entire accepted music WAVs and retained contributor notices after exact approval."""
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def digest(path):
    """Hash full file bytes without re-encoding accepted audio."""
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()

def main():
    """Validate the recorded grant before writing the finite seven-resource package."""
    scope = json.loads((ROOT / 'audio-direction/music-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/music-runtime-authority.json').read_text())
    assert authority['status'] == 'exact_scope_approved' and authority['grant']['two_byte_exact_WAV_installations']
    for row in authority['bindings']:
        assert digest(row['path']) == row['sha256'], row['path']
    destination = ROOT / 'game/assets/audio/music'
    destination.mkdir(parents=True, exist_ok=True)
    credits = ['Optional music sources', 'Runtime installation: whole accepted WAVs copied byte-identically; no further edits.']
    for clip in scope['clips']:
        assert digest(clip['accepted']['path']) == clip['accepted']['sha256']
        shutil.copyfile(ROOT / clip['accepted']['path'], ROOT / clip['proposed_runtime_path'])
        assert digest(clip['proposed_runtime_path']) == clip['accepted']['sha256']
        source = clip['source']
        credits += ['', f"{clip['id']}: {source['title']} by {source['author']}",
                    f"Source: {source['catalogue_url']}", f"License: {source['license']} - {source['license_url']}",
                    'Accepted edits: contiguous 30-55s excerpt, two-second edge fades and uniform gain; see provenance.json.',
                    'M02 additionally: original 16kHz mono resampled to 48kHz and centred in stereo; no restored bandwidth.'
                    if clip['id'] == 'M02' else 'M01 retains its original stereo mapping.']
    notice = scope['rights']['license_notice']
    assert digest(notice['path']) == notice['sha256']
    shutil.copyfile(ROOT / notice['path'], destination / 'CC0-1.0-deed.html')
    (destination / 'CREDITS.txt').write_text('\n'.join(credits) + '\n')
    (destination / 'provenance.json').write_text(json.dumps({'schema_version': 1, 'authority': 'audio-direction/music-runtime-authority.json',
        'copy_contract': 'whole accepted WAV bytes; no extraction/generation/source edit', 'clips': scope['clips'],
        'licence_notice': notice}, indent=2) + '\n')
    print('PASS: two byte-exact accepted music WAVs and current contributor/CC0 notices installed')

if __name__ == '__main__':
    main()
