from PIL import Image, ImageDraw
from pathlib import Path
import json
from math import floor
p=Path('work/unicorn_donor')
tex=Image.open('Assets/Models/rx-0-full-armor-unicorn-gundam/textures/tex_7.png').convert('RGB')
parts=json.loads((p/'shield_atlas.json').read_text())
out=Image.new('RGB',(1500,850),'#696c73')
d=ImageDraw.Draw(out)
for i,polys in enumerate(parts):
    cx=75+150*(i%10)
    cy=75+170*(i//10)
    ys=[v[0][1] for q in polys for v in q]
    zs=[v[0][2] for q in polys for v in q]
    my=(max(ys)+min(ys))/2;mz=(max(zs)+min(zs))/2
    s=125/max(.01,max(ys)-min(ys),max(zs)-min(zs))
    for q in sorted(polys,key=lambda q:sum(v[0][0] for v in q)/len(q)):
        u=sum(v[1][0] for v in q)/len(q);v=sum(v[1][1] for v in q)/len(q)
        color=tex.getpixel((int((u%1)*(tex.width-1)),int((1-v%1)*(tex.height-1))))
        d.polygon([(cx+(pt[1]-my)*s,cy-(pt[2]-mz)*s) for pt,uv in q],fill=color,outline=tuple(int(c*.7) for c in color))
    d.text((cx-9,cy+68),str(i),fill='white')
out.save(p/'parts_cpu.png')
