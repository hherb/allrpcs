"""Qualify capture/error/model-boundary behavior with synthetic devices only; never open a mic."""
import importlib.util
import io
import json
from pathlib import Path
import sys
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('worker', Path(__file__).with_name('player-speech-worker.py'))
worker = importlib.util.module_from_spec(spec)
spec.loader.exec_module(worker)


class CaptureTests(unittest.TestCase):
    def run_capture(self, device):
        output = io.StringIO()
        with patch.object(sys, 'stdin', io.StringIO('{"type":"start"}\n')), patch.object(worker, 'OUTPUT', output):
            with self.assertRaises((ValueError, PermissionError)):
                worker.microphone(device)
        return [json.loads(line) for line in output.getvalue().splitlines()]

    def test_supported_rates_finish_normally_and_close_capture(self):
        for rate in (11025, 22050, 44100, 48000):
            with self.subTest(rate=rate):
                class FinishingDevice(Device):
                    def query_devices(self, **kwargs):
                        return dict(super().query_devices(**kwargs), default_samplerate=rate)
                    def InputStream(self, **kwargs):
                        assert kwargs['samplerate'] == rate
                        assert kwargs['blocksize'] == int(rate * .032)
                        return self
                    def read(self, frames):
                        import numpy as np
                        return np.full((frames, 1), .1, dtype=np.float32), False
                device = FinishingDevice()
                output = io.StringIO()
                with patch.object(sys, 'stdin', io.StringIO('{"type":"start"}\n')), patch.object(worker, 'OUTPUT', output):
                    with patch.object(worker.time, 'monotonic', return_value=0):
                        worker.microphone(device)
                values = [json.loads(line) for line in output.getvalue().splitlines()]
                self.assertTrue(device.closed)
                self.assertEqual(values[-1], {'type':'finished'})
                self.assertEqual(len(values[1:-1]), 938)
                self.assertTrue(all(value['sampleRate'] == rate for value in values[1:-1]))

    def test_start_is_explicit_and_no_device_is_queried_before_it(self):
        class Device:
            def query_devices(self, **kwargs):
                raise AssertionError('device must remain unopened')
        with patch.object(sys, 'stdin', io.StringIO('{"type":"finish"}\n')), patch.object(worker, 'OUTPUT', io.StringIO()):
            with self.assertRaises(ValueError):
                worker.microphone(Device())

    def test_permission_denial_has_no_audio(self):
        class Denied(Device):
            def InputStream(self, **kwargs):
                raise PermissionError('synthetic denial')
        self.assertEqual(self.run_capture(Denied()), [{'type':'ready'}])

    def test_pcm_is_bounded_and_device_read_failure_closes_stream(self):
        device = Device()
        values = self.run_capture(device)
        self.assertTrue(device.closed)
        self.assertEqual([value['type'] for value in values], ['ready', 'audio'])
        self.assertEqual(values[1]['samples'], 512)
        self.assertEqual(values[1]['sampleRate'], 16000)

    def test_changed_device_retires_before_forwarding_wrong_pcm(self):
        device = Device(changed=True)
        with patch.object(worker.time, 'monotonic', side_effect=[0, 0, 2, 2]):
            values = self.run_capture(device)
        self.assertTrue(device.closed)
        self.assertEqual(values, [{'type':'ready'}])

    def test_protocol_rejects_unterminated_or_oversized_requests(self):
        for line in ('{"type":"start"}', 'a' * 33000 + '\n'):
            with patch.object(sys, 'stdin', io.StringIO(line)):
                with self.assertRaises(ValueError):
                    list(worker.messages())


class Device:
    def __init__(self, changed=False):
        self.closed = False
        self.changed = changed
        self.queries = 0
        self.reads = 0
        self.active = True
    def query_devices(self, **kwargs):
        self.queries += 1
        return {'name':'changed' if self.changed and self.queries > 1 else 'synthetic',
                'hostapi':0,'max_input_channels':1,'default_samplerate':16000}
    def InputStream(self, **kwargs):
        assert kwargs['channels'] == 1 and kwargs['blocksize'] == 512
        return self
    def __enter__(self):
        return self
    def __exit__(self, *args):
        self.closed = True
    def read(self, frames):
        import numpy as np
        self.reads += 1
        return np.full((frames, 1), .1, dtype=np.float32), self.reads > 1


if __name__ == '__main__':
    unittest.main()
