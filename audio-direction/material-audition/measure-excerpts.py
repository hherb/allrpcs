#!/usr/bin/env python3
"""Measure retained audition exports without claiming that numerical checks establish audible suitability."""
from pathlib import Path
import hashlib
import json
import subprocess
import numpy as np
import soundfile as sf
from scipy.signal import resample_poly

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def digest(path):
    """Bind exact retained files to their measurements."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def db(value):
    """Represent zero as null instead of a fictitious finite silence level."""
    return float(20 * np.log10(value)) if value > 0 else None


def main():
    """Check actual PCM, trim boundaries, circular seams, mono folding and contact gaps."""
    manifest = json.loads((ROOT / 'candidates.json').read_text())
    records = []
    for candidate in manifest['candidates']:
        path = REPO / candidate['path']
        assert digest(path) == candidate['sha256']
        data, rate = sf.read(path, always_2d=True)
        assert rate == 48000 and data.shape == (int(candidate['actual_seconds'] * rate), 2)
        assert np.isfinite(data).all()
        info = sf.info(path)
        assert info.subtype == 'PCM_24'
        mono = data.mean(axis=1)
        rms = float(np.sqrt(np.mean(data * data)))
        mono_rms = float(np.sqrt(np.mean(mono * mono)))
        peak = float(np.max(np.abs(data)))
        oversampled_peak = float(np.max(np.abs(resample_poly(data, 4, 1, axis=0))))
        assert peak <= 10 ** (-6 / 20), 'Working headroom exceeded'
        report = {
            'id': candidate['id'], 'path': candidate['path'], 'sha256': candidate['sha256'],
            'frames': len(data), 'rate_hz': rate, 'subtype': info.subtype,
            'sample_peak_dbfs': db(peak), 'rms_dbfs': db(rms),
            'four_times_oversampled_peak_dbfs': db(oversampled_peak),
            'mono_fold_rms_dbfs': db(mono_rms),
            'mono_fold_rms_change_db': db(mono_rms / rms) if rms else None,
            'first_sample_absolute_max': float(np.max(np.abs(data[0]))),
            'last_sample_absolute_max': float(np.max(np.abs(data[-1]))),
            'audible_seam_material_contamination_device_fit': 'not_assessed',
        }
        if candidate['group'] == 'regional_bed':
            seam = float(np.max(np.abs(data[0] - data[-1])))
            local = np.concatenate((data[-2400:], data[:2400]))
            differences = np.max(np.abs(np.diff(local, axis=0)), axis=1)
            report['loop'] = {
                'begin_sample': 0, 'end_sample_exclusive': len(data),
                'seam_delta_dbfs': db(seam), 'local_difference_p99_dbfs': db(float(np.percentile(differences, 99))),
                'three_repeat_frames': len(data) * 3,
                'repeat_is_exact_copy': True, 'audible_seam': 'not_assessed',
            }
        contacts = [w for edit in candidate['source_edits'] for w in edit.get('contact_windows', [])]
        if contacts:
            gaps = []
            for previous, following in zip(contacts, contacts[1:]):
                end = round(previous['excerpt_start_seconds'] * rate) + previous['end_sample'] - previous['begin_sample']
                start = round(following['excerpt_start_seconds'] * rate)
                assert start > end and np.max(np.abs(data[end:start])) == 0
                gaps.append((start - end) / rate)
            report['contacts'] = {'count': len(contacts), 'silent_gap_seconds': gaps,
                                  'anatomical_foot_identity_and_material_fit': 'not_assessed'}
        records.append(report)
    versions = {
        'ffmpeg': subprocess.check_output(['ffmpeg', '-version'], text=True).splitlines()[0],
        'numpy': np.__version__, 'soundfile': sf.__version__,
    }
    record = {
        'schema_version': 1, 'status': 'partial_internal_measurements' if manifest['missing_source_ids'] else 'excerpt_measurements_complete',
        'candidates_manifest_sha256': digest(ROOT / 'candidates.json'),
        'script_sha256': digest(Path(__file__)), 'tool_versions': versions,
        'oversampled_peak_method': 'scipy resample_poly four-times FIR estimate; not a certified BS.1770 true-peak meter',
        'individual_short_transient_lufs_target': None, 'excerpts': records, 'final_mix_outputs': [],
        'listening': {'stereo': 'not_assessed', 'mono': 'not_assessed', 'headphones': 'not_assessed',
                     'speakers': 'not_assessed', 'low_volume': 'not_assessed', 'reduced_range': 'not_assessed'},
        'speech_masking_and_intelligibility': 'not_assessed_no_voice_source',
        'owner_selected': False,
    }
    (ROOT / 'measurements.json').write_text(json.dumps(record, indent=2) + '\n')
    print('Measured', len(records), 'excerpts; human listening and full mixes remain outstanding')


if __name__ == '__main__':
    main()
