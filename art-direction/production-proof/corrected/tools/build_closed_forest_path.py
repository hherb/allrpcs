#!/usr/bin/env python3
"""Reproduce the accepted rounded closed-side forest path from its retained recipe."""

import importlib.util
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[4]
RECIPE = ROOT / 'art-direction/forest-path-ending-correction/tools/proof.py'


def compose():
    """Return exact rounded/eased pixels without rebuilding or altering archived evidence."""
    spec = importlib.util.spec_from_file_location('closed_forest_recipe', RECIPE)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    rounded, corrected = module.render()
    module.validate(rounded, corrected)
    return corrected
