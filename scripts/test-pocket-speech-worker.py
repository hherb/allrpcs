"""Synthetic request-error/reuse tests; no model imports or speech processes."""
import importlib.util
import threading
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location('pocket_worker', Path(__file__).with_name('pocket-speech-worker.py'))
worker = importlib.util.module_from_spec(spec)
spec.loader.exec_module(worker)


class WorkerTests(unittest.TestCase):
    def test_generation_error_is_terminal_and_same_model_handles_next_request(self):
        class Model:
            sample_rate = 24000
            calls = 0

            def generate_audio_stream(self, state, text, stop):
                self.calls += 1
                if self.calls == 1:
                    raise ValueError('synthetic failure')
                class Audio:
                    def detach(self): return self
                    def cpu(self): return self
                    def numpy(self):
                        import numpy as np
                        return np.array([.25], dtype=np.float32)
                yield Audio()

        instance = worker.Worker.__new__(worker.Worker)
        instance.lock = threading.RLock()
        instance.models = {'en': Model()}
        instance.states = {('en', 'default'): object()}
        instance.active = None
        instance.thread = None
        messages = []
        instance.emit = messages.append
        request = {'type':'synthesize', 'id':'first', 'text':'Synthetic.', 'language':'en', 'voiceProfile':'default'}
        self.assertTrue(instance.handle(request))
        instance.thread.join(timeout=5)
        self.assertFalse(instance.thread.is_alive())
        self.assertEqual(messages, [{'type':'error', 'id':'first', 'code':'synthesis_failed'}])
        self.assertIsNone(instance.active)
        self.assertTrue(instance.handle(dict(request, id='next')))
        instance.thread.join(timeout=5)
        self.assertFalse(instance.thread.is_alive())
        self.assertEqual([value['type'] for value in messages], ['error', 'audio', 'finished'])
        self.assertEqual(instance.models['en'].calls, 2)
        self.assertEqual(messages[-1], {'type':'finished', 'id':'next', 'status':'completed'})


if __name__ == '__main__':
    unittest.main()
