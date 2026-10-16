#!/bin/sh
# Self-test for scripts/smoke-lib.sh: a passing launch passes, a hung launch fails with the
# deadline code 124, a launch that logs an unhandled exception fails even though Godot exits 0,
# and only the diagnosed native mutex shutdown abort is retried, exactly once.

set -eu

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
repository_root=$(dirname -- "$script_dir")

. "$script_dir/smoke-lib.sh"

failures=0

expect()
{
    description=$1
    expected=$2
    actual=$3
    if [ "$actual" = "$expected" ]; then
        echo "ok: $description"
    else
        echo "FAIL: $description (expected $expected, got $actual)" >&2
        failures=$((failures + 1))
    fi
}

launch_status()
{
    status=0
    run_check "$@" >/dev/null 2>&1 || status=$?
    echo "$status"
}

expect 'passing probe succeeds' 0 \
    "$(launch_status 30 godot --headless --path "$repository_root/game" res://SmokeHarnessPassProbe.tscn)"
expect 'hung probe fails at the deadline' 124 \
    "$(launch_status 30 godot --headless --path "$repository_root/game" res://SmokeHarnessHangProbe.tscn)"
throw_status=$(launch_status 30 godot --headless --path "$repository_root/game" res://SmokeHarnessThrowProbe.tscn)
expect 'throwing probe fails' nonzero "$([ "$throw_status" -ne 0 ] && echo nonzero || echo zero)"

# Simulated native shutdown aborts: a fake godot on PATH counts its launches and replays a
# scripted exit code and output per attempt.
shim_dir=$(mktemp -d)
trap 'rm -rf "$shim_dir"' EXIT
cat >"$shim_dir/godot" <<'SHIM'
#!/bin/sh
count=$(($(cat "$SHIM_COUNT" 2>/dev/null || echo 0) + 1))
echo "$count" >"$SHIM_COUNT"
eval "output=\${SHIM_OUTPUT_$count-}; code=\${SHIM_CODE_$count-0}"
printf '%s\n' "Godot Engine v4.6.3.stable.mono.official" "$output"
echo "leak probe: ${ALLRPCS_SMOKE_HARNESS_LEAK_CHECK-unset}" >"$SHIM_COUNT.env"
exit "$code"
SHIM
chmod +x "$shim_dir/godot"

shim_case()
{
    rm -f "$shim_dir/count"
    status=0
    PATH="$shim_dir:$PATH" SHIM_COUNT="$shim_dir/count" run_check - godot >/dev/null 2>&1 || status=$?
    echo "$status/$(cat "$shim_dir/count")"
}

export SHIM_OUTPUT_1='mutex lock failed: Invalid argument' SHIM_CODE_1=134 SHIM_OUTPUT_2='done' SHIM_CODE_2=0
expect 'diagnosed mutex abort retries once and passes' 0/2 "$(shim_case)"
export SHIM_OUTPUT_2='mutex lock failed: Invalid argument' SHIM_CODE_2=134
expect 'repeated mutex abort is retried only once' 134/2 "$(shim_case)"
export SHIM_OUTPUT_1='ERROR: check failed
mutex lock failed: Invalid argument'
expect 'mutex abort after a reported error is not retried' 134/1 "$(shim_case)"
export SHIM_OUTPUT_1='mutex lock failed: Invalid argument' SHIM_CODE_1=1
expect 'other exit codes are not retried' 1/1 "$(shim_case)"
export SHIM_OUTPUT_1='ERROR: System.InvalidOperationException: shim probe' SHIM_CODE_1=0
expect 'exception text fails a zero exit' 1/1 "$(shim_case)"

# Per-launch assignments reach the launch but must not persist into later launches; a
# function-prefix assignment would persist under bash's POSIX mode (macOS /bin/sh).
export SHIM_COUNT="$shim_dir/count" SHIM_CODE_1=0 SHIM_OUTPUT_1=done
PATH="$shim_dir:$PATH"
run_check - ALLRPCS_SMOKE_HARNESS_LEAK_CHECK=1 godot >/dev/null 2>&1 || failures=$((failures + 1))
expect 'per-launch assignment reaches the launch' 'leak probe: 1' "$(cat "$shim_dir/count.env")"
expect 'per-launch assignment does not leak' unset "${ALLRPCS_SMOKE_HARNESS_LEAK_CHECK-unset}"

if [ "$failures" -ne 0 ]; then
    echo "FAIL: smoke harness self-test ($failures failed)" >&2
    exit 1
fi
echo 'PASS: smoke harness self-test'
