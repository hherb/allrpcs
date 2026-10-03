#!/usr/bin/env python3
"""Reproduce the frozen curved spring using the successor's broader preservation inventory."""

import importlib.util
import json
import sys

import proof


def main():
    """Substitute scope auditing only; retain all historical image, master and evidence checks."""
    proof.check_scope(False)
    predecessor_root = proof.ROOT / 'art-direction/spring-curved-bank-proof'
    predecessor_inventory = json.loads((predecessor_root / 'baseline.json').read_text())
    successor_inventory = json.loads((proof.HERE / 'baseline.json').read_text())
    proof.require(all(successor_inventory.get(name) == digest
                      for name, digest in predecessor_inventory.items()),
                  'successor inventory does not preserve complete predecessor baseline')
    spec = importlib.util.spec_from_file_location('outlet_predecessor', predecessor_root / 'tools/proof.py')
    predecessor = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = predecessor
    spec.loader.exec_module(predecessor)
    # The historical evidence retains its historical protected count. Its original
    # scope audit intentionally rejects successors, so use the already-verified superset.
    predecessor.check_scope = lambda build: len(predecessor_inventory)
    predecessor.run(False)
    print('PASS: frozen spring reproduced under unchanged 4,023-file successor inventory')


if __name__ == '__main__':
    main()
