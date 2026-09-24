# Allrpcs Handover

## Current state

`main` includes merged PR #352 at `1e08b7f`. Branch
`codex/phase-7-fifth-move-binding` advances the exact fifth Move cycle through two bounded seams:
exact result-to-candidate binding and exact generic-result acknowledgement. The reviewed
implementation is committed at `2565ad1` and published in draft PR #353 targeting `main`.

The new path:

- binds only the exact observed fifth-cycle generic Move result to the exact candidate binding
  retained by its guarded author;
- revalidates the complete fifth-cycle composition, observation, task, author binding, source
  candidate, and retained successor graph before binding;
- acknowledges only that same still-current bound result, resets only its completed generic task,
  and clears only the observation; and
- retains the exact result and candidate binding after acknowledgement for later typed composition.

Null, rebuilt-result, wrong-candidate, stale-graph, replaced-authority, and repeated inputs remain
inert. No typed Move request is composed or started; no action is dispatched or applied; the
ordinary turn remains empty; and placement, evidence, earlier model-call counts, and all prior
provenance remain unchanged. No cadence, provider, credential, persistence, UI/input, navigation,
perception, dialogue, timer, or new Domain behavior was added. Ordinary startup remains inert. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both seams were introduced test-first. The focused game build first failed with `CS0246` for the
  absent binding coordinator, then with `CS1061` for the absent acknowledgement surface.
- The focused switch-gated Move recovery proof passes host-local with Godot 4.6.3 Mono and .NET 8,
  including exact fifth-cycle binding and acknowledgement while authoritative placement/evidence
  and all five earlier model-call counts remain unchanged.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, formatting, and the Compatibility launch.
- The sandboxed quality run reached the zero-warning build but VSTest could not bind its loopback
  socket (`SocketException (13): Permission denied`); the identical host-local run passed.
- Session-wide review updated the focused smoke success text to describe the new binding and
  acknowledgement boundary. Complete-diff review found no remaining correctness, exact-reference,
  scene-ordering, public-surface, formatting, line-length, or documentation issues. Whitespace
  validation is clean.
- Draft PR #353's five hosted jobs completed as failures with `steps: []`; no hosted product step
  executed, so this is Actions/account infrastructure state rather than contradictory product-test
  evidence.

## Exact next bounded slice

Compose only one exact typed fifth-cycle Move request from the retained acknowledged result and
candidate binding, stopping before supplying a model or starting typed work.

Acceptance boundary:

1. Test-first add one distinct typed-request composition coordinator over only the exact retained
   acknowledged result, fifth-cycle author request, candidate binding, and current authoritative
   placement and evidence snapshots.
2. Accept only the exact retained result and exact source-owned fifth Move candidate binding for
   canonical `Move`, with the exact guarded author request and still-current successor graph.
3. Reject null, rebuilt-result, rebuilt-binding, foreign-actor, wrong-kind, wrong-candidate,
   stale-authority, stale-graph, and repeated inputs without changing any owner.
4. Produce one fresh `CharacterObjectSabotageMoveConsiderationRequest` retaining the exact actor,
   authoritative placement/evidence snapshots, destination, and only the fifth candidate.
5. Do not supply or consume a typed model, start typed work, observe/apply a typed result, reserve
   the ordinary turn, clear provenance, or alter placement/evidence authority.
6. Extend only the focused switch-gated Move proof. Keep ordinary startup inert.
7. Preserve all existing exclusions: no cadence, timers, provider transport, credentials,
   persistence, UI/input, navigation, perception, dialogue delivery, or new Domain behavior.
8. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to start only that exact typed request on a fresh scene-owned task while preserving its
   queued model across rejected preflight.
