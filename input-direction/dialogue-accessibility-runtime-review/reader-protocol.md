# VoiceOver evaluation protocol

Evaluate only the approved DialogueOverlay on macOS with Godot 4.6.3. Record the
OS, engine and VoiceOver version, date, evaluator and native window size before
an actual assistive session. Do not enable VoiceOver or alter OS settings under
this grant. Use a session already made available by the owner. Keep providers
inactive and use canned fixture text; retain concise outcomes, never user-content
recordings or credentials. Property inspection and synthetic focus are distinct
from heard reader output.

1. From world context press the retained overlay toggle, then traverse Controls,
   Audio, Dialogue and Close settings forward and backward. Confirm focus returns
   to the corresponding entry after Audio and Controls.
2. Activate Dialogue. Hear the Input name, text-entry role and input/status
   descriptions. Traverse Input, Whisper, Submit, Close in both directions.
   Hear checkbox state and the ordinary/whisper delivery meaning.
3. Submit one canned ordinary utterance by Enter and one whisper through Submit.
   Confirm accepted status, single clearing and retained delivery mode. Submit
   empty/whitespace text and confirm rejected status and retained text. Listen
   for polite status delivery without repeated frame announcements.
4. Read the named final Healer response if the existing authoritative owner has
   produced one. Do not infer a response from hidden/partial model output.
5. Close with the button and UI cancel, reopen, and confirm Dialogue entry return.
   Confirm hidden Settings, Audio and Controls siblings are absent as active form
   controls. Confirm world commands stay blocked while GUI focus owns input.
6. Repeat at 1280×720 and 1920×1080, then with one UI focus/cancel remapping.

For each item record observed, failed or not performed and a short evidence note.
A successful single-panel result still does not certify journal, map, provider
settings, other text-heavy UI, Windows/Linux readers or broad accessibility.
