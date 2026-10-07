#!/usr/bin/env python3
"""Check actual new material playback under the accepted layout's gains, mutes and mono effect."""
from pathlib import Path
import hashlib
import json
import os
import shutil
import subprocess
import tempfile
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
RATE = 48000


def main():
    """Exercise real source-derived PCM independently of the unchanged settings/controller proof."""
    destination = ROOT / 'material-controls'
    destination.mkdir(exist_ok=True)
    manifest = json.loads((ROOT / 'candidates.json').read_text())
    by_id = {c['id']: c for c in manifest['candidates']}
    inputs = {}
    for bus, identity, begin in [('Ambience', 'B01', 0), ('Effects', 'A01', 0),
                                  ('Music', 'M01', 0), ('Dialogue', 'A06', 3)]:
        relative = by_id[identity]['path'] if bus != 'Dialogue' else next(
            part['path'] for part in by_id[identity]['routed_components'] if part['bus'] == bus)
        data, rate = sf.read(REPO / relative, always_2d=True)
        assert rate == RATE
        data = data[begin * RATE:(begin + 4) * RATE]
        assert len(data) == 4 * RATE
        path = destination / (bus + '-input.wav')
        sf.write(path, data, rate, subtype='PCM_24')
        inputs[bus] = path
    probes = [(bus + '-' + mode, bus, mode) for bus in inputs for mode in ('normal', 'half', 'zero', 'mute')]
    probes += [('master-zero', 'Ambience', 'master-zero'), ('master-mute', 'Ambience', 'master-mute'),
               ('mono', 'Ambience', 'mono')]
    outputs = {}
    logs = []
    with tempfile.TemporaryDirectory(prefix='allrpcs-material-controls-') as workspace:
        project = Path(workspace)
        shutil.copyfile(REPO / 'game/AudioBusLayout.tres', project / 'AudioBusLayout.tres')
        script = (ROOT / 'tooling/audition.gd').read_text()
        script = script.replace('print("AUDITION_INPUTS ", JSON.stringify(loaded))',
            'if config.get("master_zero", false):\n\t\tAudioServer.set_bus_volume_db(0, linear_to_db(0.0))\n'
            '\tif config.has("zero_bus"):\n\t\tAudioServer.set_bus_volume_db(AudioServer.get_bus_index(config["zero_bus"]), linear_to_db(0.0))\n'
            '\tprint("AUDITION_INPUTS ", JSON.stringify(loaded))')
        (project / 'audition.gd').write_text(script)
        (project / 'project.godot').write_text('[application]\nrun/main_scene="res://audition.tscn"\n[display]\nwindow/size/viewport_width=64\nwindow/size/viewport_height=64\n[rendering]\nrenderer/rendering_method="gl_compatibility"\n[audio]\ndriver/mix_rate=48000\n')
        (project / 'audition.tscn').write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://audition.gd" id="1"]\n[node name="MaterialControl" type="Node"]\nscript = ExtResource("1")\n')
        fixture_hash = hashlib.sha256(script.encode()).hexdigest()
        for name, bus, mode in probes:
            config = {'tracks': [{'bus': bus, 'path': str(inputs[bus]), 'gain_db': -6.020599913 if mode == 'half' else 0,
                                   'mute': mode == 'mute'}], 'master_zero': mode == 'master-zero',
                      'master_mute': mode == 'master-mute', 'mono': mode == 'mono'}
            if mode == 'zero':
                config['zero_bus'] = bus
            config_path = destination / (name + '.json')
            config_path.write_text(json.dumps(config, indent=2) + '\n')
            frames = destination / 'frames'
            frames.mkdir(exist_ok=True)
            environment = dict(os.environ, ALLRPCS_AUDITION_CONFIG=str(config_path))
            command = ['godot', '--path', str(project), '--write-movie', str(frames / 'capture.png'),
                       '--fixed-fps', '60', '--rendering-method', 'gl_compatibility', '--quit-after', '240',
                       '--log-file', str(destination / (name + '-godot.log'))]
            result = subprocess.run(command, env=environment, check=True, capture_output=True, text=True)
            assert 'AUDITION_INPUTS' in result.stdout and 'ERROR:' not in result.stdout + result.stderr
            log = destination / (name + '-stdout.log')
            log.write_text(result.stdout + result.stderr)
            logs.append({'probe': name, 'sha256': hashlib.sha256(log.read_bytes()).hexdigest()})
            output = destination / (name + '.wav')
            (frames / 'capture.wav').replace(output)
            data, rate = sf.read(output, always_2d=True)
            assert rate == RATE and len(data) == 4 * RATE
            outputs[name] = data
            shutil.rmtree(frames)
            print('Captured material control', name, flush=True)
    measured = {}
    for bus in inputs:
        normal = outputs[bus + '-normal']
        assert np.max(np.abs(normal)) > .001
        half_error = float(np.max(np.abs(outputs[bus + '-half'] - normal * .5)))
        zero_peak = float(np.max(np.abs(outputs[bus + '-zero'])))
        mute_peak = float(np.max(np.abs(outputs[bus + '-mute'])))
        assert half_error < 1e-6 and zero_peak == 0 and mute_peak == 0
        measured[bus] = {'half_gain_maximum_sample_error': half_error, 'zero_peak': zero_peak, 'mute_peak': mute_peak}
    for name in ('master-zero', 'master-mute'):
        assert np.max(np.abs(outputs[name])) == 0
    mono = outputs['mono']
    channel_error = float(np.max(np.abs(mono[:, 0] - mono[:, 1])))
    fold_error = float(np.max(np.abs(mono[:, 0] - outputs['Ambience-normal'].mean(axis=1))))
    assert channel_error < 1e-6 and fold_error < 1e-6
    record = {'schema_version': 1, 'status': 'passed', 'actual_material_output_probe_count': len(probes),
              'measurement_point': 'Actual offline PCM32 after the copied accepted Master layout',
              'script_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
              'generated_fixture_sha256': fixture_hash, 'category_measurements': measured,
              'mono_channel_error': channel_error, 'mono_normalized_fold_error': fold_error,
              'master_zero_and_mute': 'exact_silence', 'logs': logs,
              'input_sha256': {bus: hashlib.sha256(path.read_bytes()).hexdigest() for bus, path in inputs.items()},
              'output_sha256': {name: hashlib.sha256((destination / (name + '.wav')).read_bytes()).hexdigest() for name in outputs},
              'limitations': 'Actual new material import/control output only; settings persistence and optional/required ducking reuse retained unchanged product evidence. No speech or human device/listening claim.'}
    (destination / 'measurements.json').write_text(json.dumps(record, indent=2) + '\n')
    print('PASS:', len(probes), 'actual material output checks')


if __name__ == '__main__':
    main()
