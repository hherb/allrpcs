#!/usr/bin/env python3
"""Install only the owner-authorized byte-exact first accepted water splash."""
import hashlib
import json
from pathlib import Path
import shutil
import wave

ROOT = Path(__file__).resolve().parents[1]


def require(value, message):
    """Reject changed owner authority and source bytes even under optimized Python."""
    if not value:
        raise ValueError(message)


def main():
    """Require exact scope bindings before extracting any PCM or installing notices."""
    grant = json.loads((ROOT / 'audio-direction/water-contact-runtime-authority.json').read_text())
    require(grant['status'] == 'exact_proposal_scope_authorized', 'Missing approved scope')
    for row in grant['proposal_bindings']:
        require(hashlib.sha256((ROOT / row['path']).read_bytes()).hexdigest() == row['sha256'], 'Changed owner proposal')
    scope = json.loads((ROOT / 'audio-direction/water-tool-owner-scope.json').read_text())
    clip = scope['proposed_resources'][0]
    source = ROOT / clip['source']['path']
    require(hashlib.sha256(source.read_bytes()).hexdigest() == clip['source']['sha256'], 'Changed accepted source')
    with wave.open(str(source), 'rb') as original:
        require((original.getnchannels(), original.getsampwidth(), original.getframerate()) == (2, 3, 48000), 'Changed source format')
        original.setpos(clip['frame_interval_half_open'][0])
        pcm = original.readframes(clip['frames'])
    require(len(pcm) == clip['pcm_bytes'] and hashlib.sha256(pcm).hexdigest() == clip['pcm_sha256'], 'Changed slice bytes')
    output = ROOT / clip['output']
    output.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(output), 'wb') as target:
        target.setnchannels(2); target.setsampwidth(3); target.setframerate(48000); target.writeframes(pcm)
    shutil.copyfile(ROOT / 'game/assets/audio/material/CC0-1.0-deed.html', output.parent / 'CC0-1.0-deed.html')
    credits = ('Water contact A01-W01\n40 CC0 water / splash / slime SFX by rubberduck\n'
               'Source: https://opengameart.org/content/40-cc0-water-splash-slime-sfx\n'
               'License: CC0-1.0 - https://creativecommons.org/publicdomain/zero/1.0/\n'
               'Changes: frame interval [48000,120000) from the accepted A01 audition; '
               'original source edits and gain retained; no further processing.\n'
               'Original author does not endorse this game.\n')
    (output.parent / 'CREDITS.txt').write_text(credits)
    record = {'schema_version': 1, 'scope_id': grant['scope_id'], 'source': clip['source'],
              'frame_interval_half_open': clip['frame_interval_half_open'], 'pcm_sha256': clip['pcm_sha256'],
              'frames': clip['frames'], 'rate_hz': 48000, 'channels': 2, 'sample_width': 3,
              'output_sha256': hashlib.sha256(output.read_bytes()).hexdigest(),
              'recorded_rights': scope['rights'], 'new_source_acquisition': False}
    (output.parent / 'provenance.json').write_text(json.dumps(record, indent=2) + '\n')
    print('PASS: owner-authorized 72000-frame exact A01 splash; unchanged accepted material and recorded source rights')


if __name__ == '__main__':
    main()
