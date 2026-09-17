# Allrpcs Handover

## Current state

`main` includes merged PR #302 at `8c98b22`. Branch
`codex/phase-7-delay-provider-transport` advances the character-targeted delay chain through three
bounded Infrastructure slices:

- Phase 7mm strictly decodes one bounded target-delay decision JSON document, rebinds value-equal
  provider text and a zero-based index to the exact request actor and candidate, and delegates final
  acceptance to the existing Domain policy.
- Phase 7mn accumulates only one complete ordered OpenAI Responses semantic lifecycle and completes
  framed SSE only after an accepted exact selection or abstention.
- Phase 7mo serializes one stateless streaming structured-output request with bounded trusted actor,
  world-time, event-log, and candidate context plus a closed exact decision schema.

Implementation commit `19024e6` is published in draft PR #303 targeting `main`.

The branch changes only Infrastructure contracts/tests, README, ROADMAP, and this handover. It does
not execute HTTP, access credentials, compose a provider model or binding, change game settings,
add scene nodes or `Main.tscn` wiring, schedule work, apply or recover a delay, author evidence,
persist state, or add input/UI, navigation, movement, perception, dialogue, or other world behavior.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused runs produced the expected missing-type compiler failures before each decoder,
  stream/SSE, and request-serialization implementation was added.
- Focused host-local validation passes 53 target-delay decoder, stream/SSE, and request-serialization
  tests. Sandboxed VSTest was blocked by its known loopback IPC restriction; the identical host-local
  commands provide the passing evidence.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,687 Domain tests, 569 Application tests, 1,081 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact request-instance rebinding, Domain-policy delegation, strict
  JSON shape and length bounds, complete ordered SSE lifecycle enforcement, bounded redacted
  failures, caller cancellation, stable trusted-context indexes, closed schemas and public surfaces,
  unchanged request world state, documentation, file size, line length, formatting, whitespace, and
  scoped paths. No unresolved finding remains.
- Draft PR #303 is open, draft, and mergeable. Its five hosted jobs failed immediately with empty
  step lists, so they are Actions/account infrastructure failures rather than hosted product-test
  evidence; the complete host-local quality result above remains the executable validation.

## Exact next bounded slice

Take **Phase 7mp: execute one target-delay request through a caller-composed OpenAI model and create
its exact model binding without construction-time I/O**.

Acceptance boundary:

1. Test-first add one `ICharacterTargetSabotageDelayDecisionModel` implementation that validates an
   exact caller-owned OpenAI setting, loads its credential before sending, posts the Phase 7mo
   request to the configured endpoint, requires `text/event-stream`, and completes only through the
   Phase 7mn SSE boundary.
2. Preserve caller cancellation and expose bounded redacted failures without response bodies,
   credentials, provider details, partial output, fallback decisions, or authoritative mutation.
3. Add one factory that retains exact settings and creates the existing target-delay binding over
   caller-owned HTTP, credential, and endpoint dependencies without credential access, network I/O,
   or model invocation during construction.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add
   settings composition, scene nodes or `Main.tscn` wiring, automatic scheduling, delay application
   or recovery, evidence, persistence, input/UI, navigation, movement, perception, dialogue, or
   other world behavior.
