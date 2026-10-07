#!/usr/bin/env python3
"""Verify the narrow replacement against immutable owner acceptance and retained numerical evidence."""
from pathlib import Path
import hashlib
import json
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
REVISION = ROOT / 'm02-flute-revision'


def digest(path):
    """Bind current or predecessor evidence by actual bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Reuse untouched evidence, checking affected captures and exact accepted material preservation."""
    manifest = json.loads((ROOT / 'candidates.json').read_text())
    measurements = json.loads((ROOT / 'measurements.json').read_text())
    previous_measurements = json.loads((REVISION / 'previous-measurements.json').read_text())
    accepted = json.loads((ROOT / 'accepted-fifteen.json').read_text())['accepted_candidates']
    assert len(accepted) == 15
    for frozen in accepted:
        current = next(c for c in manifest['candidates'] if c['id'] == frozen['id'])
        assert digest(REPO / frozen['path']) == frozen['sha256'] == current['sha256']
        for field in ['source_edits', 'routed_components', 'audition_buses', 'actual_seconds']:
            assert current[field] == frozen[field]
        for part in frozen['routed_components']:
            assert digest(REPO / part['path']) == part['sha256']
        old = next(m for m in previous_measurements['excerpts'] if m['id'] == frozen['id'])
        new = next(m for m in measurements['excerpts'] if m['id'] == frozen['id'])
        assert new == old, 'Accepted measurement was unnecessarily replaced'
    render = json.loads((ROOT / 'review/render-record.json').read_text())
    previous_render = json.loads((REVISION / 'previous-render-record.json').read_text())
    assert render['bus_layout_sha256'] == previous_render['bus_layout_sha256'] == digest(REPO / 'game/AudioBusLayout.tres')
    assert render['renderer_script_sha256'] == previous_render['renderer_script_sha256'] == digest(ROOT / 'tooling/audition.gd')
    assert render['candidates_sha256'] == measurements['candidates_manifest_sha256'] == digest(ROOT / 'candidates.json')
    for track in render['tracks']:
        assert digest(Path(track['path'])) == track['sha256']
        if track['bus'] != 'Music':
            old = next(t for t in previous_render['tracks'] if t['bus'] == track['bus'])
            assert old['sha256'] == track['sha256']
    outputs = {o['name']: o for o in render['outputs']}
    for name, output in outputs.items():
        assert digest(REPO / output['path']) == output['sha256']
        measured = next(m for m in measurements['final_mix_outputs'] if m['name'] == name)
        assert measured['sha256'] == output['sha256']
        assert digest(REPO / measured['meter_log']) == measured['meter_log_sha256']
    old_free = next(o for o in previous_render['outputs'] if o['name'] == 'music-free')
    assert old_free['sha256'] == outputs['music-free']['sha256']
    default, rate = sf.read(REPO / outputs['default']['path'], always_2d=True)
    free, free_rate = sf.read(REPO / outputs['music-free']['path'], always_2d=True)
    reduced, reduced_rate = sf.read(REPO / outputs['reduced-range']['path'], always_2d=True)
    assert rate == free_rate == reduced_rate == 48000
    assert default.shape == free.shape == reduced.shape == (300 * rate, 2)
    assert all(np.isfinite(data).all() for data in [default, free, reduced])
    pre_music = float(np.max(np.abs(default[:255 * rate] - free[:255 * rate])))
    free_music_peak = float(np.max(np.abs(free[255 * rate:])))
    assert pre_music == 0 and free_music_peak == 0 and not np.array_equal(default, reduced)
    comparison = {'default_music_free_identical_nonmusic_section': True,
                  'pre_music_maximum_sample_difference': pre_music,
                  'music_free_after_255_sample_peak': free_music_peak,
                  'music_free_music_window_silent': True, 'range_output_differs': True,
                  'rate_hz': rate, 'seconds_per_output': 300.0,
                  'input_hashes': {name: o['sha256'] for name, o in outputs.items()},
                  'fifteen_accepted_excerpts_and_routing_unchanged': True,
                  'fifteen_excerpt_measurements_reused_exactly': True,
                  'three_nonmusic_tracks_reused_exactly': True,
                  'music_free_capture_and_meter_reused_exactly': True,
                  'script_sha256': digest(Path(__file__))}
    (ROOT / 'review/comparison-checks.json').write_text(json.dumps(comparison, indent=2) + '\n')
    controls = ROOT / 'material-controls/measurements.json'
    prior_verification = json.loads((REVISION / 'previous-verification.json').read_text())
    assert digest(controls) == prior_verification['evidence']['sha256']
    control_record = json.loads(controls.read_text())
    for name, binding in control_record['input_sha256'].items():
        assert digest(ROOT / 'material-controls' / (name + '-input.wav')) == binding
    for name, binding in control_record['output_sha256'].items():
        assert digest(ROOT / 'material-controls' / (name + '.wav')) == binding
    decoding = json.loads((ROOT / 'source-decoding.json').read_text())
    assert len(decoding['files']) == 22
    for source in decoding['files']:
        assert digest(REPO / source['path']) == source['sha256']
    assert render['python_script_sha256'] == digest(REPO / render['executed_python_script_path'])
    storage = sum(p.stat().st_size for p in ROOT.rglob('*') if p.is_file())
    assert storage < 2000000000
    record = {'schema_version': 1, 'status': 'technical_revision_complete_only_m02_owner_selection_pending',
              'accepted_excerpt_count': 15, 'pending_revised_excerpt': 'M02',
              'original_source_count_retained_including_rejected_rest': 17,
              'actual_material_output_control_probes_reused': 19,
              'accepted_record_sha256': digest(ROOT / 'accepted-fifteen.json'),
              'candidate_manifest_sha256': digest(ROOT / 'candidates.json'),
              'measurements_sha256': digest(ROOT / 'measurements.json'),
              'render_record_sha256': digest(ROOT / 'review/render-record.json'),
              'comparison_checks_sha256': digest(ROOT / 'review/comparison-checks.json'),
              'controls_sha256': digest(controls), 'working_storage_bytes': storage,
              'agent_acoustic_listening': 'not_assessed',
              'runtime_installation': False, 'new_publication_authority': False}
    (REVISION / 'verification.json').write_text(json.dumps(record, indent=2) + '\n')
    print('PASS: fifteen accepted excerpts, measurements and routing preserved; dependent mixes verified')
    print('PASS: nonmusic tracks, music-free output/meter and nineteen controls reused; storage', storage)


if __name__ == '__main__':
    main()
