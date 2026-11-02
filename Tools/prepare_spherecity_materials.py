from pathlib import Path
import json,re
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[1]
src=root/'Sources/SphereCity'; dst=root/'Assets/ThirdParty/SphereCity'
out=dst/'URP';out.mkdir(exist_ok=True)
inventory=json.loads((root/'Tools/SphereCityPackageInventory.json').read_text())
ids={x['guid']:x['path'][7:] for x in inventory}
def read_image(path):
    im=Image.open(src/path).convert('RGBA');im.thumbnail((2048,2048))
    return np.asarray(im,dtype=np.float32)/255
def save(name,arr):
    p=out/name;Image.fromarray(np.uint8(np.clip(arr,0,1)*255)).save(p)
    return p.relative_to(root).as_posix()
definitions=[]
for p in (src/'Art/Materials').rglob('*.mat'):
    s=p.read_text(); tex={k:ids.get(g,'') for k,g in re.findall(r'- (\w+):\s+m_Texture: \{fileID: [-\d]+, guid: (\w+)',s)}
    colors={}
    for k,v in re.findall(r'- (\w+): \{r: ([^}]+)\}',s):
        colors[k]=[float(n) for n in re.findall(r'[-+]?\d*\.?\d+(?:e[-+]?\d+)?',v)]
    prefix='Color_b4d3794b8f25453d8ce1f34bc61105eb'
    mask=next((v for v in tex.values() if 'ColormasksAlpha' in v),'')
    packed=next((v for v in tex.values() if 'MetalGlossEmissiveAO' in v),'')
    normal=next((v for v in tex.values() if '_Normal.' in v),'')
    base=next((v for v in tex.values() if any(t in v.lower() for t in ('basecolor','basemap','albedo'))),'')
    d={'name':p.stem,'path':(dst/p.relative_to(src)).relative_to(root).as_posix(),'baseMap':'','metalMap':'','aoMap':'','emissionMap':'','normal':('Assets/ThirdParty/SphereCity/'+normal if normal else '')}
    if mask:
        a=read_image(mask);color=np.ones_like(a[:,:,:3])*np.array(colors.get(prefix+'_3',[.65,.7,.75,1])[:3])
        for i,suffix in enumerate(['','_1','_2']):
            w=a[:,:,i:i+1];color=color*(1-w)+np.array(colors.get(prefix+suffix,[.5,.5,.5,1])[:3])*w
        d['baseMap']=save(p.stem+'_Base.png',color)
    elif base:d['baseMap']='Assets/ThirdParty/SphereCity/'+base
    if packed:
        a=read_image(packed);metal=np.zeros_like(a);metal[:,:,0]=a[:,:,0];metal[:,:,3]=a[:,:,1]
        d['metalMap']=save(p.stem+'_MetalSmooth.png',metal)
        d['aoMap']=save(p.stem+'_AO.png',np.repeat(a[:,:,3:4],3,axis=2))
        emiss=np.repeat(a[:,:,2:3],3,axis=2)*np.array(colors.get(prefix+'_4',[.15,.6,1,1])[:3])
        d['emissionMap']=save(p.stem+'_Emission.png',emiss)
    definitions.append(d)
(dst/'MaterialImport.json').write_text(json.dumps({'materials':definitions},indent=2))
print('Prepared URP definitions:',len(definitions),'generated textures:',len(list(out.glob('*.png'))))

