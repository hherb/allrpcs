#!/bin/sh
# Self-test for scripts/smoke-lib.sh: a passing launch passes, a hung launch fails with the
# deadline code 124, a launch that logs an unhandled exception (from _Ready or from an awaited
# async continuation) or a GDScript error fails with smoke_exception_status even though Godot
# exits 0, only the diagnosed native mutex shutdown abort is retried, exactly once, invalid
# deadlines are rejected, quarantined launches report without failing, and ALLRPCS_SMOKE_CONTINUE=1
# records every failure and fails the summary instead of stopping at the first.

set -eu

# The self-test proves the default fail-fast behavior; collect-all mode is enabled explicitly below.
unset ALLRPCS_SMOKE_CONTINUE

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
# The throw probes quit with 0, so only the exception pattern can produce this status (never the
# deadline's 124).
expect 'exception code is distinct from the deadline code' distinct \
    "$([ "$smoke_exception_status" -ne 124 ] && [ "$smoke_exception_status" -ne 0 ] && echo distinct || echo same)"
expect 'throwing probe fails through the exception pattern' "$smoke_exception_status" \
    "$(launch_status 30 godot --headless --path "$repository_root/game" res://SmokeHarnessThrowProbe.tscn)"
expect 'async-throwing probe fails through the exception pattern' "$smoke_exception_status" \
    "$(launch_status 30 godot --headless --path "$repository_root/game" res://SmokeHarnessAsyncThrowProbe.tscn)"

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
echo "$*" >"$SHIM_COUNT.args"
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
expect 'exception text fails a zero exit' "$smoke_exception_status/1" "$(shim_case)"
export SHIM_OUTPUT_1='SCRIPT ERROR: Out of bounds get index '"'3'"' (on base: '"'Array'"')'
expect 'GDScript runtime error fails a zero exit' "$smoke_exception_status/1" "$(shim_case)"

# Per-launch assignments reach the launch but must not persist into later launches; a
# function-prefix assignment would persist under bash's POSIX mode (macOS /bin/sh).
export SHIM_COUNT="$shim_dir/count" SHIM_CODE_1=0 SHIM_OUTPUT_1=done
PATH="$shim_dir:$PATH"
run_check - ALLRPCS_SMOKE_HARNESS_LEAK_CHECK=1 godot >/dev/null 2>&1 || failures=$((failures + 1))
expect 'per-launch assignment reaches the launch' 'leak probe: 1' "$(cat "$shim_dir/count.env")"
expect 'per-launch assignment does not leak' unset "${ALLRPCS_SMOKE_HARNESS_LEAK_CHECK-unset}"

# Deadlines must be "-" or 1..1073741793 without leading zeros (CheckDeadline parses an Int32 and
# the 2 * N + 60 backstop must fit Godot's Int32 --quit-after); anything else is rejected with 2
# before Godot starts.
for deadline in '' 0 012 abc 1x 1073741794 2147483647 2147483648 99999999999; do
    rm -f "$shim_dir/count"
    status=0
    run_check "$deadline" godot >/dev/null 2>&1 || status=$?
    expect "deadline '$deadline' is rejected" 2/none \
        "$status/$(cat "$shim_dir/count" 2>/dev/null || echo none)"
done
rm -f "$shim_dir/count"
expect 'largest deadline is accepted' 0 "$(launch_status 1073741793 godot)"
expect 'largest deadline backstop fits an Int32' '--quit-after 2147483646' \
    "$(sed -n 's/.*\(--quit-after [0-9-]*\).*/\1/p' "$shim_dir/count.args")"

expect 'launch id keeps assignments, scenes and script names' \
    'A=1 B_CHECK=route res://Probe.tscn check-probe.gd' \
    "$(smoke_launch_id A=1 B_CHECK=route godot --path /x/game res://Probe.tscn --script /x/check-probe.gd)"

# Quarantined launches still run and report, but never fail; counts feed the summary line, and an
# entry that matches no launch fails the summary.
smoke_quarantine_file="$shim_dir/quarantine.txt"
printf '%s\n' '# comment' '' 'ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 #12' \
    'ALLRPCS_SMOKE_HARNESS_STALE_CHECK=1 #13' >"$smoke_quarantine_file"
smoke_passed=0 smoke_quarantined=0 smoke_quarantine_seen=
export SHIM_CODE_1=1 SHIM_OUTPUT_1='quarantined failure'
status=0
rm -f "$shim_dir/count"
report=$(run_check - ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 godot 2>&1 >/dev/null) || status=$?
expect 'failing quarantined launch does not fail' 0 "$status"
expect 'failing quarantined launch is reported' yes \
    "$(printf '%s' "$report" | grep -q '^QUARANTINED (#12): failed' && echo yes || echo no)"
export SHIM_CODE_1=0 SHIM_OUTPUT_1=done
rm -f "$shim_dir/count"
report=$(run_check - ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 godot 2>&1 >/dev/null) || status=$?
expect 'passing quarantined launch asks for removal' yes \
    "$(printf '%s' "$report" | grep -q '^QUARANTINED (#12): passed; remove it' && echo yes || echo no)"
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 godot >/dev/null 2>&1
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_OTHER_CHECK=1 godot >/dev/null 2>&1
expect 'quarantined launches are counted apart' '1 passed, 1 quarantined' \
    "$smoke_passed passed, $smoke_quarantined quarantined"
status=0
summary=$(smoke_summary 2>&1) || status=$?
expect 'stale quarantine entry fails the summary' 1 "$status"
expect 'summary names the stale entry and the counts' yes \
    "$(printf '%s' "$summary" | grep -q 'matched no launch: ALLRPCS_SMOKE_HARNESS_STALE_CHECK=1' \
        && printf '%s' "$summary" | grep -q '^smoke: 1 passed, 1 quarantined$' && echo yes || echo no)"
printf '%s\n' 'ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 missing-issue' >"$smoke_quarantine_file"
status=0
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_QUARANTINE_CHECK=1 godot >/dev/null 2>&1 || status=$?
expect 'malformed quarantine entry is rejected' 2 "$status"
smoke_quarantine_file=

# Collect-all mode: failing launches are recorded and return 0, the summary lists each with its
# status and fails; with no failures the summary passes. Without the mode the summary keeps its
# two-count line (checked above) and a failure is returned at once (checked by the shim cases).
smoke_passed=0 smoke_quarantined=0 smoke_quarantine_seen= smoke_failed=0 smoke_failures=
export ALLRPCS_SMOKE_CONTINUE=1
export SHIM_CODE_1=0 SHIM_OUTPUT_1=done
rm -f "$shim_dir/count"
expect 'collect-all passing launch succeeds' 0 "$(launch_status - ALLRPCS_SMOKE_HARNESS_OK_CHECK=1 godot)"
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_OK_CHECK=1 godot >/dev/null 2>&1
status=0
summary=$(smoke_summary 2>&1) || status=$?
expect 'collect-all summary without failures passes' 0 "$status"
expect 'collect-all summary counts failures' 'smoke: 1 passed, 0 failed, 0 quarantined' "$summary"
export SHIM_CODE_1=7 SHIM_OUTPUT_1='first failure'
status=0
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_FIRST_CHECK=1 godot >/dev/null 2>&1 || status=$?
expect 'collect-all failing launch continues' 0 "$status"
export SHIM_CODE_1=0 SHIM_OUTPUT_1='ERROR: System.InvalidOperationException: second failure'
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_SECOND_CHECK=1 godot res://Second.tscn >/dev/null 2>&1 || status=$?
expect 'collect-all exception launch continues' 0 "$status"
rm -f "$shim_dir/count"
status=0
run_check 0 godot >/dev/null 2>&1 || status=$?
expect 'collect-all still rejects an invalid deadline' 2 "$status"
status=0
summary=$(smoke_summary 2>&1) || status=$?
expect 'collect-all summary with failures fails' 1 "$status"
expect 'collect-all summary lists both failures and the counts' yes \
    "$(printf '%s\n' "$summary" | grep -qx 'FAIL: exit 7: ALLRPCS_SMOKE_HARNESS_FIRST_CHECK=1' \
        && printf '%s\n' "$summary" | grep -qx \
            "FAIL: exit $smoke_exception_status: ALLRPCS_SMOKE_HARNESS_SECOND_CHECK=1 res://Second.tscn" \
        && printf '%s\n' "$summary" | grep -qx 'smoke: 1 passed, 2 failed, 0 quarantined' \
        && echo yes || echo no)"
unset ALLRPCS_SMOKE_CONTINUE
smoke_passed=0 smoke_quarantined=0 smoke_quarantine_seen= smoke_failed=0 smoke_failures=
export SHIM_CODE_1=5 SHIM_OUTPUT_1='fail-fast failure'
status=0
rm -f "$shim_dir/count"
run_check - ALLRPCS_SMOKE_HARNESS_FIRST_CHECK=1 godot >/dev/null 2>&1 || status=$?
expect 'without collect-all a failure is returned' 5 "$status"
status=0
summary=$(smoke_summary 2>&1) || status=$?
expect 'without collect-all the summary keeps two counts' '0/smoke: 0 passed, 0 quarantined' \
    "$status/$summary"

if [ "$failures" -ne 0 ]; then
    echo "FAIL: smoke harness self-test ($failures failed)" >&2
    exit 1
fi
echo 'PASS: smoke harness self-test'
