#!/usr/bin/env python3
"""Mechanically separate generated structure modules, package masters and review boards.

No repainting or runtime integration; generated source sheets remain unchanged.
"""
from collections import deque
from io import BytesIO
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
FONT_PATH = '/System/Library/Fonts/Supplemental/Arial.ttf'
FONT = ImageFont.truetype(FONT_PATH, 22)
SMALL = ImageFont.truetype(FONT_PATH, 17)
FAMILIES = {
    'village': ['Village cottage', 'Meeting/store cottage', 'Market stall', 'Plaster wall', 'Door and doorstep', 'Fence and gate'],
    'sickhouse': ['Sickhouse exterior', 'Roofless sickhouse shell', 'Detached roof', 'Window wall', 'Open entrance', 'Shuttered window'],
    'garden': ['Apothecary hut', 'Potting shed', 'Pergola', 'Empty raised bed', 'Garden fence and gate', 'Well'],
    'shrine': ['Sheltered shrine', 'Open shrine shelter', 'Stone altar', 'Steps and landing', 'Standing stone', 'Enclosure entrance'],
    'laundry': ['Laundry shelter', 'Stone washing trough', 'Drying rack', 'Washing line', 'Wash tub and board', 'Timber landing'],
    'wagon': ['Covered wagon', 'Abandoned wagon', 'Detached canopy', 'Wagon wheel', 'Broken side panel', 'Roadside shelter'],
    'woodland': ['Forester cabin', 'Woodland lean-to', 'Lookout platform', 'Forest gate', 'Timber fence', 'Plank footbridge'],
}
HEIGHTS = {
    'village': [500,520,280,200,200,140], 'sickhouse': [560,300,250,220,210,130],
    'garden': [450,340,320,75,160,180], 'shrine': [400,380,100,80,260,120],
    'laundry': [340,100,230,250,95,55], 'wagon': [260,230,160,140,110,340],
    'woodland': [450,330,480,240,200,110],
}
# Pixel crop regions follow the generated spacing; no source pixels repainted.
CELLS = {
    'village': [(0,0,500,550),(480,0,1110,550),(1110,0,1536,550),
                (20,570,510,990),(560,550,970,990),(980,620,1536,990)],
    'sickhouse': [(0,0,525,548),(525,0,1048,548),(1048,90,1536,480),
                  (40,549,515,1000),(550,549,1020,1000),(1050,560,1536,1000)],
    'garden': [(0,0,525,590),(530,100,1020,580),(1030,0,1536,554),
               (20,630,510,1010),(520,640,1060,1010),(1100,558,1480,1010)],
    'shrine': [(20,20,510,560),(520,20,1030,555),(1040,190,1520,550),
               (20,590,550,990),(580,550,980,1000),(995,560,1536,990)],
    'laundry': [(0,0,558,547),(560,110,1060,500),(1080,40,1536,520),
                (0,526,560,965),(600,560,990,960),(1040,550,1536,970)],
    'wagon': [(0,0,535,540),(530,80,1040,515),(1045,80,1536,515),
              (60,545,460,990),(540,550,995,1000),(1000,525,1536,1000)],
    'woodland': [(0,30,575,585),(580,140,1080,582),(1060,10,1536,600),
                 (10,586,530,970),(525,600,1020,980),(1030,600,1536,1010)],
}


def separate(crop):
    """Exclude neighboring modules by retaining the largest connected alpha component."""
    w, h = crop.size
    mask = bytearray(bool(a) for a in crop.getchannel('A').tobytes())
    largest = []
    for k in range(w*h):
        if not mask[k]:
            continue
        mask[k] = 0
        queue, component = deque([k]), []
        while queue:
            v = queue.popleft()
            component.append(v)
            y, x = divmod(v, w)
            neighbors = ([v-1] if x else []) + ([v+1] if x<w-1 else [])
            neighbors += ([v-w] if y else []) + ([v+w] if y<h-1 else [])
            for n in neighbors:
                if mask[n]:
                    mask[n] = 0
                    queue.append(n)
        if len(component) > len(largest):
            largest = component
    selector = bytearray(w*h)
    for v in largest:
        selector[v] = 255
    return Image.composite(crop, Image.new('RGBA',crop.size), Image.frombytes('L',crop.size,bytes(selector)))


def png_bytes(im):
    buf = BytesIO()
    im.save(buf,format='PNG')
    return buf.getvalue()


def write_master(im,path,name):
    xml = Element('image',w=str(im.width),h=str(im.height),name=name,version='0.0.3')
    stack = SubElement(xml,'stack')
    for title,src in [('Corrections','data/corrections.png'),('Source paint','data/source.png')]:
        SubElement(stack,'layer',name=title,src=src,x='0',y='0',opacity='1.0',visibility='visible',
                   **{'composite-op':'svg:src-over'})
    with zipfile.ZipFile(path,'w') as out:
        out.writestr('mimetype','image/openraster',compress_type=zipfile.ZIP_STORED)
        out.writestr('stack.xml',tostring(xml))
        out.writestr('data/source.png',png_bytes(im))
        out.writestr('data/corrections.png',png_bytes(Image.new('RGBA',im.size)))
        out.writestr('mergedimage.png',png_bytes(im))


def main():
    for folder in ['masters','exports','review']:
        (ROOT/folder).mkdir(exist_ok=True)
    assets,sprites = [],{}
    for family,names in FAMILIES.items():
        sheet = Image.open(ROOT/'sources'/f'{family}.png').convert('RGBA')
        board = Image.new('RGB',(1536,1160),'#e8e3d5')
        draw = ImageDraw.Draw(board)
        draw.text((24,14),f'{family.upper()} — structures batch 01',fill='#1a201d',font=FONT)
        draw.text((24,45),'Enlarged detail review; game-scale arrangements shown separately.',fill='#454a40',font=SMALL)
        for i,name in enumerate(names):
            row,col = divmod(i,3)
            cell = CELLS.get(family,[(c*sheet.width//3,r*sheet.height//2,(c+1)*sheet.width//3,(r+1)*sheet.height//2)
                                    for r in range(2) for c in range(3)])[i]
            crop = sheet.crop(cell)
            if not (family == 'garden' and i == 4):
                crop = separate(crop)
            bounds = crop.getchannel('A').getbbox()
            if bounds is None:
                raise ValueError(f'Empty module: {family}-{i+1:02}')
            paint = crop.crop(bounds)
            wanted = HEIGHTS[family][i]
            width = max(1,round(paint.width*wanted/paint.height))
            master = paint.resize((width*2,wanted*2),Image.Resampling.LANCZOS)
            export = master.resize((width,wanted),Image.Resampling.LANCZOS)
            asset_id = f'{family}-{i+1:02}'
            master_path,export_path = ROOT/'masters'/f'{asset_id}.ora',ROOT/'exports'/f'{asset_id}.png'
            write_master(master,master_path,name)
            export.save(export_path)
            sprites[asset_id] = export
            assets.append({'id':asset_id,'name':name,'source':f'sources/{family}.png','source_cell':list(cell),
                           'source_alpha_bounds_in_cell':list(bounds),'master':f'masters/{asset_id}.ora',
                           'export':f'exports/{asset_id}.png','size':list(export.size),
                           'proposed_pivot':[width//2,wanted-1],'pivot_status':'provisional',
                           'sha256':hashlib.sha256(export_path.read_bytes()).hexdigest()})
            preview = paint.copy()
            preview.thumbnail((464,445),Image.Resampling.LANCZOS)
            x,y = col*512,88+row*530
            board.paste(preview,(x+(512-preview.width)//2,y+(445-preview.height)//2),preview)
            draw.text((x+18,y+482),f'{asset_id}: {name}',fill='#1a201d',font=SMALL)
        board.save(ROOT/'review'/f'{family}.png')
    for composition,placements in {
        'garden-courtyard':[('garden-01',290,640),('garden-03',890,470),('garden-04',810,620),
                            ('garden-05',600,680),('garden-06',1090,660)],
        'roadside-woodland':[('woodland-01',285,640),('wagon-02',680,670),('woodland-04',1050,330),
                              ('woodland-05',630,455),('woodland-02',1040,690)],
    }.items():
        canvas = Image.new('RGBA',(1280,720),'#8b9568')
        for asset_id,x,y in sorted(placements,key=lambda p:p[2]):
            sprite = sprites[asset_id]
            canvas.alpha_composite(sprite,(x-sprite.width//2,y-sprite.height))
        draw = ImageDraw.Draw(canvas)
        draw.text((24,18),f'{composition} / proposed game-scale arrangement',fill='#1a201d',font=FONT)
        draw.rectangle((550,65,710,238),fill='#e8e3d5')
        draw.line([(564,79),(564,223)],fill='#454a40',width=4)
        draw.text((576,102),'144 px\ncharacter\nheight',fill='#1a201d',font=SMALL)
        canvas.convert('RGB').save(ROOT/'review'/f'{composition}.png')
    approved = (ROOT/'owner-acceptance.json').exists()
    (ROOT/'inventory.json').write_text(json.dumps({'status':'owner-approved artwork' if approved else 'awaiting collective artwork review',
        'generator':'built-in image_gen','asset_count':len(assets),
        'validation':'See validation.json; runtime integration remains separate.' if approved else 'deferred until owner feedback',
        'notes':'Original source paint retained; raster ORA correction layers blank; scale/pivots and module fit provisional.',
        'assets':assets},indent=2)+'\n')
    print(f'Packaged {len(assets)} modules, editable 2x masters, seven family boards and two scale compositions.')


if __name__ == '__main__':
    main()
