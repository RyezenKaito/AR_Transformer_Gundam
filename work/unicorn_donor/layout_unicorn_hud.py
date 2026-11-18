from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance
import numpy as np, math, random
src=Path('work/unicorn_donor/hud_renders')
out=Path('Assets/Game/Features/Combat/UI/Unicorn/Art')
out.mkdir(parents=True,exist_ok=True)

def extract(key):
    def rgb(suffix):return np.asarray(Image.open(src/(key+suffix+'.png')).convert('RGB'),dtype=np.float32)/255
    lo,hi=rgb('_black'),rgb('_white')
    alpha=np.clip(1-np.median(hi-lo,axis=2),0,1)
    rgb=np.clip(lo/np.maximum(alpha[:,:,None],.005),0,1)
    sprite=Image.fromarray(np.uint8(np.dstack((rgb,alpha))*255),'RGBA')
    return sprite

def fitted(sprite,w,h):
    sprite=sprite.copy()
    sprite.thumbnail((w,h),Image.Resampling.LANCZOS)
    return sprite

def trail(canvas,paths,color,width=4):
    soft=Image.new('RGBA',canvas.size); d=ImageDraw.Draw(soft)
    for path in paths:d.line(path,fill=(*color,202),width=width*4,joint='curve')
    canvas.alpha_composite(soft.filter(ImageFilter.GaussianBlur(width*1.7)))
    crisp=Image.new('RGBA',canvas.size);d=ImageDraw.Draw(crisp)
    for path in paths:
        d.line(path,fill=(*color,240),width=width,joint='curve')
        d.line(path,fill=(235,245,255,245),width=max(1,width//3),joint='curve')
    canvas.alpha_composite(crisp)

def field(canvas,accent,origin,mode):
    x,y=origin; d=ImageDraw.Draw(canvas)
    for r in (48,67,86):
        d.arc((x-r,y-r,x+r,y+r),-27,107,fill=(*accent,116),width=1)
        d.arc((x-r,y-r,x+r,y+r),153,218,fill=(*accent,81),width=1)
    rand=random.Random(mode)
    for i in range(26):
        x=rand.randint(38,340);y=rand.randint(16,183)
        d.line((x,y,x+rand.randint(2,12),y-rand.randint(1,8)),fill=(*accent,rand.randint(35,135)),width=1)

def build(key,mode,accent):
    art=Image.new('RGBA',(384,216))
    field(art,accent,(204,116),mode)
    subject=extract(key)
    if mode==2:subject=subject.crop(subject.getbbox())
    subject=fitted(subject,330 if mode==2 else 365,177 if mode==2 else 225)
    x=(384-subject.width)//2
    y=(216-subject.height)//2 if mode==2 else 216-subject.height+8
    if mode==0:x-=19
    if mode==3:x+=35;y-=9
    if mode in (1,3):
        halo=Image.new('RGBA',art.size)
        shapes=ImageDraw.Draw(halo)
        for k in range(7):
            j=k*16
            shapes.arc((75+j*.6,19+j*.12,316-j*.3,238-j*.11),187+(k%2)*30,327-(k%3)*29,fill=(*accent,105),width=2+k%3)
        art.alpha_composite(halo.filter(ImageFilter.GaussianBlur(1.6)))
    art.alpha_composite(subject,(x,y))
    if mode==0:
        trail(art,[[(175,91),(255,68),(331,47)],[(191,91),(270,76),(347,62)]],accent,4)
    elif mode==1:
        trail(art,[[(94,197),(71,157),(79,124),(57,103)],[(279,202),(310,152),(300,125),(322,97)],
                   [(106,159),(95,140),(103,111)]],accent,3)
    elif mode==2:
        trail(art,[[(276,167),(315,185),(347,196)]],accent,4)
    else:
        trail(art,[[(221,115),(171,139),(116,178),(74,209)],[(220,111),(161,152),(118,201)],[(217,120),(165,145),(69,199)]],accent,6)
    art.save(out/(key+'.png'))

build('Twin_Vulcan',0,(105,223,255))
build('Psycho_Frame',1,(255,47,87))
build('Beam_Magnum',2,(126,202,255))
build('Full_Burst',3,(255,62,93))
portrait=extract('Portrait')
box=portrait.getbbox()
portrait=portrait.crop((max(0,(box[0]+box[2]-412)//2),0,min(portrait.width,(box[0]+box[2]+412)//2),460))
portrait.thumbnail((384,384),Image.Resampling.LANCZOS)
portrait.save(out/'Portrait.png')


