#!/bin/sh
# Current quality entry point; retain scripts/quality.sh as frozen proof input.
set -eu
quality_root=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
quality_python="$quality_root/artifacts/quality-python/bin/python"
if [ ! -x "$quality_python" ]; then
    echo 'Create artifacts/quality-python and install requirements-quality.txt; see README.md.' >&2
    exit 1
fi
PATH="$quality_root/artifacts/quality-python/bin:$PATH"
export PATH
exec "$quality_python" "$quality_root/quality.py" "$@"
