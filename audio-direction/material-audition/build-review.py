#!/usr/bin/env python3
"""Build one local complete-series listening page from actual retained excerpts and measured mixes."""
from pathlib import Path
import hashlib
import html
import json

ROOT = Path(__file__).resolve().parent


def escape(value):
    """Render source metadata as text rather than executable third-party content."""
    return html.escape(str(value), quote=True)


def main():
    """Require the complete series before presenting a collective owner review."""
    candidates = json.loads((ROOT / 'candidates.json').read_text())
    measurements = json.loads((ROOT / 'measurements.json').read_text())
    selection_path = ROOT / 'owner-selection.json'
    owner_selection = json.loads(selection_path.read_text()) if selection_path.exists() else {}
    complete_acceptance = owner_selection.get('status') == 'complete_series_owner_accepted'
    assert len(candidates['candidates']) == 16 and not candidates['missing_source_ids']
    assert len(measurements['final_mix_outputs']) == 3
    plan = json.loads((ROOT.parent / 'material-audition-source-plan.json').read_text())
    mixes = []
    levels = {item['name']: item['integrated_lufs'] for item in measurements['final_mix_outputs']}
    preview_db = levels['default'] - levels['reduced-range']
    for item in measurements['final_mix_outputs']:
        comparisons = item['comparisons_to_provisional_targets']
        deviations = []
        if not comparisons['within_default_loudness_band']:
            deviations.append('Outside Default reference band of −25 to −21 LUFS')
        if not comparisons['within_lra_band']:
            band = comparisons['lra_band']
            deviations.append(f'Outside provisional {band["min"]}–{band["max"]} LU range band')
        description = f"{item['integrated_lufs']:.1f} LUFS · {item['lra_lu']:.1f} LU range · {item['true_peak_dbtp']:.1f} dBTP"
        mixes.append(f'<article><h3>{escape(item["name"].replace("-", " ").title())}</h3>'
                     f'<p class="meter">{description}</p>'
                     f'<p class="meter">{escape("; ".join(deviations))}</p>'
                     f'<audio controls preload="metadata" src="review/{escape(item["name"])}.wav"></audio></article>')
    groups = {'music': 'Optional music', 'regional_bed': 'Place and weather',
              'walking_material': 'Walking materials', 'action_feedback': 'Actions and feedback'}
    sections = []
    for key, title in groups.items():
        clips = []
        for candidate in candidates['candidates']:
            if candidate['group'] != key:
                continue
            status = ('Accepted — quiet flute replacement' if candidate['id'] == 'M02' else 'Accepted — unchanged') if complete_acceptance else 'Accepted — unchanged' if candidate['owner_selected'] else 'Revised flute — listening selection pending' if candidate['id'] == 'M02' and (ROOT / 'accepted-fifteen.json').exists() else 'Listening selection pending'
            clips.append(f'<article id="candidate-{escape(candidate["id"])}"><h3><span>{escape(candidate["id"])}</span> {escape(candidate["title"])}</h3>'
                         f'<p class="meter">{escape(status)}</p>'
                         f'<p class="meter">{candidate["actual_seconds"]:g} seconds · '
                         f'{escape(" / ".join(candidate["audition_buses"]))} · '
                         f'{candidate["sample_peak_dbfs"]:.1f} dBFS peak</p>'
                         f'<audio controls preload="metadata" src="clips/{escape(candidate["id"])}.wav"></audio></article>')
        sections.append(f'<section><h2>{title}</h2><div class="grid">{"".join(clips)}</div></section>')
    timeline = ''.join(f'<li><button data-time="{row["start"]}">{int(row["start"]//60)}:{int(row["start"]%60):02d}'
                       f' {escape(row["region_or_role"])}</button></li>' for row in plan['planned_mix_timeline_seconds'])
    document = '''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Allrpcs — material audio audition</title><style>
:root{color-scheme:dark;font:17px/1.55 system-ui,sans-serif;background:#17231f;color:#e9ece3}body{max-width:1160px;margin:auto;padding:32px 24px 64px}
h1{font:600 clamp(28px,5vw,44px)/1.2 Georgia,serif;margin:12px 0}h2{font:500 26px Georgia,serif;margin:30px 0 14px}h3{font-size:17px;margin:0 0 8px}h3 span{color:#d4ba82}
p{max-width:85ch}.eyebrow,.meter{font-size:13px;color:#bfcbbd}.eyebrow{letter-spacing:.12em;text-transform:uppercase}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(270px,1fr));gap:14px}
article{padding:18px;border:1px solid #47564a;border-radius:8px;background:#22332b}audio{width:100%;height:40px}a{color:#dbc38e}button{font:inherit;color:#dbc38e;background:none;border:1px solid #536557;border-radius:5px;padding:7px 10px;cursor:pointer}button:focus-visible,a:focus-visible{outline:3px solid #f7d37b}ul{list-style:none;padding:0;display:flex;gap:8px;flex-wrap:wrap}.note{padding:18px;border-left:3px solid #c4a466;background:#29352b}footer{margin-top:36px;font-size:14px;color:#bfcbbd}
</style><header><div class="eyebrow">Complete series · listening review pending</div><h1>Quiet, tactile material sounds</h1>
<p>Sixteen source-derived candidates and three versions of the same five-minute sequence. Play starts only when you choose it. Timeline labels are audition examples; these sounds have no world or gameplay assignments.</p>
<p class="note">Numerical checks are recorded. Audible seams, incidental voices or modern noise, material suitability, headphones, speakers and low-volume listening still need human assessment. After listening, send one collective selection or list changes by candidate ID. No selection is recorded yet.</p></header>
<section><h2>Compare the full sequence</h2><div class="grid">MIXES</div><ul>TIMELINE</ul>
<button id="match-range" data-gain-db="PREVIEW_DB">Level-match Reduced range preview (+PREVIEW_DB dB)</button><p id="preview-state" class="meter">Original playback level. Optional preview gain affects listening only; the WAV and its measurements stay unchanged.</p>
<p class="meter">The segment buttons seek all three comparison players. The policy bands remain provisional; measured departures are retained in <a href="measurements.json">measurements</a>. Music-free keeps the same nonmusic tracks and timings.</p></section>
SECTIONS<footer><a href="CREDITS.txt">Source credits and edit notices</a> · <a href="provenance.json">Originals and rights records</a> · <a href="candidates.json">Edit lineage</a> · <a href="measurements.json">Measurements</a>
<p>PCM24 review files, 48 kHz. Godot internally loads these WAV tracks as 16-bit before its final mixer capture. Speech and device intelligibility are unassessed.</p></footer>
<script>const players=[...document.querySelectorAll('audio')];for(const p of players){p.addEventListener('play',()=>{for(const q of players)if(q!==p)q.pause()})}for(const b of document.querySelectorAll('[data-time]')){b.addEventListener('click',()=>{for(const p of document.querySelectorAll('section:first-of-type audio'))p.currentTime=Number(b.dataset.time)})}
const match=document.querySelector('#match-range');let context,gain,matched=false;match.addEventListener('click',async()=>{if(!context){context=new AudioContext();gain=context.createGain();context.createMediaElementSource(document.querySelector('audio[src="review/reduced-range.wav"]')).connect(gain).connect(context.destination)}await context.resume();matched=!matched;gain.gain.value=matched?10**(Number(match.dataset.gainDb)/20):1;match.textContent=matched?'Restore original Reduced range level':`Level-match Reduced range preview (+${match.dataset.gainDb} dB)`;document.querySelector('#preview-state').textContent=matched?`Reduced range preview gain +${match.dataset.gainDb} dB. WAV and measured values unchanged.`:'Original playback level. Optional preview gain affects listening only; the WAV and its measurements stay unchanged.'})</script></html>'''
    if (ROOT / 'accepted-fifteen.json').exists():
        document = document.replace('Complete series · listening review pending', 'Fifteen accepted · M02 flute revision')
        document = document.replace('Numerical checks are recorded. Audible seams, incidental voices or modern noise, material suitability, headphones, speakers and low-volume listening still need human assessment. After listening, send one collective selection or list changes by candidate ID. No selection is recorded yet.',
                                    'Your fifteen accepted candidates remain unchanged. Only M02, rest and healing, has been replaced with a quiet shakuhachi flute excerpt. Its listening selection is pending. Default and Reduced range include the revision; Music-free is unchanged.')
        document = document.replace('<section><h2>Compare the full sequence', 'MUSIC_SECTION<section id="compare-mixes"><h2>Compare the full sequence')
        document = document.replace("section:first-of-type audio", '#compare-mixes audio')
        document = document.replace('MUSIC_SECTION', sections.pop(0))
    if complete_acceptance:
        document = document.replace('Fifteen accepted · M02 flute revision', 'Complete series accepted · Default range selected')
        document = document.replace('Your fifteen accepted candidates remain unchanged. Only M02, rest and healing, has been replaced with a quiet shakuhachi flute excerpt. Its listening selection is pending. Default and Reduced range include the revision; Music-free is unchanged.',
                                    'All sixteen sounds are accepted, including the quiet shakuhachi rest/healing motif. Default range is selected. The other fifteen excerpts remain unchanged; Reduced range and Music-free are retained for comparison.')
    document = document.replace('MIXES', ''.join(mixes)).replace('TIMELINE', timeline).replace('SECTIONS', ''.join(sections)).replace('PREVIEW_DB', f'{preview_db:.1f}')
    (ROOT / 'index.html').write_text(document)
    selection = {
        'schema_version': 1, 'status': 'pending_collective_owner_listening_selection',
        'owner_statement': None, 'selected_ids': None, 'date': None,
        'candidate_manifest_sha256': hashlib.sha256((ROOT / 'candidates.json').read_bytes()).hexdigest(),
        'mix_measurements_sha256': hashlib.sha256((ROOT / 'measurements.json').read_bytes()).hexdigest(),
        'review_html_sha256': hashlib.sha256((ROOT / 'index.html').read_bytes()).hexdigest(),
        'meaning': 'Prepared candidate series; this pending record is not acceptance or runtime authority',
    }
    if not (ROOT / 'owner-selection.json').exists():
        (ROOT / 'owner-selection.json').write_text(json.dumps(selection, indent=2) + '\n')
    print('Built listening page; existing owner decisions preserved')


if __name__ == '__main__':
    main()
