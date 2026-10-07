#!/usr/bin/env python3
"""Retain a named original downloaded by the authenticated browser or supplied directly by the owner."""
from pathlib import Path
from urllib.parse import urlsplit, unquote
import argparse
import hashlib
import json
import shutil
import subprocess
import requests

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def digest(path):
    """Bind exact container and public rights-capture bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Register only the seven named source IDs without reading cookies or collecting credentials."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-id', required=True)
    parser.add_argument('--file', required=True, type=Path)
    parser.add_argument('--observed-download-url', required=True,
                        help='Exact original URL observed on the authenticated catalogue page')
    args = parser.parse_args()
    plan = json.loads((REPO / 'audio-direction/material-audition-source-plan.json').read_text())
    authority = json.loads((ROOT / 'acquisition-authority.json').read_text())
    for binding in authority['plan_bindings']:
        assert digest(REPO / binding['path']) == binding['sha256']
    matching = [s for s in plan['sources'] if s['id'] == args.source_id
                and urlsplit(s['catalogue_url']).hostname == 'freesound.org']
    assert len(matching) == 1, 'Not a named Freesound original'
    source = matching[0]
    sound_id = urlsplit(source['catalogue_url']).path.rstrip('/').split('/')[-1]
    url = urlsplit(args.observed_download_url)
    assert url.scheme == 'https' and url.hostname == 'freesound.org'
    assert url.path.startswith(urlsplit(source['catalogue_url']).path + 'download/')
    filename = unquote(url.path).split('/')[-1]
    assert filename.startswith(sound_id + '__') and Path(filename).name == filename
    assert Path(filename).suffix.lower() == ('.mp3' if args.source_id == 'timber' else '.wav')
    record_path = ROOT / 'provenance.json'
    record = json.loads(record_path.read_text())
    assert args.source_id not in record['sources'], 'Original already registered'
    size = args.file.stat().st_size
    assert size > 0 and record['download_bytes_received'] + size <= authority['limits']['download_bytes']
    assert sum(f.stat().st_size for f in ROOT.rglob('*') if f.is_file()) + size <= authority['limits']['working_storage_bytes']
    # Fetch public rights only. The original has already been downloaded by the supported browser API.
    response = requests.get(source['catalogue_url'], timeout=(15, 60))
    response.raise_for_status()
    license_url = plan['licenses'][source['license']]['url']
    assert license_url.removeprefix('https://').encode() in response.content, 'Declared licence changed'
    capture = ROOT / 'rights' / (args.source_id + '-catalogue.html')
    capture.write_bytes(response.content)
    metadata = json.loads(subprocess.check_output([
        'ffprobe', '-v', 'error', '-show_format', '-show_streams', '-of', 'json', str(args.file)], text=True))
    streams = [s for s in metadata['streams'] if s['codec_type'] == 'audio']
    assert len(streams) == 1 and int(streams[0]['channels']) == 2
    assert abs(float(metadata['format']['duration']) - source['catalogue_metadata']['duration_seconds']) < .15
    assert int(streams[0]['sample_rate']) == source['catalogue_metadata']['sample_rate_hz']
    destination = ROOT / 'originals' / args.source_id / filename
    destination.parent.mkdir(parents=True, exist_ok=True)
    assert not destination.exists()
    shutil.copyfile(args.file, destination)
    metadata['format']['filename'] = str(destination.relative_to(REPO))
    record['download_bytes_received'] += size
    record['sources'][args.source_id] = {
        'title': source['title'], 'author': source['author'], 'catalogue_url': source['catalogue_url'],
        'catalogue_capture': str(capture.relative_to(REPO)), 'catalogue_sha256': digest(capture),
        'license': source['license'], 'license_url': license_url, 'acquired_on': '2026-10-07',
        'method': 'Original downloaded by owner-authenticated supported browser API or supplied by owner',
        'download_url': args.observed_download_url, 'path': str(destination.relative_to(REPO)),
        'original_filename': filename, 'sha256': digest(destination), 'bytes': size,
        'acoustic_listening': 'not_assessed', 'metadata': metadata, 'fee_aud': 0,
    }
    temporary = record_path.with_suffix('.json.tmp')
    temporary.write_text(json.dumps(record, indent=2) + '\n')
    temporary.replace(record_path)
    print('Retained named original', args.source_id, size, 'bytes')


if __name__ == '__main__':
    main()
