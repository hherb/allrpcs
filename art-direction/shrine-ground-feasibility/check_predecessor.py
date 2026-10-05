#!/usr/bin/env python3
"""Run unchanged Wagon checks after proving exact external successor preservation."""

import importlib.util

from inspect_package import preservation
from build import REPO


def main():
    """Delegate historical membership only; retain every live asset/rejection/capture check."""
    count = preservation()
    path = REPO / 'scripts/validate-wagon-ground-runtime.py'
    spec = importlib.util.spec_from_file_location('frozen_wagon', path)
    validator = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(validator)
    validator.validate(preservation=False)
    validator.rejections()
    validator.captures()
    print(f'PASS: {count} preserved predecessor files and unchanged live Wagon checks')


if __name__ == '__main__':
    main()
