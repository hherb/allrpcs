#!/usr/bin/env python3
"""Inspect approved originals and safely unpack audio/rights members without executing archive contents."""
from pathlib import Path, PurePosixPath
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
import stat
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
LIMIT = 2000000000
AUDIO_EXTENSIONS = {'.wav', '.flac', '.mp3', '.ogg'}


def digest(path):
    """Hash bytes without interpreting source content as instructions."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def metadata(path):
    """Read the actual audio stream metadata with the installed decoder."""
    result = subprocess.run(['ffprobe', '-v', 'error', '-show_format', '-show_streams',
                             '-of', 'json', str(path)], check=True, capture_output=True, text=True)
    return json.loads(result.stdout)


def extract(identity, original):
    """Retain a complete member inventory and extract only safe audio/rights files under the cap."""
    destination = ROOT / 'members' / identity
    inventory = []
    with zipfile.ZipFile(original) as archive:
        names = [member.filename for member in archive.infolist()]
        assert len(names) == len(set(names)), 'Duplicate member path'
        for member in archive.infolist():
            name = PurePosixPath(member.filename)
            assert not name.is_absolute() and '..' not in name.parts and '\\' not in member.filename
            assert not any(':' in part for part in name.parts), 'Unsafe archive path'
            mode = member.external_attr >> 16
            assert not stat.S_ISLNK(mode), 'Archive symlink is not allowed'
            record = {'member_name': member.filename, 'bytes': member.file_size, 'crc32': member.CRC,
                      'extracted': False}
            inventory.append(record)
            rights = 'license' in name.name.lower() or 'licence' in name.name.lower() or 'readme' in name.name.lower()
            selected = name.suffix.lower() in AUDIO_EXTENSIONS or (rights and name.suffix.lower() in {'.txt', '.md'})
            if member.is_dir() or not selected:
                continue
            target = destination.joinpath(*name.parts)
            assert target.resolve().is_relative_to(destination.resolve())
            if not target.exists():
                assert sum(p.stat().st_size for p in ROOT.rglob('*') if p.is_file()) + member.file_size <= LIMIT
                target.parent.mkdir(parents=True, exist_ok=True)
                with archive.open(member) as input_file, target.open('xb') as output_file:
                    remaining = member.file_size
                    while chunk := input_file.read(min(1024 * 1024, remaining + 1)):
                        remaining -= len(chunk)
                        assert remaining >= 0, 'Expanded member exceeds its declared size'
                        output_file.write(chunk)
                    assert remaining == 0
            assert target.stat().st_size == member.file_size
            record.update(extracted=True, path=str(target.relative_to(REPO)), sha256=digest(target))
    return inventory


def main():
    """Verify original hashes and record actual decoded metadata and archive members."""
    path = ROOT / 'provenance.json'
    record = json.loads(path.read_text())
    jobs = []
    for identity, source in record['sources'].items():
        original = REPO / source['path']
        assert digest(original) == source['sha256'], 'Original changed: ' + identity
        if original.suffix.lower() == '.zip':
            source['members'] = extract(identity, original)
            for member in source['members']:
                if member['extracted'] and Path(member['path']).suffix.lower() in AUDIO_EXTENSIONS:
                    jobs.append((member, REPO / member['path']))
        else:
            jobs.append((source, original))
    with ThreadPoolExecutor(max_workers=4) as pool:
        for (target, audio), value in zip(jobs, pool.map(lambda pair: metadata(pair[1]), jobs)):
            target['metadata'] = value
    path.write_text(json.dumps(record, indent=2) + '\n')
    print('PASS: immutable originals; safe archive paths; actual metadata for', len(jobs), 'audio files')
    for identity, source in record['sources'].items():
        if 'members' in source:
            print(identity + ': ' + ', '.join(m['member_name'] for m in source['members'] if m['extracted']))
        else:
            audio = source['metadata']['streams'][0]
            print(identity + ': ' + audio['codec_name'] + ', ' + audio['sample_rate'] + ' Hz, ' + str(audio['channels'])
                  + ' channels, duration ' + source['metadata']['format'].get('duration', 'unknown'))


if __name__ == '__main__':
    main()
