#!/usr/bin/env python3
"""Acquire only the nine approved public source containers, retaining original bytes and rights records."""
from pathlib import Path
from urllib.parse import urlsplit, unquote
import hashlib
import json
import requests

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PLAN = json.loads((REPO / 'audio-direction/material-audition-source-plan.json').read_text())
AUTHORITY = json.loads((ROOT / 'acquisition-authority.json').read_text())
LINKS = {r['source_id']: r for r in json.loads((ROOT / 'public-catalogue-links.json').read_text())}
PREFERRED = {'rain': 'amb_rain2.flac', 'stream': 'stream-waterfall.zip',
             'leaf-rain': 'Rain-on-dry-leaves-sound.mp3', 'wind': 'wind background noise 2.wav',
             'water-contact': 'water-splash-slime-sfx.zip', 'wood-tool': '100-CC0-wood-metal-SFX.zip',
             'interface': 'kenney_interface-sounds.zip', 'exploration': 'sunset_plains.wav', 'rest': 'Revelation.Wav'}


def save_json(path, value):
    """Write a durable metadata record without altering frozen source-plan bytes."""
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, indent=2) + '\n')
    temporary.replace(path)


def main():
    """Enforce the explicit grant, named source URLs and total transfer/storage caps."""
    for binding in AUTHORITY['plan_bindings']:
        assert hashlib.sha256((REPO / binding['path']).read_bytes()).hexdigest() == binding['sha256']
    path = ROOT / 'provenance.json'
    record = json.loads(path.read_text()) if path.exists() else {
        'schema_version': 1, 'date': '2026-10-07', 'status': 'acquisition_in_progress',
        'authority': 'audio-direction/material-audition/acquisition-authority.json',
        'download_bytes_received': 0, 'sources': {}, 'credentials_retained': False,
        'new_accounts_created': False, 'purchase_fees_aud': 0, 'owner_selected': False}
    for source in PLAN['sources']:
        identity = source['id']
        if identity not in PREFERRED or identity in record['sources']:
            continue
        catalogue = LINKS[identity]
        html = (REPO / catalogue['path']).read_bytes()
        assert hashlib.sha256(html).hexdigest() == catalogue['sha256']
        licence_url = PLAN['licenses'][source['license']]['url']
        assert licence_url.removeprefix('https://').encode() in html, 'Declared licence no longer present: ' + identity
        matches = [l['url'] for l in catalogue['download_links']
                   if unquote(urlsplit(l['url']).path).split('/')[-1] == PREFERRED[identity]]
        assert len(matches) == 1, 'Missing or ambiguous named original: ' + identity
        url = matches[0]
        folder = ROOT / 'originals' / identity
        folder.mkdir(parents=True, exist_ok=True)
        destination = folder / PREFERRED[identity]
        assert not destination.exists(), 'Unrecorded existing original: ' + identity
        temporary = destination.with_suffix(destination.suffix + '.part')
        print('Downloading ' + identity + ': ' + PREFERRED[identity], flush=True)
        response = requests.get(url, stream=True, timeout=(15, 60))
        response.raise_for_status()
        assert not response.headers.get('Content-Type', '').lower().startswith('text/'), 'Unexpected text download'
        expected = int(response.headers.get('Content-Length', 0))
        assert record['download_bytes_received'] + expected <= AUTHORITY['limits']['download_bytes']
        try:
            with temporary.open('xb') as output:
                for chunk in response.iter_content(1024 * 1024):
                    if not chunk:
                        continue
                    record['download_bytes_received'] += len(chunk)
                    assert record['download_bytes_received'] <= AUTHORITY['limits']['download_bytes'], 'Transfer cap reached'
                    assert sum(f.stat().st_size for f in ROOT.rglob('*') if f.is_file()) + len(chunk) <= AUTHORITY['limits']['working_storage_bytes'], 'Storage cap reached'
                    output.write(chunk)
            assert temporary.stat().st_size > 0
            temporary.replace(destination)
        finally:
            save_json(path, record)
            response.close()
        record['sources'][identity] = {
            'title': source['title'], 'author': source['author'], 'catalogue_url': source['catalogue_url'],
            'catalogue_capture': catalogue['path'], 'catalogue_sha256': catalogue['sha256'],
            'license': source['license'], 'license_url': licence_url, 'acquired_on': '2026-10-07',
            'method': 'Named original from public catalogue download link', 'download_url': url,
            'path': str(destination.relative_to(REPO)), 'original_filename': destination.name,
            'sha256': hashlib.sha256(destination.read_bytes()).hexdigest(), 'bytes': destination.stat().st_size,
            'acoustic_listening': 'not_assessed', 'metadata': None, 'fee_aud': 0}
        save_json(path, record)
        print('Retained ' + identity + ': ' + str(destination.stat().st_size) + ' bytes', flush=True)
    print('Public acquisition complete: ' + str(len(record['sources'])) + ' sources', flush=True)


if __name__ == '__main__':
    main()
