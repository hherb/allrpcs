#!/usr/bin/env python3
"""Extract only the owner-authorized exact PCM contact ranges from accepted passages."""

import hashlib
import json
from pathlib import Path
import shutil
import wave

ROOT = Path(__file__).resolve().parents[1]


def require(value, message):
    """Reject changed approval, ranges or samples even with optimized Python."""
    if not value:
        raise ValueError(message)


def digest(path):
    """Hash exact file bytes without decoding an accepted source."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Write 24 headers around unchanged PCM and retain their parent ancestry."""
    scope = json.loads((ROOT / 'audio-direction/footstep-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/footstep-runtime-authority.json').read_text())
    require(authority['owner_statement'] == 'approved'
            and authority['status'] == 'runtime_and_exact_pcm_extraction_authorized', 'Missing extraction grant')
    for binding in authority['proposal_bindings']:
        require(digest(ROOT / binding['path']) == binding['sha256'], 'Changed approved proposal')
    output = ROOT / 'game/assets/audio/footsteps'
    output.mkdir(parents=True, exist_ok=True)
    records = []
    sources = json.loads((ROOT / 'audio-direction/material-audition/provenance.json').read_text())['sources']
    candidates = json.loads((ROOT / 'audio-direction/material-audition/candidates.json').read_text())['candidates']
    ancestry = []
    credits = ['Allrpcs Seeker footstep contact resources',
               '24 exact PCM slices from owner-accepted passages; headers only, no new sample processing.',
               'No anatomical left/right attribution is inferred from source recordings.', '']
    for contact in scope['extraction']['contacts']:
        parent = contact['accepted_parent']
        require(digest(ROOT / parent['path']) == parent['sha256'], 'Accepted parent changed')
        with wave.open(str(ROOT / parent['path']), 'rb') as reader:
            require((reader.getnchannels(), reader.getsampwidth(), reader.getframerate(), reader.getnframes())
                    == (2, 3, 48000, 480000), 'Unexpected accepted PCM format')
            pcm = reader.readframes(reader.getnframes())
        for variant in contact['variants']:
            begin, end = variant['parent_pcm_begin_frame'], variant['parent_pcm_end_frame_exclusive']
            require(end - begin == 24000, 'Unapproved range duration')
            data = pcm[begin * 6:end * 6]
            require(hashlib.sha256(data).hexdigest() == variant['expected_pcm_sha256'], 'Unapproved PCM slice')
            path = ROOT / variant['proposed_path']
            require(path.parent == output, 'Extraction outside authorized directory')
            with wave.open(str(path), 'wb') as writer:
                writer.setnchannels(2)
                writer.setsampwidth(3)
                writer.setframerate(48000)
                writer.writeframes(data)
            records.append({**variant, 'derived_file_created': True, 'derived_wav_sha256': digest(path),
                            'parent': parent, 'source_ancestry': contact['source_ancestry'],
                            'lossy_original': contact['lossy_original']})
        source = sources[contact['source_id']]
        candidate = next(c for c in candidates if c['id'] == contact['id'])
        ancestry.append({'id': contact['id'], 'accepted_parent': parent,
                         'accepted_source_edits': candidate['source_edits'],
                         'title': source['title'], 'author': source['author'],
                         'source_url': source['catalogue_url'], 'license': source['license'],
                         'license_url': source['license_url']})
        notice = source['credits_text'].replace('Draft excerpts:', 'Accepted parent passages:')
        notice = notice.replace('in candidates.json', 'in packaged provenance.json')
        credits.extend([notice,
                        f"Runtime {contact['id']}-01 through {contact['id']}-06: exact half-second PCM slices; parent/source ranges in provenance.json.", ''])
    require(len(records) == 24, 'Extraction count mismatch')
    (output / 'CREDITS.txt').write_text('\n'.join(credits) + '\n')
    (output / 'provenance.json').write_text(json.dumps({'schema_version': 1,
        'authority': 'audio-direction/footstep-runtime-authority.json',
        'operation': 'Header-only WAV extraction; PCM24 stereo 48000 Hz, original accepted frame bytes.',
        'accepted_parent_ancestry': ancestry, 'clips': records}, indent=2) + '\n')
    shutil.copyfile(ROOT / 'audio-direction/material-audition/rights/CC0-1.0-deed.html', output / 'CC0-1.0-deed.html')
    print('PASS: 24 authorized WAV resources written with exact accepted PCM; credits and ancestry retained')


if __name__ == '__main__':
    main()
