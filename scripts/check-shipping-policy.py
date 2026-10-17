#!/usr/bin/env python3
"""Pin every preset and production test override to the shipping policy."""
import configparser
import pathlib
import re
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[1]
GAME = ROOT / 'game'
OVERRIDE_CONTROLLERS = (
    'OpenAiCredentialController', 'CharacterModelSettingsStorageController',
    'SaveDocumentStorageController', 'AudioSettingsStorageController',
    'ControlSettingsStorageController', 'OrdinaryParticipationSmokeModelSupplier',
    'TemporaryCharacterModelCompositionController',
)


class ShippingPolicyTests(unittest.TestCase):
    def test_every_actual_preset_excludes_harness_resources(self):
        config = configparser.ConfigParser()
        config.read(GAME / 'export_presets.cfg')
        presets = [s for s in config.sections() if re.fullmatch(r'preset\.\d+', s)]
        self.assertTrue(presets)
        for preset in presets:
            with self.subTest(preset=preset):
                patterns = config[preset]['exclude_filter'].strip('"').split(',')
                self.assertTrue({'*Check*', '*Proof*', '*Probe*'} <= set(patterns))

    def test_test_overrides_use_debug_policy_at_every_read(self):
        for controller in OVERRIDE_CONTROLLERS:
            with self.subTest(controller=controller):
                source = (GAME / (controller + '.cs')).read_text()
                self.assertFalse('OS.GetEnvironment(' in source, 'Raw test environment read: ' + controller)
                self.assertIn('DebugTestEnvironment.GetEnvironment(', source)
        policy = GAME / 'DebugTestEnvironment.cs'
        self.assertTrue(policy.exists(), 'Debug-only test environment policy missing')
        source = policy.read_text()
        self.assertIn('#if DEBUG', source)
        self.assertIn('OS.IsDebugBuild()', source)

    def test_no_unclassified_production_environment_reads(self):
        allowed = {'DebugTestEnvironment.cs', 'DialogueSpeechController.cs', 'PlayerSpeechController.cs'}
        actual = {
            path.name for path in GAME.rglob('*.cs')
            if '.godot' not in path.parts
            and not any(word in path.name for word in ('Check', 'Proof', 'Probe'))
            and 'OS.GetEnvironment(' in path.read_text()
        }
        self.assertEqual(actual, allowed)

    def test_ordinary_speech_configuration_remains_available(self):
        for controller in ('DialogueSpeechController', 'PlayerSpeechController'):
            self.assertIn('OS.GetEnvironment(', (GAME / (controller + '.cs')).read_text())

    def test_export_plugin_enabled(self):
        self.assertIn('res://addons/shipping_hygiene/plugin.cfg', (GAME / 'project.godot').read_text())


if __name__ == '__main__':
    unittest.main()
