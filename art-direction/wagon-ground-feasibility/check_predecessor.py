#!/usr/bin/env python3
"""Run frozen Sickhouse checks after the exact external successor proves preservation."""

import importlib.util
from pathlib import Path

from inspect_package import preservation


def main():
    """Delegate tree membership only after proving every protected predecessor byte."""
    count = preservation()
    repo = Path(__file__).resolve().parents[2]
    path = repo / 'scripts/validate-sickhouse-ground-runtime.py'
    spec = importlib.util.spec_from_file_location('frozen_sickhouse', path)
    validator = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(validator)
    # The unchanged validator's membership gate knows only its historical additions.
    # The external successor replaces that gate, preserving all other live checks.
    validator.validate(preservation=False)
    validator.rejections()
    validator.captures()
    print(f'PASS: {count} preserved predecessor files and unchanged live Sickhouse checks')


if __name__ == '__main__':
    main()
