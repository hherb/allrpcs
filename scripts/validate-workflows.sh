#!/bin/sh

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

if ! command -v actionlint >/dev/null 2>&1; then
  echo "actionlint is required to validate GitHub Actions workflows." >&2
  exit 127
fi

cd "$repository_root"
actionlint -color
