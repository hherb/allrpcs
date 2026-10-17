# Shared POSIX-sh wrapper for headless Godot smoke launches (sourced, not executed).
#
# run_check <deadline-frames|-> [NAME=VALUE...] godot <args...>
#
# A numeric deadline sets ALLRPCS_CHECK_DEADLINE_FRAMES for the launch, which arms the
# CheckDeadline autoload: a check still running after that many frames exits with code 124 and
# prints "FAIL: check deadline of <N> frames reached". "--quit-after $((2 * N + 60))" is appended
# only as a backstop. "-" means no deadline (editor import and --script launches). A deadline
# must be a decimal integer from 1 to 1073741793 without leading zeros, so the shell and
# CheckDeadline (which parses an Int32) agree on it and the backstop still fits Godot's Int32
# --quit-after (2 * 1073741793 + 60 = 2147483646); anything else returns 2.
#
# Per-launch environment assignments go after the deadline and are applied with env(1), so
# they reach only that launch. (A "NAME=VALUE run_check ..." prefix would persist in the
# calling shell under bash's POSIX mode, which is /bin/sh on macOS.)
#
# A launch fails when it exits nonzero or its output names an unhandled .NET exception or a
# GDScript runtime error: Godot logs exceptions escaping _Ready/_Process as
# "ERROR: <Type>Exception: <message>" and GDScript errors as "SCRIPT ERROR: <message>", and in
# both cases keeps running and can still exit 0. Such a launch returns 3 (smoke_exception_status),
# distinct from the deadline code 124 and from a check's own failure exit. Only the diagnosed
# native mutex shutdown abort (exit 134 with "mutex lock failed" and no reported error) is
# retried, exactly once, with the identical command; this mirrors native_shutdown() in quality.py.
#
# Quarantine: when smoke_quarantine_file names a file, a launch whose launch id (see
# smoke_launch_id) is listed there still runs, but its result is reported as
# "QUARANTINED (#<issue>)" and never fails the caller. smoke_summary prints the final counts and
# fails when a quarantine entry matched no launch.

unset ALLRPCS_CHECK_DEADLINE_FRAMES

smoke_exception_pattern='^(ERROR: [A-Za-z_][A-Za-z0-9_.]*Exception: |SCRIPT ERROR: )'
smoke_exception_status=3
smoke_quarantine_file=${smoke_quarantine_file-}
smoke_passed=0
smoke_quarantined=0
smoke_quarantine_seen=

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

# Accepts "-" or a decimal frame count in 1..1073741793 written without leading zeros.
smoke_valid_deadline()
{
    case $1 in
        -) return 0 ;;
        '' | 0* | *[!0-9]*) return 1 ;;
    esac
    [ "${#1}" -le 10 ] && [ "$1" -le 1073741793 ]
}

# Prints the launch id of run_check's arguments (after the deadline): the leading NAME=VALUE
# assignments, every res:// argument, and the file name of the --script argument, in order and
# separated by single spaces. Examples:
#   ALLRPCS_OPENING_REGION_OLD_SHRINE_CHECK=route
#   ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 ALLRPCS_CHARACTER_MODEL_SETTINGS_PANEL_CHECK=1
#   res://DirectionAImportProof.tscn
#   check-closed-forest-runtime.gd
smoke_launch_id()
{
    smoke_id=
    smoke_in_assignments=1
    smoke_previous=
    for smoke_argument in "$@"; do
        smoke_word=
        if [ "$smoke_in_assignments" -eq 1 ]; then
            case $smoke_argument in
                [A-Za-z_]*=*) smoke_word=$smoke_argument ;;
                *) smoke_in_assignments=0 ;;
            esac
        fi
        case $smoke_argument in
            res://*) smoke_word=$smoke_argument ;;
        esac
        if [ "$smoke_previous" = --script ]; then
            smoke_word=${smoke_argument##*/}
        fi
        if [ -n "$smoke_word" ]; then
            smoke_id="${smoke_id:+$smoke_id }$smoke_word"
        fi
        smoke_previous=$smoke_argument
    done
    printf '%s\n' "$smoke_id"
}

# Prints the issue reference ("#123") quarantining the launch id, if listed. Returns 2 for a
# malformed quarantine file.
smoke_quarantine_issue()
{
    [ -n "$smoke_quarantine_file" ] || return 0
    awk -v id="$1" -v file="$smoke_quarantine_file" '
        /^[[:space:]]*(#|$)/ { next }
        {
            if ($NF !~ /^#[0-9]+$/ || NF < 2) {
                print "FAIL: malformed quarantine entry in " file ": " $0 > "/dev/stderr"
                bad = 1
                next
            }
            entry = $0
            sub(/[[:space:]]+#[0-9]+[[:space:]]*$/, "", entry)
            sub(/^[[:space:]]+/, "", entry)
            if (entry == id) { print $NF }
        }
        END { exit bad ? 2 : 0 }
    ' "$smoke_quarantine_file"
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
    if ! smoke_valid_deadline "$smoke_deadline"; then
        echo "FAIL: run_check deadline must be - or a frame count from 1 to 1073741793" \
            "without leading zeros: $smoke_deadline" >&2
        return 2
    fi
    smoke_launch_key=$(smoke_launch_id "$@")
    smoke_issue=
    if [ -n "$smoke_launch_key" ]; then
        smoke_issue=$(smoke_quarantine_issue "$smoke_launch_key") || return 2
    fi
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
        echo "FAIL: smoke launch logged an unhandled exception or script error:" \
            "run_check $smoke_deadline $*" >&2
        smoke_status=$smoke_exception_status
    elif [ "$smoke_status" -ne 0 ]; then
        echo "FAIL: smoke launch exited $smoke_status: run_check $smoke_deadline $*" >&2
    fi
    rm -f "$smoke_output" "$smoke_status_file"
    if [ -n "$smoke_issue" ]; then
        smoke_quarantined=$((smoke_quarantined + 1))
        smoke_quarantine_seen="$smoke_quarantine_seen
$smoke_launch_key"
        if [ "$smoke_status" -eq 0 ]; then
            echo "QUARANTINED ($smoke_issue): passed; remove it from the quarantine list:" \
                "$smoke_launch_key" >&2
        else
            echo "QUARANTINED ($smoke_issue): failed with $smoke_status (not counted):" \
                "$smoke_launch_key" >&2
        fi
        return 0
    fi
    if [ "$smoke_status" -eq 0 ]; then
        smoke_passed=$((smoke_passed + 1))
    fi
    return "$smoke_status"
}

# Prints "smoke: <n> passed, <q> quarantined". Fails when a quarantine entry matched no launch,
# so a renamed or removed launch cannot leave a stale entry behind.
smoke_summary()
{
    smoke_stale=0
    if [ -n "$smoke_quarantine_file" ]; then
        smoke_seen_file=$(mktemp)
        printf '%s\n' "$smoke_quarantine_seen" >"$smoke_seen_file"
        smoke_stale_entries=$(awk -v seen_file="$smoke_seen_file" '
            FILENAME == seen_file { seen[$0] = 1; next }
            /^[[:space:]]*(#|$)/ { next }
            {
                entry = $0
                sub(/[[:space:]]+#[0-9]+[[:space:]]*$/, "", entry)
                sub(/^[[:space:]]+/, "", entry)
                if (!(entry in seen)) { print entry }
            }
        ' "$smoke_seen_file" "$smoke_quarantine_file")
        rm -f "$smoke_seen_file"
        if [ -n "$smoke_stale_entries" ]; then
            printf 'FAIL: quarantine entry matched no launch: %s\n' "$smoke_stale_entries" >&2
            smoke_stale=1
        fi
    fi
    echo "smoke: $smoke_passed passed, $smoke_quarantined quarantined"
    return "$smoke_stale"
}
