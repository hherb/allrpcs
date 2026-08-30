#!/bin/sh

set -eu

if [ "$#" -ne 1 ]; then
  echo "usage: $0 <linux-x64|macos-universal|windows-x64>" >&2
  exit 64
fi

case "$1" in
  linux-x64)
    preset="Linux Development"
    output_name="Allrpcs.x86_64"
    ;;
  macos-universal)
    preset="macOS Development"
    output_name="Allrpcs-macOS.zip"
    ;;
  windows-x64)
    preset="Windows Development"
    output_name="Allrpcs.exe"
    ;;
  *)
    echo "unsupported development-build target: $1" >&2
    exit 64
    ;;
esac

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")
target_directory="$repository_root/artifacts/unsigned-development/$1"
output_path="$target_directory/$output_name"

mkdir -p "$target_directory"

godot --headless --path "$repository_root/game" --export-debug "$preset" "$output_path"

{
  echo "ALLRPCS UNSIGNED DEVELOPMENT BUILD EVIDENCE"
  echo
  echo "Target: $1"
  echo "Preset: $preset"
  echo "Commit: $(git -C "$repository_root" rev-parse HEAD)"
  echo "Runner OS: ${RUNNER_OS:-local}"
  echo "Runner architecture: ${RUNNER_ARCH:-$(uname -m)}"
  echo "Godot: $(godot --version)"
  echo ".NET SDK: $(dotnet --version)"
  echo
  echo "This debug export is intentionally unsigned and is retained only as build evidence."
  echo "It is not signed, notarized, installer-packaged, release-tested, or approved for distribution."
  echo "A successful export does not establish gameplay, Gatekeeper, SmartScreen, or store validation."
} > "$target_directory/UNSIGNED-DEVELOPMENT-EVIDENCE.txt"
