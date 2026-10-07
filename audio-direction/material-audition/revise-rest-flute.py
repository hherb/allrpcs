#!/usr/bin/env python3
"""Prepare only the owner-requested flute replacement, preserving accepted excerpts and evidence."""
from pathlib import Path
import hashlib
import importlib.util
import json
import numpy as np
import soundfile as sf
from scipy.signal import resample_poly

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
REVISION = ROOT / 'm02-flute-revision'
RATE = 48000


def digest(path):
    """Bind exact files to revision and acceptance records."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, value):
    """Atomically replace a current record without changing frozen predecessor evidence."""
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, indent=2) + '\n')
    temporary.replace(path)


def main():
    """Use one contiguous source passage, with no pitch change, synthesis, reverb or composition."""
    spec = importlib.util.spec_from_file_location('prepare', ROOT / 'prepare-excerpts.py')
    prepare = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(prepare)
    request = json.loads((REVISION / 'owner-request.json').read_text())
    accepted = json.loads((ROOT / 'accepted-fifteen.json').read_text())['accepted_candidates']
    manifest = json.loads((ROOT / 'candidates.json').read_text())
    for frozen in accepted:
        current = next(c for c in manifest['candidates'] if c['id'] == frozen['id'])
        assert current['sha256'] == frozen['sha256'] == digest(REPO / frozen['path'])
        assert current['source_edits'] == frozen['source_edits']
        assert current['routed_components'] == frozen['routed_components']
        for component in frozen['routed_components']:
            assert component['sha256'] == digest(REPO / component['path'])
        current['owner_selected'] = True
        current['qualification_status'] = 'owner_listening_accepted'
        current['owner_acceptance_record'] = 'audio-direction/material-audition/accepted-fifteen.json'
    candidate = next(c for c in manifest['candidates'] if c['id'] == 'M02')
    assert not candidate['owner_selected'], 'An accepted flute replacement cannot be revised automatically'
    source = json.loads((ROOT / 'provenance.json').read_text())['sources']['rest-flute']
    assert source['catalogue_url'] == request['replacement_source_url']
    path = REPO / source['path']
    assert digest(path) == source['sha256']
    decoded = prepare.decode(path)
    assert np.isfinite(decoded).all() and len(decoded) >= 55 * RATE
    audio, gain_db = prepare.gain(prepare.fade(decoded[30 * RATE:55 * RATE], 2), rms_db=-28)
    guard_db = 0.0
    if np.max(np.abs(audio)) > 10 ** (-7 / 20):
        audio, guard_db = prepare.gain(audio, peak_db=-7)
    sf.write(REPO / candidate['path'], audio, RATE, subtype='PCM_24')
    candidate.update(source_ids=['rest-flute'], title='Rest and healing — quiet shakuhachi',
                     edit_plan='Contiguous 30–55 second flute passage, two-second edge fades, uniform −28 dBFS RMS target.',
                     sha256=digest(REPO / candidate['path']),
                     sample_peak_dbfs=float(20 * np.log10(np.max(np.abs(audio)))),
                     rms_dbfs=float(20 * np.log10(np.sqrt(np.mean(audio.astype(float) ** 2)))),
                     final_uniform_headroom_gain_db=guard_db,
                     source_edits=[{'source_id': 'rest-flute', 'member_name': None, 'path': source['path'],
                                    'sha256': source['sha256'], 'decoded_rate_hz': RATE,
                                    'decoded_frames': len(decoded), 'lossy_original': False,
                                    'trim_begin_sample': 30 * RATE, 'trim_end_sample': 55 * RATE,
                                    'edge_fade_seconds': 2, 'applied_gain_db': gain_db,
                                    'original_channels': 1, 'original_rate_hz': 16000,
                                    'mono_mapping': 'Identical centred left/right; no artificial stereo'}],
                     qualification_status='revised_m02_pending_owner_listening',
                     revision_script_sha256=digest(Path(__file__)))
    manifest.update(status='fifteen_accepted_revised_m02_pending_selection',
                    acceptance_record_sha256=digest(ROOT / 'accepted-fifteen.json'),
                    revision_request_sha256=digest(REVISION / 'owner-request.json'),
                    revision_script_sha256=digest(Path(__file__)))
    write(ROOT / 'candidates.json', manifest)
    measurements = json.loads((ROOT / 'measurements.json').read_text())
    data, rate = sf.read(REPO / candidate['path'], always_2d=True)
    rms = np.sqrt(np.mean(data * data))
    peak = np.max(np.abs(data))
    assert rate == RATE and data.shape == (25 * RATE, 2) and peak < .5
    report = {'id': 'M02', 'path': candidate['path'], 'sha256': candidate['sha256'],
              'frames': len(data), 'rate_hz': rate, 'subtype': 'PCM_24',
              'sample_peak_dbfs': float(20 * np.log10(peak)),
              'rms_dbfs': float(20 * np.log10(rms)),
              'four_times_oversampled_peak_dbfs': float(20 * np.log10(np.max(np.abs(resample_poly(data, 4, 1, axis=0))))),
              'mono_fold_rms_dbfs': float(20 * np.log10(rms)), 'mono_fold_rms_change_db': 0.0,
              'first_sample_absolute_max': float(np.max(np.abs(data[0]))),
              'last_sample_absolute_max': float(np.max(np.abs(data[-1]))),
              'audible_seam_material_contamination_device_fit': 'not_assessed',
              'measurement_script_sha256': digest(Path(__file__))}
    measurements['excerpts'] = [report if e['id'] == 'M02' else e for e in measurements['excerpts']]
    measurements['candidates_manifest_sha256'] = digest(ROOT / 'candidates.json')
    measurements['revision_lineage'] = {'unchanged_fifteen_measurements': 'm02-flute-revision/previous-measurements.json',
                                        'predecessor_sha256': digest(REVISION / 'previous-measurements.json'),
                                        'updated_excerpt_only': 'M02'}
    write(ROOT / 'measurements.json', measurements)
    decoder = json.loads((ROOT / 'source-decoding.json').read_text())
    decoder['files'] = [e for e in decoder['files'] if e['source_id'] != 'rest']
    decoder['files'].append({'path': source['path'], 'source_id': 'rest-flute', 'sha256': source['sha256'],
                             'decoded_frames_at_48000_hz': len(decoded), 'decoded_channels': 2,
                             'decoder_exit_code': 0, 'decoder_error_output': '', 'finite_samples': True,
                             'sample_peak_dbfs': float(20 * np.log10(np.max(np.abs(decoded)))),
                             'samples_at_or_above_0_9999': int(np.count_nonzero(np.abs(decoded) >= .9999)),
                             'near_full_scale_is_not_proof_of_audible_clipping': True,
                             'acoustic_contamination_and_suitability': 'not_assessed',
                             'measurement_script_sha256': digest(Path(__file__))})
    decoder['candidate_manifest_sha256'] = digest(ROOT / 'candidates.json')
    decoder['revision_lineage'] = '21 unchanged decoder records reused; only rest-flute newly decoded'
    write(ROOT / 'source-decoding.json', decoder)
    print('Prepared only M02; fifteen accepted excerpt bytes and edit records unchanged')
    print('M02:', report['rms_dbfs'], 'dBFS RMS;', report['four_times_oversampled_peak_dbfs'], 'estimated dBFS peak')


if __name__ == '__main__':
    main()
