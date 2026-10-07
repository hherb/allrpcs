#!/usr/bin/env python3
"""Package imported production captures and persistent-identity slow tracking."""
import json
from PIL import Image, ImageDraw, ImageSequence
from build import DIRECTIONS, OUT, REPO, render, require, sha

SAMPLES = list(range(0,720,45))


def lossless(path, frames, durations):
    """Verify decoded pixel data and actual frame durations in retained WebP loops."""
    frames[0].save(path,save_all=True,append_images=frames[1:],duration=durations,
                   loop=0,lossless=True,quality=100,method=4)
    with Image.open(path) as animation:
        decoded=[];timings=[]
        for frame in ImageSequence.Iterator(animation):
            frame.load();decoded.append(frame.convert('RGBA').copy());timings.append(frame.info['duration'])
    require(len(frames)==len(decoded) and all(a.tobytes()==b.tobytes() for a,b in zip(frames,decoded)),
            'Decoded review differs from source capture')
    require(timings==durations,'Playback timing changed')
    return timings


def main():
    """Retain every pose plus production and diagnostic views of the same cycle."""
    require(not (OUT/'candidate.json').exists(),'Frozen candidate cannot be regenerated')
    review=OUT/'review';review.mkdir(exist_ok=True);timings={}
    for scale in (1,2):
        frames=[Image.open(OUT/f'captures/comparison-{scale}x-{ms:03d}.png').convert('RGBA') for ms in SAMPLES]
        timings[f'old-vs-new-{scale}x.webp']=lossless(review/f'old-vs-new-{scale}x.webp',frames,[45]*16)
        if scale==2:
            sides=[]
            for frame in frames:
                side=frame.crop((494,474,1198,922))
                ImageDraw.Draw(side).text((8,2),'Reference-based natural gait: west / east. Actual Godot import, 2x native.',fill='black')
                sides.append(side)
            timings['production-sides-2x.webp']=lossless(review/'production-sides-2x.webp',sides,[45]*16)
    contacts=Image.new('RGBA',(1280,1272),'#e9e5d9');draw=ImageDraw.Draw(contacts);row=0
    for direction in DIRECTIONS:
        count=16 if direction in ('west','east') else 8
        for index in range(count):
            frame=Image.open(OUT/f'exports/seeker-cloak-{direction}-walk-{index+1:02d}.png').convert('RGBA')
            x=(index%8)*160;y=(row+index//8)*212
            contacts.alpha_composite(frame,(x,y+22))
            draw.text((x+2,y+2),f'{direction} {index+1}: '+('accepted' if count==8 else f'{index*6.25:g}% cycle'),fill='black')
        row+=count//8
    contacts.save(review/'all-48-phases.png')
    sides={direction:render(direction) for direction in ('west','east')};tracking=[]
    for index in range(16):
        board=Image.new('RGBA',(860,460),'#e9e5d9');draw=ImageDraw.Draw(board)
        draw.text((12,8),'Same legs throughout: RED A / BLUE B. Half-speed review of the 720ms cycle.',fill='black')
        for column,direction in enumerate(('west','east')):
            paints,records,_=sides[direction];record=records[index];origin=(70+420*column,66)
            board.alpha_composite(paints[index],origin);sign=1 if direction=='east' else -1
            knees=max(record['legs'],key=lambda l:sign*l['knee_2x'][0])['leg']
            feet=max(record['legs'],key=lambda l:sign*l['foot_centroid_2x'][0])['leg']
            support=' + '.join(record['legs'][i]['leg'] for i in record['supporting_legs'])
            draw.text((30+420*column,30),f'{direction}: {index*6.25:g}% / knee {knees} / foot {feet} / support {support}',fill='black')
            for li in (1,0):
                leg=record['legs'][li];color='#d13d38' if li==0 else '#2879cd'
                points=[(origin[0]+leg[key][0],origin[1]+leg[key][1]) for key in ('hip_2x','knee_2x','ankle_2x')]
                draw.line(points,fill='white',width=6);draw.line(points,fill=color,width=3)
                for key in ('knee_2x','foot_centroid_2x'):
                    x,y=leg[key];x+=origin[0];y+=origin[1]
                    draw.ellipse((x-5,y-5,x+5,y+5),fill=color,outline='white',width=2)
                    draw.text((x+7,y-7),leg['leg'],fill=color,stroke_width=1,stroke_fill='white')
            draw.text((30+420*column,438),'Bent knee advances; trailing foot follows into extension.',fill='black')
        tracking.append(board)
    timings['knee-foot-transfer-slow-2x.webp']=lossless(review/'knee-foot-transfer-slow-2x.webp',tracking,[90]*16)
    (OUT/'evidence/review-timing.json').write_text(json.dumps(timings,indent=2)+'\n')
    report={'status':'passed','source':'32 actual imported Godot captures; native authoring diagnostic clearly marked',
            'production_cycle_ms':720,'slow_cycle_ms':1440,'sample_times_ms':SAMPLES,
            'decoded_rgba_and_durations':'exact source captures and authored durations',
            'files':{str(p.relative_to(REPO)):sha(p) for p in sorted(review.iterdir()) if p.is_file() and p.name not in ('.DS_Store','manifest.json')}}
    (review/'manifest.json').write_text(json.dumps(report,indent=2)+'\n')
    print('PASS: lossless production comparisons, all 48 poses, slow persistent-identity tracking and decoded timing')


if __name__=='__main__':
    main()
