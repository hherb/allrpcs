# Shared POSIX-sh wrapper for headless Godot smoke launches (sourced, not executed).
#
# run_check <deadline-frames|-> [NAME=VALUE...] godot <args...>
#
# A numeric deadline sets ALLRPCS_CHECK_DEADLINE_FRAMES for the launch, which arms the
# CheckDeadline autoload: a check still running after that many frames exits with code 124 and
# prints "FAIL: check deadline of <N> frames reached". "--quit-after $((2 * N + 60))" is appended
# only as a backstop. "-" means no deadline (editor import and --script launches).
#
# Per-launch environment assignments go after the deadline and are applied with env(1), so
# they reach only that launch. (A "NAME=VALUE run_check ..." prefix would persist in the
# calling shell under bash's POSIX mode, which is /bin/sh on macOS.)
#
# A launch fails when it exits nonzero or its output names an unhandled .NET exception: Godot
# .NET logs exceptions escaping _Ready/_Process as "ERROR: <Type>Exception: <message>" and keeps
# running. Only the diagnosed native mutex shutdown abort (exit 134 with "mutex lock failed" and
# no reported error) is retried, exactly once, with the identical command; this mirrors
# native_shutdown() in quality.py.

unset ALLRPCS_CHECK_DEADLINE_FRAMES

smoke_exception_pattern='^ERROR: [A-Za-z_][A-Za-z0-9_.]*Exception: '

# Prints the output of the last Godot launch in the file (text after the last engine banner).
smoke_last_launch()
{
    awk '/Godot Engine v/ { text = "" } { text = text $0 "\n" } END { printf "%s", text }' "$1"
}

smoke_native_shutdown()
{
    [ "$1" -eq 134 ] || return 1
    smoke_last_launch "$2" | grep -q 'mutex lock failed' || return 1
    ! smoke_last_launch "$2" | grep -qE 'ERROR:|AssertionError|Unhandled exception'
}

smoke_launch()
{
    smoke_launch_status=0
    if [ "$smoke_deadline" = - ]; then
        env "$@" 2>&1 || smoke_launch_status=$?
    else
        env ALLRPCS_CHECK_DEADLINE_FRAMES="$smoke_deadline" "$@" \
            --quit-after $((2 * smoke_deadline + 60)) 2>&1 || smoke_launch_status=$?
    fi
    echo "$smoke_launch_status" >"$smoke_status_file"
}

run_check()
{
    smoke_deadline=$1
    shift
    case $smoke_deadline in
        -) ;;
        '' | 0 | *[!0-9]*)
            echo "FAIL: run_check deadline must be a positive frame count or -: $smoke_deadline" >&2
            return 2
            ;;
    esac
    smoke_output=$(mktemp)
    smoke_status_file=$(mktemp)
    smoke_attempt=1
    while :; do
        smoke_launch "$@" | tee "$smoke_output"
        smoke_status=$(cat "$smoke_status_file")
        if [ "$smoke_attempt" -eq 1 ] && smoke_native_shutdown "$smoke_status" "$smoke_output"; then
            echo "RETRY identical smoke launch after diagnosed native shutdown: $*" >&2
            smoke_attempt=2
            continue
        fi
        break
    done
    if [ "$smoke_status" -eq 0 ] && grep -qE "$smoke_exception_pattern" "$smoke_output"; then
        echo "FAIL: smoke launch logged an unhandled exception: run_check $smoke_deadline $*" >&2
        smoke_status=1
    elif [ "$smoke_status" -ne 0 ]; then
        echo "FAIL: smoke launch exited $smoke_status: run_check $smoke_deadline $*" >&2
    fi
    rm -f "$smoke_output" "$smoke_status_file"
    return "$smoke_status"
}
