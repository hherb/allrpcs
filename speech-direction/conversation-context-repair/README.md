# Conversation context repair

Owner-authorized successor for bounded multi-turn conversation context.
The existing physical qualification was suspended unchanged during this repair;
its previous selection is retained in `physical-slice.json`.

History uses captured hearing audiences, retains exact speech and speaker identities,
and includes only the acting character’s completed examination result. History is
session-local, bounded, and cleared on successful save restoration. This slice does
not add conversation persistence or change action execution.

Completion requires privacy/budget/load tests, ordinary Main multi-turn checks, an
actual selected-model follow-up, full current quality, review and publication.

## Runtime contract

`DialogueEventController` records successful world dialogue events with their captured
hearing audiences. Player input and committed model replies use this same owner.
`CharacterConversationHistory` retains at most 128 occurrences / 64,000 UTF-16 code
units. Each request receives the newest heard/own speech, up to 24 utterances /
12,000 code units, in chronological order. Unheard speech is filtered before the
request budget is applied. The current event is excluded by reference, so repeated
words on separate turns remain valid history and the current prompt appears once.

Historical entries retain exact text, speaker identity and delivery mode. Ollama
and OpenAI serialize those entries as chronological native user/assistant messages,
using the responding character identity to select the role. Trusted context keeps
message-index/speaker/delivery metadata without duplicating the speech text. Shared
provider instructions treat historical speech as untrusted in-world dialogue,
including earlier model replies; engine context establishes action outcomes. The
assembler reserves the current utterance, authoritative state and affordances
before fitting older speech within the existing 128-entry / 32,000-unit context
ceiling. Old speech is dropped as whole utterances, newest first retained.

Only the acting character receives the latest completed examination result as an
explicit `action-outcome` entry, and
only while its exact physical occurrence remains in the current transcript.
This does not invent collection, inventory, observations for bystanders, pending
results or outcomes of failed/cancelled actions. The herb execution implementation
and its frozen receipts remain unchanged.

A successful complete save restoration clears session history and examination
context. A rejected load preserves history. The pending pre-load request is
invalidated, so its eventual model response cannot become new speech in the
restored world. History survives ordinary dialogue close/reopen within the session;
it is not added to the save schema. Existing saved commitments remain available.
No personal dialogue or audio is recorded by this verification package; fixtures
use fictional text and isolated settings, without opening the microphone.

## Checks

- Application tests cover speaker/whisper metadata, exact repeated occurrences,
  heard/own filtering, both storage limits, request limits, authoritative-context
  reservation, exact committed examination ownership and immutable projections.
- Infrastructure tests inspect actual Ollama JSON for speaker metadata and quoted
  untrusted history instructions.
- `ConversationContextMainCheck.tscn` exercises ordinary submitted turns through
  Main and captures the actual HTTP request: two/three exchanges, private speech,
  a second character, real examination travel/result, invalid load, successful
  full save restoration during generation, stale reply rejection and new dialogue.
- The final controlled Main result is retained from the required root quality stage.
  The selected `gemma4:e4b` local model passed 35 checks across five submissions:
  earlier-name recall on two follow-ups, silent examination execution, and an
  accurate safety answer that also retained the instruction to leave the herbs. Full quality and final
  evidence are recorded in `verification.json`.

Reproduce with isolated configuration and a writable report path:

```sh
ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED=1 \
ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL=0 \
ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE=1 \
ALLRPCS_SPEECH_CONFIG=/private/tmp/allrpcs-speech-missing.json \
ALLRPCS_PLAYER_SPEECH_CONFIG=/private/tmp/allrpcs-asr-missing.json \
ALLRPCS_CONVERSATION_MODE=controlled \
ALLRPCS_CONVERSATION_REPORT=/private/tmp/allrpcs-conversation-controlled.json \
godot --headless --path game --quit-after 10800 res://ConversationContextMainCheck.tscn
```

Use `ALLRPCS_CONVERSATION_MODE=live` for the selected local model, with Ollama
already running, and a separate report path. No online provider is needed.
Root `./quality.sh` includes the controlled Main check as a required stage.

The current generation-input smoke fixture now requires the current utterance only
in the explicit current-dialogue entry, rather than duplicating it as recent speech,
and excludes the new lifecycle override from its source API reflection. Its actor,
knowledge, perception, affordance, memory and no-mutation assertions remain. Frozen
validators and historical receipts are unchanged and run on their pinned trees.

Exploratory local-model diagnostics are retained under `diagnostics/`: an opening
statement did not produce a generation (exact decision was not logged), nearby
Sage interjections invalidated a two-speaker fixture assumption, and some ambiguous
follow-ups elicited clarification. An untyped memory outcome was supplied correctly
but the local model still asked to examine the patch; the explicit `action-outcome`
kind and completion instructions correct this in the final live trial. The final fixture positions the healer outside
the Sage's hearing range, and uses explicit context-dependent questions. The final
serialization sends historical conversation as native chat messages rather than
placing historical speech text inside a system context JSON object.

The first full gate stopped at the older dialogue event controller API reflection
fixture after 34 completed stages. History is now internal, and the current fixture
permits Godot ready/exit callbacks while preserving its occurrence/hearing/state
assertions. The exact two-file correction, failed log, 18,911 input hashes and
checkpoint-specific recovery guard are retained under `quality/attempt1` and
`finish-quality.py`. The guard passed its source/correction/receipt drift probes.
It reuses only the unchanged first 21 historical/art passes, reruns continuation
and every current product, full scene and runtime check. The driver intentionally
rejects later ledger/publication state. Frozen validators remain unchanged.
Some existing scene fixtures emit ObjectDB shutdown warnings; the new conversation
and examination fixtures must exit without that warning.

One recovery attempt stopped after `product-9` without a terminal receipt; neither
a matching process nor its execution session remained. Its 31 completed stages and
logs are retained under `quality/interrupted`, without inferring a product failure
or a successful full gate. The same guarded recovery was repeated in an attached
terminal with periodic session waits. Final quality evidence comes only from the
completed run under `quality/final`.

Final verification: 217 root stages and 5,657 unit tests passed, with no skipped unit tests.
Controlled Main passed 63 assertions; the selected local model passed 35 checks.
The repair is complete and closed; the original human trial resumes with its existing statuses.
