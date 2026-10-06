# Continuation loop investigation — 2026-10-06

This investigation covers the ten preceding Allrpcs chats, their published handover
revisions and the current interrupted duplicate. It changes continuation routing,
not accepted artwork, runtime scenes or gameplay.

## What failed

1. **Approval fragmentation.** Artwork selection, scope, runtime views and publication
   were repeatedly made into separate stops. Some distinctions were legitimate for
   genuinely new runtime work, but sessions applied them mechanically after broad
   owner acceptance. The owner explicitly objected to the whole Shrine-ground slice
   being revisited, then to a fifth Laundry approval, then to repeated Village Edge
   confirmation. Skill and package prose was being treated as stronger than those
   instructions. Some decisions were also asked twice in a single turn, through an
   input widget and again in the final reply.
2. **The handover remembered only the latest checkpoint.** Pruning replaced the
   completed section with the latest package and a locally invented successor. It
   had no persistent list of closed areas or a requirement that a successor fill
   an identified remaining product gap.
3. **Adjacent studies became a progress loop.** Audit, external comparison, selection,
   scope and runtime steps produced real commits, but then another audit of the same
   accepted geography became the next slice. Repeated capture matrices, complete
   predecessor inventories and validation dominated the continuation instructions.
   A negative result such as "no additional apron" did not retire that workstream.
4. **Historical metadata remained easy to misread.** Immutable package READMEs and
   verification files retain "pending" and old "next" text. Terminal acceptance
   supersedes those records, but the current queue lacked a durable index explaining
   that precedence. Preserving historical evidence is correct; using it as the live
   work queue is not.

## Concrete evidence

| Evidence | Observed failure or completed state |
|---|---|
| Shrine chat `01a10ac4-5eb1-75e1-af11-c735ebd34eea` | Owner: "No, the whole shrine-ground slice. We need to streamline the process a bit it seems". Closure published in `5e5e3cab`. |
| Laundry chat `01a10b90-47a6-7133-9900-31392d2723a7` | Owner: "THis is already the fifth time I approve this. IT IS APPROVED!!! Check your loops and make sure this kind of pointless redundancy does not happen again". Closure published in `0b4d5884`. |
| Village Edge chat `01a10bc8-936b-7750-b845-c9e61c767701` | Owner: "again, confirmed multiple times already. We have to move on!". Runtime closed in `cbc02d06`; forest approach audit in `a6903284` found no need for another apron. |
| Clearing chat `01a10edb-e0bf-71a0-9701-2e9718ebb454` | Owner: "The audit was already done, that slice has been approved. Move on to the next slice." Duplicate audit scaffolding was removed; external ground comparison then published in `ff3259c7`. |
| Latest prior chat `01a10fad-d601-7fb3-8713-ff35ba3725bd` | Owner selected grouped, approved its scope and finally said "confirmed - this can be finished now and we can move on to the next slice". Runtime published in `ea63fcc4`. |
| `ea63fcc4` handover change | Despite that closure, the new successor was another "closed-forest approach dressing feasibility" packet in the same area. It was authored as a new proposal; no separate completed packet for that exact successor was found in the published tree. |
| Current chat `01a11071-ff4b-7123-a100-8be38219df89` | Owner stopped that successor: "we just had done exactly this piece and determined that it is completed and accepted". Only untracked draft scripts/input metadata existed; they were removed. No captures or product edits occurred. This closes automatic return to that study. |

The published commits are not ten copies of identical code: several installed distinct
underlays or dressing. The defect is repeated owner decisions and self-generated
return to completed or low-value adjacent studies, despite repeated instructions to
move forward. The immediate forest successor was a routing error, not repository
corruption or an unfinished accepted installation.

Other inspected chats: `01a10a82-753a-7722-bbe4-3ce12b9af10e` (Wagon),
`01a10a38-9e20-7213-847b-5323692603d3` (Sickhouse),
`01a10970-c498-7a50-838f-edf5d20f5114` (Square),
`01a105b6-ef61-7372-bc51-4e286e312acc` (Garden scope/runtime), and
`01a10578-1b0d-7391-b864-c761e3f3e445` (ground compatibility/Garden selection).

## Repair

- `CONTINUATION.json` persists closed scopes and workstreams, with small evidence
  bindings. It survives handover pruning and records exactly one executable slice.
- `AGENTS.md` requires sessions to reconcile direct owner instructions and the ledger
  before implementation. Historical package queues are no longer actionable.
- `scripts/check-continuation.py` rejects a closed scope, a renamed closed workstream,
  loss of committed closures, missing evidence, a successor without an unfinished
  deliverable, and divergence between the ledger and the handover's active section.
  `--sync` updates only that section. An explicit later owner reopening is recorded
  separately, preserving the original closure.
- The latest forest-study successor is retired. Further automatic ground-apron and
  Clearing/forest dressing studies are parked. This does not declare all terrain,
  accessibility, pacing or the complete opening region finished.
- The new successor is the **complete approved character animation import proof**.
  Current acceptance binds 432 new directional frames plus 48 inherited cloak frames;
  the current production manifest contains no new Healer/companion animation assets.
  Import/registration is a real missing deliverable. Artwork selection stays closed.
  The full set is one slice, rather than repeating the same workflow for nine roles.
- The roadmap now separates completed character artwork production from unfinished
  import/registration. Its old unchecked production item could otherwise trigger
  another attempt to generate and approve the already accepted batch.

Product files, frozen packages and their validators remain unchanged. Verification
for this repair is the continuation checker and its regression probes, documentation
links, reviewed diff and scoped Git publication. A full game suite is not warranted
for these continuation documents and the standalone checker.

Final verification: eleven regression probes pass, including rejection of retired
scope/workstream routing, a lost closure, an invented reopening, changed acceptance
bytes, already-existing outputs and divergent handover text. The actual published
`ea63fcc4` handover is also rejected by the new checker. Changed document links and
Python syntax pass. Source acceptance confirms exactly 432 new directional frames
and 48 inherited frames for the next slice. Review fixed the probe fixture's reliance
on the current queue status so the tests remain usable after the queue advances.
