#!/usr/bin/env python3
"""Measure three actual final Master captures against the unchanged provisional audition targets."""
from pathlib import Path
import hashlib
import argparse
import json
import re
import subprocess
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def digest(path):
    """Bind output and retained measurement evidence."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Meter the input to loudnorm, leaving review audio unchanged and recording deviations honestly."""
    render = json.loads((ROOT / 'review/render-record.json').read_text())
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--revise-m02', action='store_true', help='Reuse exact unaffected music-free meter evidence')
    args = parser.parse_args()
    assert {o['name'] for o in render['outputs']} == {'default', 'reduced-range', 'music-free'}
    measurements = json.loads((ROOT / 'measurements.json').read_text())
    assert len(measurements['excerpts']) == 16
    assert render['candidates_sha256'] == measurements['candidates_manifest_sha256']
    policy_path = REPO / 'audio-direction/dynamic-range-policy.json'
    policy = json.loads(policy_path.read_text())['targets']
    reports = []
    for output in render['outputs']:
        path = REPO / output['path']
        assert digest(path) == output['sha256']
        if args.revise_m02 and output['name'] == 'music-free':
            old = next(m for m in measurements['final_mix_outputs'] if m['name'] == 'music-free')
            assert old['sha256'] == output['sha256']
            assert digest(REPO / old['meter_log']) == old['meter_log_sha256']
            reports.append({**old, **output, 'measurement_reused_unchanged': True})
            continue
        command = ['ffmpeg', '-nostdin', '-hide_banner', '-i', str(path),
                   '-af', 'loudnorm=I=-23:TP=-1:LRA=9:print_format=json', '-f', 'null', '-']
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        log_path = ROOT / 'review' / (output['name'] + '-meter.log')
        log_path.write_text(result.stderr)
        matches = re.findall(r'\{\s*"input_i".*?\}', result.stderr, re.DOTALL)
        assert len(matches) == 1, 'Missing complete loudnorm input measurement'
        values = json.loads(matches[0])
        data, rate = sf.read(path, always_2d=True)
        peak = float(np.max(np.abs(data)))
        lra = float(values['input_lra'])
        loudness = float(values['input_i'])
        true_peak = float(values['input_tp'])
        name = output['name']
        range_target = policy['reduced_range_lra_lu'] if name == 'reduced-range' else policy['default_lra_lu']
        reports.append({
            **output, 'measurement_point': render['measurement_point'],
            'integrated_lufs': loudness, 'lra_lu': lra, 'true_peak_dbtp': true_peak,
            'sample_peak_dbfs': float(20 * np.log10(peak)), 'rate_hz': rate,
            'meter': 'Installed ffmpeg loudnorm INPUT fields; measurement filter output is discarded',
            'meter_log': str(log_path.relative_to(REPO)), 'meter_log_sha256': digest(log_path),
            'comparisons_to_provisional_targets': {
                'default_loudness_band': policy['default_mix_lufs_i'],
                'within_default_loudness_band': policy['default_mix_lufs_i']['audition_min'] <= loudness <= policy['default_mix_lufs_i']['audition_max'],
                'lra_band': range_target, 'within_lra_band': range_target['min'] <= lra <= range_target['max'],
                'true_peak_ceiling_dbtp': policy['delivered_master_true_peak_max_dbtp'],
                'within_true_peak_ceiling': true_peak <= policy['delivered_master_true_peak_max_dbtp'],
            },
            'listening_and_device_assessment': 'not_assessed',
        })
    measurements['status'] = 'complete_numerical_measurements_pending_listening_and_owner_selection'
    measurements['final_mix_outputs'] = reports
    measurements['mix_render_record_sha256'] = digest(ROOT / 'review/render-record.json')
    measurements['mix_meter_script_sha256'] = digest(Path(__file__))
    measurements['policy_sha256'] = digest(policy_path)
    measurements['combined_pre_master_sample_peak_dbfs'] = render['combined_pre_master_sample_peak_dbfs']
    (ROOT / 'measurements.json').write_text(json.dumps(measurements, indent=2) + '\n')
    for report in reports:
        print(report['name'], report['integrated_lufs'], 'LUFS;', report['lra_lu'], 'LU LRA;', report['true_peak_dbtp'], 'dBTP')


if __name__ == '__main__':
    main()
