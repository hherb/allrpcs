#!/usr/bin/env python3
"""Check the complete decoded recordings used by the audition without inferring acoustic suitability."""
from pathlib import Path
import hashlib
import json
import subprocess
import numpy as np

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def main():
    """Record actual decoder success, finite samples and potential original clipping for every used file."""
    candidates = json.loads((ROOT / 'candidates.json').read_text())
    files = {e['path']: e for c in candidates['candidates'] for e in c['source_edits']}
    reports = []
    for name, source in files.items():
        path = REPO / name
        assert hashlib.sha256(path.read_bytes()).hexdigest() == source['sha256']
        command = ['ffmpeg', '-v', 'error', '-nostdin', '-i', str(path),
                   '-f', 'f32le', '-ac', '2', '-ar', '48000', 'pipe:1']
        result = subprocess.run(command, check=True, capture_output=True)
        data = np.frombuffer(result.stdout, dtype='<f4').reshape(-1, 2)
        assert len(data) == source['decoded_frames'] and np.isfinite(data).all()
        peak = float(np.max(np.abs(data)))
        near_full_scale = int(np.count_nonzero(np.abs(data) >= .9999))
        reports.append({'path': name, 'source_id': source['source_id'], 'sha256': source['sha256'],
                        'decoded_frames_at_48000_hz': len(data), 'decoded_channels': 2,
                        'decoder_exit_code': result.returncode, 'decoder_error_output': result.stderr.decode(),
                        'finite_samples': True, 'sample_peak_dbfs': float(20 * np.log10(max(peak, 1e-12))),
                        'samples_at_or_above_0_9999': near_full_scale,
                        'near_full_scale_is_not_proof_of_audible_clipping': True,
                        'acoustic_contamination_and_suitability': 'not_assessed'})
    record = {'schema_version': 1, 'status': 'decoder_checks_passed',
              'scope': 'Complete decoded direct recordings and exact used archive members; unused pack members are metadata-inspected only',
              'candidate_manifest_sha256': hashlib.sha256((ROOT / 'candidates.json').read_bytes()).hexdigest(),
              'script_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(), 'files': reports}
    (ROOT / 'source-decoding.json').write_text(json.dumps(record, indent=2) + '\n')
    print('Decoded', len(reports), 'used recordings/member files; all finite and no decoder errors')


if __name__ == '__main__':
    main()
