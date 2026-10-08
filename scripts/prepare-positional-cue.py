#!/usr/bin/env python3
"""Extract only the owner-authorized PCM interval from the accepted Dialogue component."""
import hashlib
import json
from pathlib import Path
import shutil
import wave

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    """Hash exact repository file bytes."""
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def main():
    """Require bound authority before creating one unchanged-sample shipping resource."""
    scope = json.loads((ROOT / 'audio-direction/positional-cue-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/positional-cue-runtime-authority.json').read_text())
    if authority['status'] != 'runtime_and_exact_pcm_extraction_authorized' or authority['resource_count'] != 1:
        raise ValueError('Missing exact extraction authority')
    for row in authority['proposal_bindings']:
        if digest(row['path']) != row['sha256']:
            raise ValueError('Changed approved proposal')
    extraction = scope['extraction']
    parent = extraction['accepted_parent']
    if digest(parent['path']) != parent['sha256']:
        raise ValueError('Changed accepted parent')
    with wave.open(str(ROOT / parent['path']), 'rb') as source:
        if (source.getnchannels(), source.getsampwidth(), source.getframerate()) != (2, 3, 48000):
            raise ValueError('Wrong accepted format')
        source.setpos(extraction['begin_frame'])
        pcm = source.readframes(extraction['frame_count'])
    if hashlib.sha256(pcm).hexdigest() != extraction['expected_pcm_sha256']:
        raise ValueError('Wrong proposed PCM range')
    output = ROOT / extraction['proposed_path']
    output.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(output), 'wb') as target:
        target.setnchannels(2)
        target.setsampwidth(3)
        target.setframerate(48000)
        target.writeframes(pcm)
    record = dict(extraction)
    record.update(resource_created=True, header_sha256=digest(extraction['proposed_path']))
    (output.parent / 'provenance.json').write_text(json.dumps(record, indent=2) + '\n')
    shutil.copyfile(ROOT / scope['rights']['licence_notice']['path'], output.parent / 'CC0-1.0-deed.html')
    (output.parent / 'CREDITS.txt').write_text(
        'Interface Sounds by Kenney\nSource: https://kenney.nl/assets/interface-sounds\n'
        'License: CC0-1.0 - https://creativecommons.org/publicdomain/zero/1.0/\n'
        'Changes: retained audition edits; exact unchanged stereo PCM frames [192000,216000) '
        'from accepted A06-Dialogue.wav (4.0-4.5 seconds); no additional sample transformations.\n'
        'Runtime use: optional recorded-reply indication; not voice or conversation completion.\n')
    print('PASS: one approved 24000-frame PCM resource; samples exact; provenance and CC0 credit packaged')


if __name__ == '__main__':
    main()
